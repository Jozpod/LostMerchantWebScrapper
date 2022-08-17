using HtmlAgilityPack;
using LostMerchantWebScrapper.Services;
using System.IO;
using System.Threading.Tasks;

namespace LostMerchantWebScrapperTests
{
    internal class TestPageScrapper : IPageScrapper
    {
        private readonly string _filePath;

        public TestPageScrapper(string filePath)
        {
            _filePath = filePath;
        }

        public Task<HtmlDocument> LoadFromWebAsync(string url)
        {
            var document = new HtmlDocument();
            var path = Path.Combine("Data", _filePath);
            document.Load(path);
            return Task.FromResult(document);
        }
    }
}
