namespace LostMerchantWebScrapper.Services
{
    public interface IFileProvider
    {
        Stream OpenRead(string path);
    }
}