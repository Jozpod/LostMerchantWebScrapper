
using LostMerchantWebScrapper;
using LostMerchantWebScrapper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

var currentDirectory = Directory.GetCurrentDirectory();

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(currentDirectory)
    .AddJsonFile("appsettings.json");

var configuration = configurationBuilder.Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton<ILostArkMerchantService, LostArkMerchantService>()
    .AddSingleton<ILostArkStatusService, LostArkStatusService>()
    .AddSingleton<ISystemClock, DefaultSystemClock>()
    .AddSingleton<ITaskManager, DefaultTaskManager>()
    .AddSingleton<IPageScrapper, PageScrapper>()
    .AddSingleton<ILostArkDescriptor, LostArkDescriptor>()
    .AddSingleton<IFileProvider, DefaultFileProvider>()
    .AddSingleton<MerchantChecker>()
    .AddSingleton<IWebDriver>((serviceProvider) =>
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

        var driver = new ChromeDriver(chromeOptions);
        return driver;
    })
    .AddSingleton<IJavaScriptExecutor>((serviceProvider) =>
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




var merchantChecker = serviceProvider.GetRequiredService<MerchantChecker>();
await merchantChecker.RunAsync();