using Backend_Bridge.Constants;
using Backend_Bridge.Data;
using Backend_Bridge.Hubs;
using Backend_Bridge.Models;
using Backend_Bridge.Services.Payments;
using Microsoft.AspNetCore.SignalR;

namespace Backend_Bridge.Services
{
    public class PaymentValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditLogService _auditLogService;
        private readonly ManualVericationService _manualVericationService;
        private readonly RiskScoringService _riskScoringService;
        private readonly IHubContext<PaymentNotificationHub> _hubContext;
        private readonly FraudAuditService _fraudAuditService;
        private readonly OrderLookupService _orderLookupService;
        private readonly PaymentRuleValidator _paymentRuleValidator;
        private readonly PaymentRecordService _paymentRecordService;
        private readonly PaymentNotificationService _paymentNotificationService;
        private readonly PaymentQueryService _paymentQueryService;

        public PaymentValidationService(
            ApplicationDbContext context,
            AuditLogService auditLogService,
            ManualVericationService manualVericationService,
            RiskScoringService riskScoringService,
            IHubContext<PaymentNotificationHub> hubContext,
            FraudAuditService fraudAuditService,
            OrderLookupService orderLookupService,
            PaymentRuleValidator paymentRuleValidator,
            PaymentRecordService paymentRecordService,
            PaymentNotificationService paymentNotificationService,
            PaymentQueryService paymentQueryService)
        {
            _context = context;
            _auditLogService = auditLogService;
            _manualVericationService = manualVericationService;
            _riskScoringService = riskScoringService;
            _hubContext = hubContext;
            _fraudAuditService = fraudAuditService;
            _orderLookupService = orderLookupService;
            _paymentRuleValidator = paymentRuleValidator;
            _paymentRecordService = paymentRecordService;
            _paymentNotificationService = paymentNotificationService;
            _paymentQueryService = paymentQueryService;
        }

        public IEnumerable<FraudAttempt> GetFraudLogs()
        {
            return _paymentQueryService.GetFraudLogs();
        }

        public IEnumerable<Payment> GetPayments()
        {
            return _paymentQueryService.GetPayments();
        }

        public object GetPaymentsDetails()
        {
            return _paymentQueryService.GetPaymentsDetails();
        }

        public void ExpirePendingOrders()
        {
            _orderLookupService.ExpirePendingOrders();
        }

        public (bool IsValid, string Message) ValidateReference(string reference)
        {
            if (!_context.Payments.Any(p => p.Reference == reference))
                return (true, "Referencia válida.");

            _fraudAuditService.Register(
                reference,
                0,
                "Referencia duplicada",
                "REFERENCIA_DUPLICADA",
                "Se detectó un intento de pago con una referencia ya utilizada."
            );

            _paymentRecordService.RegisterRejectedDuplicateReference(reference);

            return (false, "La referencia ya fue utilizada.");
        }

        public async Task<(bool IsValid, string Message)> ValidateAmount(
            decimal amount,
            string payerName,
            string reference,
            string customerPhone)
        {
            var order = _orderLookupService.FindPendingOrder(payerName);

            if (order == null)
                return RegisterAdvancedPaymentOrRejectExpiredOrder(amount, payerName, reference, customerPhone);

            var errors = _paymentRuleValidator.Validate(order, amount, reference, customerPhone);

            if (errors.Any())
                return await RejectSuspiciousPayment(order, amount, reference, customerPhone, errors);

            return await ConfirmPayment(order, amount, reference, customerPhone);
        }

        private (bool IsValid, string Message) RegisterAdvancedPaymentOrRejectExpiredOrder(
            decimal amount,
            string payerName,
            string reference,
            string customerPhone)
        {
            var expiredValidation = _orderLookupService.ValidateExpiredOrder(payerName, reference);

            if (!expiredValidation.IsValid)
                return expiredValidation;

            _paymentRecordService.RegisterAdvancedPayment(amount, reference, customerPhone);

            return (true, "Pago adelantado registrado correctamente.");
        }

        private async Task<(bool IsValid, string Message)> RejectSuspiciousPayment(
            Order order,
            decimal amount,
            string reference,
            string customerPhone,
            List<string> errors)
        {
            order.Status = OrderStatuses.Suspected;

            _auditLogService.Register(
                "ORDEN_SUSPENDIDA",
                "La orden fue suspendida por fallos de validación. Debe crear la orden nuevamente.",
                reference,
                order.Id
            );

            var risk = _riskScoringService.Evaluate(errors, amount);
            _context.Risks.Add(risk);
            _context.SaveChanges();

            _paymentRecordService.RegisterRejectedPayment(order, amount, reference, customerPhone, risk, errors);

            await _paymentNotificationService.SendTransactionNotificationAsync(
                OrderStatuses.Suspected,
                amount,
                reference,
                order.Id
            );

            var errorMessage = string.Join(" | ", errors);

            _manualVericationService.Register(OrderStatuses.Suspected, errorMessage, order.Id);

            return (
                false,
                "La orden fue suspendida por errores de validación. Debe crear la orden nuevamente. Errores: " + errorMessage
            );
        }

        private async Task<(bool IsValid, string Message)> ConfirmPayment(
            Order order,
            decimal amount,
            string reference,
            string customerPhone)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                order.Status = OrderStatuses.Paid;

                _paymentRecordService.RegisterApprovedPayment(order, amount, reference, customerPhone);

                _auditLogService.Register(
                    "PAGO_CONFIRMADO",
                    "El pago fue asociado correctamente con la orden y la orden fue marcada como pagada.",
                    reference,
                    order.Id
                );

                await _hubContext.Clients.Group($"order-{order.Id}")
                    .SendAsync("PaymentConfirmed", new
                    {
                        orderId = order.Id,
                        amount,
                        senderNumber = customerPhone,
                        reference,
                        message = "Pago confirmado correctamente."
                    });

                transaction.Commit();

                await _paymentNotificationService.SendTransactionNotificationAsync(
                    OrderStatuses.Paid,
                    amount,
                    reference,
                    order.Id
                );

                return (true, "Pago confirmado correctamente.");
            }
            catch
            {
                transaction.Rollback();
                return (false, "Ocurrió un error al registrar el pago.");
            }
        }
    }
}
