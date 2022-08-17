using LostMerchantWebScrapper.Models;

namespace LostMerchantWebScrapper.Services
{
    public interface ILostArkDescriptor
    {
        Task<Rarity?> GetRarityAsync(string rapportOrCard);
    }
}
