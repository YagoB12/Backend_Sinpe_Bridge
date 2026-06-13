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
            var senderPhone = ExtractSenderPhone(messageBody);
            var reference = ExtractReference(messageBody);

            ValidateParsedData(amount, payerName, reference);

            return new ParsedSmsDto
            {
                Amount = amount,
                PayerName = payerName,
                SenderPhone = senderPhone,
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
                @"(?:₡|CRC\s?)\s?([\d.,]+)|(?:recibido|recibió)\s+([\d.,]+)\s+colones",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                throw new FormatException("No se pudo extraer el monto del SMS.");
            }

            var rawAmount = match.Groups[1].Success
                ? match.Groups[1].Value.Trim()
                : match.Groups[2].Value.Trim();

            rawAmount = NormalizeAmount(rawAmount);

            if (!decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
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
                @"(?:de|por parte de)\s+([A-Za-zÁÉÍÓÚáéíóúÑñ\s]+?)(?=\.\s*(?:plata|Referencia)|\.|,|Ref|Referencia|Comprobante|$)",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                throw new FormatException("No se pudo extraer el nombre del pagador del SMS.");
            }

            return match.Groups[1].Value.Trim();
        }

        private string ExtractSenderPhone(string messageBody)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                throw new ArgumentException("El mensaje SMS no puede estar vacío.");
            }

            var match = Regex.Match(
                messageBody,
                @"\b(?:plata|telefono|tel[eé]fono|celular)\s*-?\s*(\d{8})\b",
                RegexOptions.IgnoreCase
            );

            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
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

        private static string NormalizeAmount(string rawAmount)
        {
            rawAmount = rawAmount.Trim();

            if (rawAmount.Contains('.') && rawAmount.Contains(','))
            {
                return rawAmount.LastIndexOf(',') > rawAmount.LastIndexOf('.')
                    ? rawAmount.Replace(".", "").Replace(",", ".")
                    : rawAmount.Replace(",", "");
            }

            if (rawAmount.Contains(','))
            {
                var decimals = rawAmount.Length - rawAmount.LastIndexOf(',') - 1;
                return decimals == 2
                    ? rawAmount.Replace(",", ".")
                    : rawAmount.Replace(",", "");
            }

            return rawAmount;
        }
    }
}
