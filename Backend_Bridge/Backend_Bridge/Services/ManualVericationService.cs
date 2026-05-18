using Backend_Bridge.Data;
using Backend_Bridge.Models;

namespace Backend_Bridge.Services
{
    public class ManualVericationService
    {
        private readonly ApplicationDbContext _context;
        public ManualVericationService(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Register(string action, string description, int? orderId = null)
        {
            var manual = new ManualReviewTransaction
            {
                OrderId = orderId,
                Description = description,
                ActionType= action,
                CreatedAt = DateTime.Now
            };

            _context.ManualReviewTransactions.Add(manual);
            _context.SaveChanges();
        }
    }
}
