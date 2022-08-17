using LostMerchantWebScrapper.Models;

namespace LostMerchantWebScrapper
{
    public class MerchantCheckerOption
    {
        public TimeSpan FindButtonTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan ProbeDelay { get; set; }

        public TimeSpan LookupStatusExpiry { get; set; }

        public string Region { get; set; } = string.Empty;

        public string Server { get; set; } = string.Empty;

        public string LostMerchantsUrl { get; set; } = string.Empty;

        public string ServerStatusUrl { get; set; } = string.Empty;

        public List<FilterRule> Rules { get; set; } = new List<FilterRule>();
    }
}
