using HtmlAgilityPack;

namespace LostMerchantWebScrapper.Services
{
    internal class PageScrapper : IPageScrapper
    {
        public Task<HtmlDocument> LoadFromWebAsync(string url)
        {
            var client = new HtmlWeb();
            var document = client.LoadFromWebAsync(url);
            return document;
        }
    }
}
