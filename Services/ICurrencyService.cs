namespace TechMoveGLMS.Web.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();

        Task<decimal> ConvertUsdToZarAsync(decimal amountUsd);
    }
}
