using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace LostMerchantWebScrapper.Services
{
    internal class LostArkStatusService : ILostArkStatusService
    {
        private readonly IPageScrapper _pageScrapper;
        private readonly IMemoryCache _cache;
        private readonly ISystemClock _systemClock;
        private MerchantCheckerOption _option;

        public LostArkStatusService(
            IPageScrapper pageScrapper,
            IMemoryCache cache,
            ISystemClock systemClock,
            IOptionsMonitor<MerchantCheckerOption> option)
        {
            _pageScrapper = pageScrapper;
            _cache = cache;
            _systemClock = systemClock;
            _option = option.CurrentValue;
            option.OnChange((option, key) => _option = option);
        }

        public Task<bool> IsThaemineRunning()
        {
            var entry = _cache.GetOrCreateAsync("server-status", OnCreate);

            return entry;
        }

        private async Task<bool> OnCreate(ICacheEntry entry)
        {
            const string thaemineServerStatusNode = "//main/section/div/div[5]/div[3]/div[27]/div[1]/div";
            const string runningStatusClass = "status--good";

            entry.AbsoluteExpiration = _systemClock.UtcNow + _option.LookupStatusExpiry;
            var document = await _pageScrapper.LoadFromWebAsync(_option.ServerStatusUrl);

            var node = document.DocumentNode.SelectSingleNode(thaemineServerStatusNode);

            if (node == null)
            {
                return false;
            }

            // HtmlAgilityPack has issues with value inside class.
            var isRunning = node.OuterHtml.Contains(runningStatusClass);
            return isRunning;
        }
    }
}
