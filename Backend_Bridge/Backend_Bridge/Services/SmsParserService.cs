using System.Globalization;
using System.Text.RegularExpressions;
using Backend_Bridge.DTOs;
using Backend_Bridge.Services.Interfaces;

namespace Backend_Bridge.Services
{
    public class SmsParserService : ISmsParserService
    {
        public ParsedSmsDto Parse(string messageBody)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                throw new ArgumentException("El mensaje SMS no puede estar vacío.");
            }

            var amount = ExtractAmount(messageBody);
            var payerName = ExtractPayerName(messageBody);
            var reference = ExtractReference(messageBody);

            ValidateParsedData(amount, payerName, reference);

            return new ParsedSmsDto
            {
                Amount = amount,
                PayerName = payerName,
                Reference = reference
            };
        }

        private decimal ExtractAmount(string messageBody)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                throw new ArgumentException("El mensaje SMS no puede estar vacío.");
            }

            var match = Regex.Match(
                messageBody,
                @"(?:₡|CRC\s?)\s?([\d,]+(?:\.\d{1,2})?)",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                throw new FormatException("No se pudo extraer el monto del SMS.");
            }

            var rawAmount = match.Groups[1].Value.Replace(",", "").Trim();

            if (!decimal.TryParse(rawAmount, out var amount))
            {
                throw new FormatException("El monto extraído no tiene un formato válido.");
            }

            return amount;
        }

        private string ExtractPayerName(string messageBody)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                throw new ArgumentException("El mensaje SMS no puede estar vacío.");
            }

            var match = Regex.Match(
                messageBody,
                @"(?:de|por parte de)\s+([A-Za-zÁÉÍÓÚáéíóúÑñ\s]+?)(?=\.|,|Ref|Referencia|Comprobante|$)",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                throw new FormatException("No se pudo extraer el nombre del pagador del SMS.");
            }

            return match.Groups[1].Value.Trim();
        }

        private string ExtractReference(string messageBody)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                throw new ArgumentException("El mensaje SMS no puede estar vacío.");
            }

            var match = Regex.Match(
                messageBody,
                @"\b\d{20,30}\b"
            );

            if (!match.Success)
            {
                throw new FormatException("No se pudo extraer la referencia SINPE de 25 dígitos.");
            }

            return match.Value;
        }
        private void ValidateParsedData(decimal amount, string payerName, string reference)
        {
            if (amount <= 0)
            {
                throw new FormatException("El monto extraído no es válido.");
            }

            if (string.IsNullOrWhiteSpace(payerName))
            {
                throw new FormatException("El nombre del pagador no es válido.");
            }

            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new FormatException("La referencia SINPE no es válida.");
            }

            if (!Regex.IsMatch(reference, @"^\d{20,30}$"))
            {
                throw new FormatException("La referencia SINPE debe contener exactamente 25 dígitos.");
            }
        }
    }
}