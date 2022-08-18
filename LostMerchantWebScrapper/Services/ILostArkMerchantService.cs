using LostMerchantWebScrapper.Models;

namespace LostMerchantWebScrapper.Services
{
    public interface ILostArkMerchantService
    {
        Task<List<MerchantEntry>> GetEntriesAsync(
            string? url = null,
            bool setLocalStorage = true,
            CancellationToken cancellationToken = default);
    }
}
