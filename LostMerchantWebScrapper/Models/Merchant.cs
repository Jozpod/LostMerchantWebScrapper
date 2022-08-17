namespace LostMerchantWebScrapper.Models
{
    public class Merchant
    {
        public string Name { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public IEnumerable<string> Zones { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<Card> Cards { get; set; } = Enumerable.Empty<Card>();

        public IEnumerable<Rapport> Rapports { get; set; } = Enumerable.Empty<Rapport>();
    }
}
