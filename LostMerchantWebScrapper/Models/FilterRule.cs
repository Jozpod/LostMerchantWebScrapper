
namespace LostMerchantWebScrapper.Models
{
    public class FilterRule
    {
        public FilterRuleType Type { get; set; }

        public string? Name { get; set; }
        
        public Rarity? Rarity { get; set; }

        public enum FilterRuleType
        {
            Rapport,
            Card
        }
    }
}
