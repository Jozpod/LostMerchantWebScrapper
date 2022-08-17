using FluentAssertions;
using LostMerchantWebScrapper;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace LostMerchantWebScrapperTests
{
    [TestClass]
    public class LostArkStatusServiceTests
    {
        public static ILostArkStatusService CreateService(string filePath)
        {
            var scraper = new TestPageScrapper(filePath);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var systemClockMock = new Mock<ISystemClock>(MockBehavior.Strict);
            var optionMock = new Mock<IOptionsMonitor<MerchantCheckerOption>>(MockBehavior.Strict);
            var option = new MerchantCheckerOption();

            systemClockMock
                .Setup(pr => pr.UtcNow)
                .Returns(DateTime.UtcNow);

            optionMock
                .Setup(pr => pr.CurrentValue)
                .Returns(option);

            optionMock
                .Setup(pr => pr.OnChange(It.IsAny<Action<MerchantCheckerOption, string>>()))
                .Returns(new Mock<IDisposable>().Object);

            var service = new LostArkStatusService(
                scraper,
                memoryCache,
                systemClockMock.Object,
                optionMock.Object);
            return service;
        }

        [TestMethod]
        public async Task Should_Return_False_No_Node()
        {
            var filePath = "servers_down_status.mhtml";
            var service = CreateService(filePath);

            var result = await service.IsThaemineRunning();
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task Should_Return_False_Maintenance()
        {
            var filePath = "servers_down_status.mhtml";
            var service = CreateService(filePath);

            var result = await service.IsThaemineRunning();
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task Should_Return_True()
        {
            var filePath = "servers_running_status.mhtml";
            var service = CreateService(filePath);

            var result = await service.IsThaemineRunning();
            result.Should().BeTrue();
        }
    }
}