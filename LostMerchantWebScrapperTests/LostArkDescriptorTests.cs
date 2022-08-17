using FluentAssertions;
using HtmlAgilityPack;
using LostMerchantWebScrapper;
using LostMerchantWebScrapper.Models;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace LostMerchantWebScrapperTests
{
    [TestClass]
    public class LostArkDescriptorTests
    {
        internal static ILostArkDescriptor CreateService()
        {
            var fileProvider = new DefaultFileProvider();
            var service = new LostArkDescriptor(fileProvider);
            return service;
        }

        [TestMethod]
        [DataRow(Rarity.Uncommon, "Sir Druden")]
        [DataRow(Rarity.Rare, "Prideholme Neria")]
        [DataRow(Rarity.Epic, "Tournament Entrance Stamp")]
        [DataRow(Rarity.Legendary, "Wei")]
        public async Task Should_Return_Rarity(Rarity expectedRarity, string rapportOrCard)
        {
            var service = CreateService();
            var rarity = await service.GetRarityAsync(rapportOrCard);
            rarity.Should().Be(expectedRarity);
        }

        [TestMethod]
        public async Task Should_Return_Null()
        {
            var service = CreateService();

            var rarity = await service.GetRarityAsync("test");
            rarity.Should().BeNull();
        }
    }
}