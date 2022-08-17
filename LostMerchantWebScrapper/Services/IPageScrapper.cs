using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostMerchantWebScrapper.Services
{
    public interface IPageScrapper
    {
        public Task<HtmlDocument> LoadFromWebAsync(string url);
    }
}
