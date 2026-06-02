using Backend_Bridge.Models;

namespace Backend_Bridge.Services
{
    public class RiskScoringService
    {
        public Risk Evaluate(List<string> errors, decimal amount)
        {
            int score = 0;

            // cada error suma puntos
            score += errors.Count * 25;

            // monto alto = más riesgo
            if (amount > 1000) score += 20;
            if (amount > 5000) score += 40;

            // clasificación final
            string level =
                score >= 70 ? "HIGH" :
                score >= 40 ? "MEDIUM" :
                "LOW";

            return new Risk
            {
                Level = level,
                Description = $"Score: {score}"
            };
        }

    }
}
