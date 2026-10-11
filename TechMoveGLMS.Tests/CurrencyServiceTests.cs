using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetUsdToZarRateAsync_ReturnsZarRateFromApiResponse()
        {
            const string json =
                """
                {
                    "amount": 1.0,
                    "base": "USD",
                    "date": "2026-10-11",
                    "rates": {
                        "ZAR": 18.25
                    }
                }
                """;

            using var httpClient = CreateHttpClient(json);
            var service = new CurrencyService(httpClient);

            var rate = await service.GetUsdToZarRateAsync();

            Assert.Equal(18.25m, rate);
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_ConvertsAmountUsingApiRate()
        {
            const string json =
                """
                {
                    "amount": 1.0,
                    "base": "USD",
                    "date": "2026-10-11",
                    "rates": {
                        "ZAR": 18.25
                    }
                }
                """;

            using var httpClient = CreateHttpClient(json);
            var service = new CurrencyService(httpClient);

            var result = await service.ConvertUsdToZarAsync(100m);

            Assert.Equal(1825.00m, result);
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_RoundsToTwoDecimalPlacesAwayFromZero()
        {
            const string json =
                """
                {
                    "amount": 1.0,
                    "base": "USD",
                    "date": "2026-10-11",
                    "rates": {
                        "ZAR": 1.005
                    }
                }
                """;

            using var httpClient = CreateHttpClient(json);
            var service = new CurrencyService(httpClient);

            var result = await service.ConvertUsdToZarAsync(1m);

            Assert.Equal(1.01m, result);
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_ReturnsZero_WhenAmountIsZero()
        {
            const string json =
                """
                {
                    "amount": 1.0,
                    "base": "USD",
                    "date": "2026-10-11",
                    "rates": {
                        "ZAR": 18.25
                    }
                }
                """;

            using var httpClient = CreateHttpClient(json);
            var service = new CurrencyService(httpClient);

            var result = await service.ConvertUsdToZarAsync(0m);

            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_Throws_WhenZarRateIsMissing()
        {
            const string json =
                """
                {
                    "amount": 1.0,
                    "base": "USD",
                    "date": "2026-10-11",
                    "rates": {}
                }
                """;

            using var httpClient = CreateHttpClient(json);
            var service = new CurrencyService(httpClient);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetUsdToZarRateAsync());

            Assert.Equal(
                "The USD to ZAR exchange rate could not be retrieved.",
                exception.Message);
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_Throws_WhenApiReturnsFailureStatus()
        {
            const string json = "{}";

            using var httpClient = CreateHttpClient(
                json,
                HttpStatusCode.ServiceUnavailable);

            var service = new CurrencyService(httpClient);

            await Assert.ThrowsAsync<HttpRequestException>(
                () => service.GetUsdToZarRateAsync());
        }

        private static HttpClient CreateHttpClient(
            string responseJson,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new FakeHttpMessageHandler(
                responseJson,
                statusCode);

            return new HttpClient(handler);
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _responseJson;
            private readonly HttpStatusCode _statusCode;

            public FakeHttpMessageHandler(
                string responseJson,
                HttpStatusCode statusCode)
            {
                _responseJson = responseJson;
                _statusCode = statusCode;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(
                        _responseJson,
                        Encoding.UTF8,
                        "application/json")
                };

                return Task.FromResult(response);
            }
        }
    }
}