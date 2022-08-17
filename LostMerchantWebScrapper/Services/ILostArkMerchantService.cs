using LostMerchantWebScrapper.Models;

namespace LostMerchantWebScrapper.Services
{
    public interface ILostArkMerchantService
    {
        Task<List<MerchantEntry>> GetEntriesAsync();
    }
}
