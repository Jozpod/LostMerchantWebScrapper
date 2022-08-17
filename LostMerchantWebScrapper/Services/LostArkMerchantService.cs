using LostMerchantWebScrapper.Models;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace LostMerchantWebScrapper.Services
{
    internal class LostArkMerchantService : ILostArkMerchantService
    {
        private readonly IWebDriver _webDriver;
        private readonly IJavaScriptExecutor _javaScriptExecutor;
        private readonly ILostArkDescriptor _lostArkDescriptor;
        private readonly ILogger _logger;
        private MerchantCheckerOption _option;

        public LostArkMerchantService(
            IWebDriver webDriver,
            IJavaScriptExecutor javaScriptExecutor,
            ILostArkDescriptor lostArkDescriptor,
            ILogger<ILostArkMerchantService> logger,
            IOptionsMonitor<MerchantCheckerOption> option
            )
        {
            _webDriver = webDriver;
            _javaScriptExecutor = javaScriptExecutor;
            _lostArkDescriptor = lostArkDescriptor;
            _logger = logger;
            _option = option.CurrentValue;
            option.OnChange((option, key) => _option = option);
        }

        private void SetLocalStorageItem(string key, string value)
        {
            _javaScriptExecutor.ExecuteScript($"window.localStorage.setItem('{key}', '{value}');");
        }

        public async Task<List<MerchantEntry>> GetEntriesAsync()
        {
            _webDriver.Navigate().GoToUrl(_option.LostMerchantsUrl);

            SetLocalStorageItem(nameof(_option.Region), _option.Region);
            SetLocalStorageItem(nameof(_option.Server), _option.Server);

            var tableNodeXPath = By.XPath(@"//*[@id=""app""]/div/div[2]/table[1]/tbody");

            var wait = new WebDriverWait(_webDriver, _option.FindButtonTimeout);
            IWebElement? tableElement = null;
            var list = new List<MerchantEntry>();

            try
            {
                tableElement = wait.Until(ExpectedConditions.ElementIsVisible(tableNodeXPath));
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Could not find table");
                return list;
            }

            await ComputeEntriesAsync(tableElement, list);

            return list;
        }

        private async Task ComputeEntriesAsync(IWebElement tableElement, List<MerchantEntry> list)
        {
            var trNode = By.TagName("tr");
            var rows = tableElement.FindElements(trNode);

            foreach (var row in rows)
            {
                var tdNode = By.TagName("td");
                var cells = row.FindElements(tdNode);

                if(cells.Count != 6)
                {
                    _logger.LogDebug("Invalid number of rows");
                    break;
                }

                var rapport = cells[4].Text;

                var entry = new MerchantEntry
                {
                    Name = cells[0].Text,
                    Region = cells[1].Text,
                    Zone = cells[2].Text,
                    Card = cells[3].Text,
                    Rapport = rapport,
                    RapportRarity = await _lostArkDescriptor.GetRarityAsync(rapport),
                    Votes = int.Parse(cells[5].Text),
                };

                list.Add(entry);
            }
        }
    }
}
