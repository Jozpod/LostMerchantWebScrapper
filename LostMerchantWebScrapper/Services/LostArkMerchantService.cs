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

        public async Task<List<MerchantEntry>> GetEntriesAsync(
            string? url = null,
            bool setLocalStorage = true,
            CancellationToken cancellationToken = default)
        {
            url ??= _option.LostMerchantsUrl;

            _webDriver.Navigate().GoToUrl(url);

            if (setLocalStorage)
            {
                SetLocalStorageItem(nameof(_option.Region), _option.Region);
                SetLocalStorageItem(nameof(_option.Server), _option.Server);
            }

            var reTry = true;
            var list = new List<MerchantEntry>();

            while (reTry)
            {
                try
                {
                    reTry = false;

                    var tableNodeXPath = By.XPath(@"//*[@id=""app""]/div/div[2]/table[1]/tbody");
                    var wait = new WebDriverWait(_webDriver, _option.FindButtonTimeout);
                    IWebElement? tableElement = null;

                    try
                    {
                        tableElement = wait.Until(ExpectedConditions.ElementIsVisible(tableNodeXPath), cancellationToken);
                    }
                    catch (WebDriverTimeoutException)
                    {
                        _logger.LogWarning("Could not find table");
                        return list;
                    }

                    await ComputeEntriesAsync(tableElement, list);

                }
                catch (StaleElementReferenceException)
                {
                    _logger.LogWarning("Stale reference. Retrying...");
                    reTry = true;
                    list = new List<MerchantEntry>();
                }
            }

            return list;
        }

        private async Task ComputeEntriesAsync(IWebElement tableElement, ICollection<MerchantEntry> list)
        {
            var trNode = By.TagName("tr");
            var rows = tableElement.FindElements(trNode);

            foreach (var row in rows)
            {
                var tdNode = By.TagName("td");
                var cells = row.FindElements(tdNode);

                if (cells.Count == 1)
                {
                    continue;
                }

                if (cells.Count != 7)
                {
                    _logger.LogDebug("Invalid number of rows");
                    break;
                }

                var zone = cells[2].Text;
                zone = zone == "?" ? null : zone;
                var card = cells[3].GetAttribute("innerText");
                card = card == "?" ? null : card;
                var rapport = cells[4].GetAttribute("innerText");
                rapport = rapport == "?" ? null : rapport;
                var votes = cells[5].Text;
                votes = string.IsNullOrEmpty(votes) ? "0" : votes;
                var rapportRarity = string.IsNullOrEmpty(rapport) ? null : await _lostArkDescriptor.GetRarityAsync(rapport);

                var entry = new MerchantEntry
                {
                    Name = cells[0].Text,
                    Region = cells[1].Text,
                    Zone = zone,
                    Card = card,
                    Rapport = rapport,
                    RapportRarity = rapportRarity,
                    Votes = int.Parse(votes),
                };

                list.Add(entry);
            }
        }

        private void SetLocalStorageItem(string key, string value)
        {
            _javaScriptExecutor.ExecuteScript($"window.localStorage.setItem('{key}', '{value}');");
        }
    }
}
