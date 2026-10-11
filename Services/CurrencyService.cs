using System.Text.Json;

namespace TechMoveGLMS.Web.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            const string apiUrl =
                "https://api.frankfurter.app/latest?from=USD&to=ZAR";

            using var response =
                await _httpClient.GetAsync(apiUrl);

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content.ReadAsStringAsync();

            var exchangeData =
                JsonSerializer.Deserialize<ExchangeRateResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (exchangeData?.Rates == null ||
                !exchangeData.Rates.TryGetValue(
                    "ZAR",
                    out decimal exchangeRate))
            {
                throw new InvalidOperationException(
                    "The USD to ZAR exchange rate could not be retrieved.");
            }

            return exchangeRate;
        }

        public async Task<decimal> ConvertUsdToZarAsync(
            decimal amountUsd)
        {
            var exchangeRate =
                await GetUsdToZarRateAsync();

            return Math.Round(
                amountUsd * exchangeRate,
                2,
                MidpointRounding.AwayFromZero);
        }

        private class ExchangeRateResponse
        {
            public decimal Amount { get; set; }

            public string Base { get; set; } = string.Empty;

            public string Date { get; set; } = string.Empty;

            public Dictionary<string, decimal> Rates { get; set; }
                = new();
        }
    }
}
