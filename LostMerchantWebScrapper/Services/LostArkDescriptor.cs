using LostMerchantWebScrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LostMerchantWebScrapper.Services
{
    public class LostArkDescriptor : ILostArkDescriptor
    {
        private readonly IFileProvider _fileProvider;
        private IDictionary<string, Merchant> _merchantsDict;
        private IDictionary<string, Rarity> _rapportRarityMap;
        private IDictionary<string, Rarity> _cardRarityMap;
        private bool _loaded;

        private static IDictionary<string, Rarity> EmptyDict = new Dictionary<string, Rarity>();

        public LostArkDescriptor(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _merchantsDict = new Dictionary<string, Merchant>();
            _rapportRarityMap = EmptyDict;
            _cardRarityMap = EmptyDict;
        }

        public async Task LoadAsync()
        {
            if (_loaded)
            {
                return;
            }

            var filePath = Path.Combine("Data", "merchants.json");
            var stream = _fileProvider.OpenRead(filePath);
            var serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            _merchantsDict = await JsonSerializer.DeserializeAsync<IDictionary<string, Merchant>>(stream, serializerOptions)
                ?? throw new NullReferenceException();

            var merchants = _merchantsDict
                .Select(pr => pr.Value)
                .ToList();

            _rapportRarityMap = merchants
                .SelectMany(pr => pr.Rapports)
                .GroupBy(pr => pr.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(pr => pr.Key, pr => pr.First().Rarity);

            _cardRarityMap = merchants
                .SelectMany(pr => pr.Cards)
                .GroupBy(pr => pr.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(pr => pr.Key, pr => pr.First().Rarity);

            _loaded = true;
        }

        public async Task<Rarity?> GetRarityAsync(string rapportOrCard)
        {
            await LoadAsync();
            var hasKey = _rapportRarityMap.TryGetValue(rapportOrCard, out var rarity);

            if (hasKey)
            {
                return rarity;
            }

            hasKey = _cardRarityMap.TryGetValue(rapportOrCard, out rarity);

            if (hasKey)
            {
                return rarity;
            }

            return null;
        }
    }
}
