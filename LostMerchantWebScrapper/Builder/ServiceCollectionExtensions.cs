using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace LostMerchantWebScrapper.Builder
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChromeWebDriver(this IServiceCollection services)
        {
            return services.AddSingleton<IWebDriver>((serviceProvider) =>
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("headless");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--disable-crash-reporter");
                chromeOptions.AddArgument("--disable-extensions");
                chromeOptions.AddArgument("--disable-in-process-stack-traces");
                chromeOptions.AddArgument("--disable-logging");
                chromeOptions.AddArgument("--disable-dev-shm-usage");
                chromeOptions.AddArgument("--log-level=3");
                chromeOptions.AddExcludedArguments(new[] { "enable-logging" });

                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                //chromeDriverService.SuppressInitialDiagnosticInformation = true;
                chromeDriverService.HideCommandPromptWindow = true;

                var driver = new ChromeDriver(chromeDriverService, chromeOptions);
                return driver;
            });
        }
    }
}
