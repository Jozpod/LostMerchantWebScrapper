namespace LostMerchantWebScrapper.Models
{
    public class MerchantEntry
    {
        public string? Name { get; set; }

        public string? Region { get; set; }

        public string? Zone { get; set; }

        public string? Card { get; set; }

        public string? Rapport { get; set; }

        public Rarity? RapportRarity { get; set; }

        public int Votes { get; set; }
    }
}
