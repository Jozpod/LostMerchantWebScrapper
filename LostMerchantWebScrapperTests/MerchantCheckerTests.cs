using FluentAssertions;
using HtmlAgilityPack;
using LostMerchantWebScrapper;
using LostMerchantWebScrapper.Models;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LostMerchantWebScrapperTests
{
    [TestClass]
    public class MerchantCheckerTests
    {
        private readonly Mock<ILostArkMerchantService> _lostArkMerchantServiceMock = new Mock<ILostArkMerchantService>(MockBehavior.Strict);
        private readonly Mock<ILostArkStatusService> _lostArkStatusServiceMock = new Mock<ILostArkStatusService>(MockBehavior.Strict);
        private readonly Mock<ITaskManager> _taskManagerMock = new Mock<ITaskManager>(MockBehavior.Strict);
        private readonly Mock<ISystemClock> _systemClockMock = new Mock<ISystemClock>(MockBehavior.Strict);
        private MerchantCheckerOption _option;

        internal MerchantChecker CreateService(ILogger<MerchantChecker>? logger = null)
        {
            logger ??= NullLogger<MerchantChecker>.Instance;
            var optionMock = new Mock<IOptionsMonitor<MerchantCheckerOption>>(MockBehavior.Strict);
            _option = new MerchantCheckerOption();

            optionMock
                .Setup(pr => pr.CurrentValue)
                .Returns(_option);

            optionMock
                .Setup(pr => pr.OnChange(It.IsAny<Action<MerchantCheckerOption, string>>()))
                .Returns(new Mock<IDisposable>().Object);

            var service = new MerchantChecker(
                _lostArkStatusServiceMock.Object,
                _lostArkMerchantServiceMock.Object,
                _taskManagerMock.Object,
                _systemClockMock.Object,
                logger,
                optionMock.Object);
            return service;
        }

        [TestMethod]
        public async Task Should_Retry_When_Server_Down()
        {
            var service = CreateService();

            _lostArkStatusServiceMock
                .Setup(pr => pr.IsThaemineRunning())
                .ReturnsAsync(false);

            _taskManagerMock
                .Setup(pr => pr.Delay(_option.ProbeDelay))
                .Returns(Task.CompletedTask);

            _lostArkStatusServiceMock
                .Setup(pr => pr.IsThaemineRunning())
                .ReturnsAsync(true);

            _systemClockMock
                .Setup(pr => pr.UtcNow)
                .Returns(new DateTime(2022, 8, 17, 20, 31, 20));

            _lostArkMerchantServiceMock
                .Setup(pr => pr.GetEntriesAsync())
                .ReturnsAsync(new List<MerchantEntry>());

            await service.InnerRunAsync();
        }

        [TestMethod]
        public async Task Should_Retry_Later_When_Not_Right_Time()
        {
            var service = CreateService();

            _lostArkStatusServiceMock
                .Setup(pr => pr.IsThaemineRunning())
                .ReturnsAsync(true);

            _systemClockMock
                .Setup(pr => pr.UtcNow)
                .Returns(new DateTime(2022, 8, 17, 20, 21, 20));

            _taskManagerMock
                .Setup(pr => pr.Delay(It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            await service.InnerRunAsync();
        }

        [TestMethod]
        public async Task Should_Display_Merchant_Info_Legendary_Rapport()
        {
            var loggerMock = new Mock<ILogger<MerchantChecker>>(MockBehavior.Strict);
            var service = CreateService(loggerMock.Object);
            var merchantEntry = new MerchantEntry
            {
                Name = "Merchant",
                Zone = "Rattan Hill",
                Region = "Anikka",
                Rapport = "test",
                RapportRarity = Rarity.Legendary,
                Card = "Wei",
                Votes = 1,
            };
            _option.Rules = new List<FilterRule>
           {
                new FilterRule
                {
                    Type = FilterRule.FilterRuleType.Rapport,
                    Name = "test"
                }
            };

            var merchantEntries = new List<MerchantEntry>(){ merchantEntry };

            _lostArkStatusServiceMock
                .Setup(pr => pr.IsThaemineRunning())
                .ReturnsAsync(true);

            _systemClockMock
                .Setup(pr => pr.UtcNow)
                .Returns(new DateTime(2022, 8, 17, 20, 31, 20));

            _taskManagerMock
                .Setup(pr => pr.Delay(It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            _lostArkMerchantServiceMock
                .Setup(pr => pr.GetEntriesAsync())
                .ReturnsAsync(merchantEntries);

            loggerMock
                .Setup(logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Verifiable();

            await service.InnerRunAsync();

            loggerMock.Verify();
        }

        [TestMethod]
        public async Task Should_Display_Merchant_Info_Legendary_Wei()
        {
            var loggerMock = new Mock<ILogger<MerchantChecker>>(MockBehavior.Strict);
            var service = CreateService(loggerMock.Object);
            var merchantEntry = new MerchantEntry
            {
                Name = "Merchant",
                Zone = "Rattan Hill",
                Region = "Anikka",
                Rapport = "test",
                RapportRarity = Rarity.Epic,
                Card = "Wei",
                Votes = 1,
            };
            _option.Rules = new List<FilterRule>
            {
                new FilterRule
                {
                    Type = FilterRule.FilterRuleType.Card,
                    Name = "Wei"
                }
            };

            var merchantEntries = new List<MerchantEntry>() { merchantEntry };

            _lostArkStatusServiceMock
                .Setup(pr => pr.IsThaemineRunning())
                .ReturnsAsync(true);

            _systemClockMock
                .Setup(pr => pr.UtcNow)
                .Returns(new DateTime(2022, 8, 17, 20, 31, 20));

            _taskManagerMock
                .Setup(pr => pr.Delay(It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            _lostArkMerchantServiceMock
                .Setup(pr => pr.GetEntriesAsync())
                .ReturnsAsync(merchantEntries);

            loggerMock
                .Setup(logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Verifiable();

            await service.InnerRunAsync();

            loggerMock.Verify();
        }
    }
}