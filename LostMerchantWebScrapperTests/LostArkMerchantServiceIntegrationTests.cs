using FluentAssertions;
using LostMerchantWebScrapper;
using LostMerchantWebScrapper.Builder;
using LostMerchantWebScrapper.Models;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace LostMerchantWebScrapperTests
{
    //[TestClass]
    public class LostArkMerchantServiceIntegrationTests
    {
        private ServiceProvider? _serviceProvider;

        public ILostArkMerchantService CreateService()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json");

            var configuration = configurationBuilder.Build();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<ILostArkMerchantService, LostArkMerchantService>()
                .AddSingleton<ISystemClock, DefaultSystemClock>()
                .AddSingleton<ILostArkDescriptor, LostArkDescriptor>()
                .AddSingleton<IFileProvider, DefaultFileProvider>()
                .AddChromeWebDriver()
                .AddSingleton((serviceProvider) =>
                {
                    var driver = serviceProvider.GetRequiredService<IWebDriver>();
                    return (IJavaScriptExecutor)driver;
                })
                .AddLogging(configure => configure.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                }))
                .Configure<MerchantCheckerOption>(configuration)
                .AddMemoryCache()
                .BuildServiceProvider();

            var service = _serviceProvider.GetRequiredService<ILostArkMerchantService>();

            return service;
        }

        [TestMethod]
        public async Task Should_Return_Empty_Collection()
        {
            var service = CreateService();
            var currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, "Data", "upcoming_merchants.mhtml");

            var result = await service.GetEntriesAsync(filePath, false);
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Should_Return_Entries()
        {
            var service = CreateService();
            var currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, "Data", "merchants_populated.mhtml");

            var result = await service.GetEntriesAsync(filePath, false);
            result.Should().HaveCount(11);

            result.First().Should().BeEquivalentTo(new MerchantEntry
            {
                Name = "Lucas",
                Region = "Yudia",
                Zone = "Saland Hill",
                Rapport = "Yudia Natural Salt",
                Card = "Morina",
                RapportRarity = Rarity.Epic,
                Votes = 1
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            var webDriver = _serviceProvider?.GetRequiredService<IWebDriver>();
            webDriver?.Quit();
        }
    }
}