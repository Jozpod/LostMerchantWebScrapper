namespace LostMerchantWebScrapper.Services
{
    public class DefaultFileProvider : IFileProvider
    {
        public Stream OpenRead(string path) => File.OpenRead(path);
    }
}