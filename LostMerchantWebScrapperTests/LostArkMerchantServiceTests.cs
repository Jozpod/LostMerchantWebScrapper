using FluentAssertions;
using LostMerchantWebScrapper;
using LostMerchantWebScrapper.Models;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LostMerchantWebScrapperTests
{
    [TestClass]
    public class LostArkMerchantServiceTests
    {
        private Mock<IWebDriver> _webDriverMock = new Mock<IWebDriver>(MockBehavior.Strict);
        private Mock<ILostArkDescriptor> _lostArkDescriptorMock = new Mock<ILostArkDescriptor>(MockBehavior.Strict);

        public ILostArkMerchantService CreateService(TimeSpan waitTimeout)
        {
            var navigateMock = new Mock<INavigation>(MockBehavior.Strict);

            _webDriverMock
                .Setup(pr => pr.Navigate())
                .Returns(navigateMock.Object);

            navigateMock
                .Setup(pr => pr.GoToUrl(It.IsNotNull<string>()));
            
            var javaScriptExecutorMock = new Mock<IJavaScriptExecutor>(MockBehavior.Strict);
            var optionMock = new Mock<IOptionsMonitor<MerchantCheckerOption>>(MockBehavior.Strict);
            var option = new MerchantCheckerOption
            {
                FindButtonTimeout = waitTimeout,
            };

            javaScriptExecutorMock
                .Setup(pr => pr.ExecuteScript(It.IsNotNull<string>()))
                .Returns(option);

            optionMock
                .Setup(pr => pr.CurrentValue)
                .Returns(option);

            optionMock
                .Setup(pr => pr.OnChange(It.IsAny<Action<MerchantCheckerOption, string>>()))
                .Returns(new Mock<IDisposable>().Object);

            var service = new LostArkMerchantService(
                _webDriverMock.Object,
                javaScriptExecutorMock.Object,
                _lostArkDescriptorMock.Object,
                NullLogger<ILostArkMerchantService>.Instance,
                optionMock.Object);
            return service;
        }

        [TestMethod]
        public async Task Should_Return_Empty_Collection_Timeout()
        {
            var service = CreateService(TimeSpan.FromSeconds(0));

            var webElement = new Mock<IWebElement>(MockBehavior.Strict);

            _webDriverMock
               .Setup(pr => pr.FindElement(It.IsAny<By>()))
               .Returns(webElement.Object);

            webElement
                .Setup(pr => pr.Displayed)
                .Returns(false);

            var result = await service.GetEntriesAsync();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Should_Return_Empty_Collection_Node_Not_Found()
        {
            var service = CreateService(TimeSpan.FromSeconds(0));

            var webElement = new Mock<IWebElement>(MockBehavior.Strict);

            _webDriverMock
               .Setup(pr => pr.FindElement(It.IsAny<By>()))
               .Returns(webElement.Object);

            webElement
                .Setup(pr => pr.Displayed)
                .Returns(true);

            webElement
                .Setup(pr => pr.FindElements(It.IsAny<By>()))
                .Returns(new List<IWebElement>().AsReadOnly());

            var result = await service.GetEntriesAsync();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Should_Return_Entries()
        {
            var service = CreateService(TimeSpan.FromSeconds(1));

            var tableElement = new Mock<IWebElement>(MockBehavior.Strict);
            var rowElement = new Mock<IWebElement>(MockBehavior.Strict);

            var expected = new MerchantEntry
            {
                Name = "Malone",
                Region = "West Luterra",
                Zone = "Mount Zagoras",
                Card = "Cassleford",
                Rapport = "Stalwart Cage",
                RapportRarity = Rarity.Epic,
                Votes = 1,
            };

            var nameCell = CreateElementWithText(expected.Name);
            var regionCell = CreateElementWithText(expected.Region);
            var zoneCell = CreateElementWithText(expected.Zone);
            var cardCell = CreateElementWithText(expected.Card);
            var rapportCell = CreateElementWithText(expected.Rapport);
            var votesCell = CreateElementWithText(expected.Votes.ToString());

            var rows = new List<IWebElement>()
            {
                rowElement.Object
            }.AsReadOnly();

            var cells = new List<IWebElement>()
            {
                nameCell,
                regionCell,
                zoneCell,
                cardCell,
                rapportCell,
                votesCell,
            }.AsReadOnly();

            _webDriverMock
               .Setup(pr => pr.FindElement(It.IsAny<By>()))
               .Returns(tableElement.Object);

            tableElement
                .Setup(pr => pr.Displayed)
                .Returns(true);

            tableElement
               .Setup(pr => pr.FindElements(It.IsAny<By>()))
               .Returns(rows);

            rowElement
                .Setup(pr => pr.FindElements(It.IsAny<By>()))
                .Returns(cells);

            _lostArkDescriptorMock
                .Setup(pr => pr.GetRarityAsync(It.IsNotNull<string>()))
                .ReturnsAsync(expected.RapportRarity);

            var result = await service.GetEntriesAsync();
            var entry = result.First();

            entry.Should().BeEquivalentTo(expected);
        }

        private IWebElement CreateElementWithText(string text)
        {
            var webElement = new Mock<IWebElement>(MockBehavior.Strict);

            webElement
                .Setup(pr => pr.Text)
                .Returns(text);

            return webElement.Object;
        }
    }
}