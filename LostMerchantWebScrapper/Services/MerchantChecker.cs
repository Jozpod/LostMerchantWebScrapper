using Humanizer;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LostMerchantWebScrapper.Models.FilterRule;

namespace LostMerchantWebScrapper.Services
{
    public class MerchantChecker
    {
        private readonly ILostArkStatusService _statusService;
        private readonly ILostArkMerchantService _merchantService;
        private readonly ITaskManager _taskManager;
        private readonly ISystemClock _systemClock;
        private readonly ILogger _logger;
        private MerchantCheckerOption _option;

        public MerchantChecker(ILostArkStatusService statusService,
            ILostArkMerchantService merchantService,
            ITaskManager taskManager,
            ISystemClock systemClock,
            ILogger<MerchantChecker> logger,
            IOptionsMonitor<MerchantCheckerOption> option)
        {
            _statusService = statusService;
            _merchantService = merchantService;
            _taskManager = taskManager;
            _systemClock = systemClock;
            _logger = logger;
            _option = option.CurrentValue;
            option.OnChange((option, key) => _option = option);
        }

        public async Task RunAsync()
        {
            while (true)
            {
                await InnerRunAsync();
            }
        }

        internal async Task InnerRunAsync()
        {
            var isRunning = await _statusService.IsThaemineRunning();

            if (!isRunning)
            {
                _logger.LogInformation("Server is not running");
                await _taskManager.Delay(_option.ProbeDelay);
                return;
            }

            var timeOfDay = _systemClock.UtcNow.TimeOfDay;
            var minutes = timeOfDay.Minutes;
            var hasMerchantsSpawned = minutes > 29 && minutes < 55;

            if (!hasMerchantsSpawned)
            {
                _logger.LogInformation("Merchants either disappeared or have not spawned");
                minutes = (minutes > 55 ? 90 : 30) - minutes;
                var seconds = TimeSpan.FromSeconds(timeOfDay.Seconds);
                var delay = TimeSpan.FromMinutes(minutes).Subtract(seconds);
                _logger.LogInformation("Retrying in {}...", delay.Humanize(2));
                await _taskManager.Delay(delay);
                return;
            }

            var entries = await _merchantService.GetEntriesAsync();

            if (!entries.Any())
            {
                _logger.LogInformation("No entries found");
                await _taskManager.Delay(_option.ProbeDelay);
                return;
            }

            var stringComparison = StringComparison.InvariantCultureIgnoreCase;

            foreach (var entry in entries)
            {
                foreach (var rule in _option.Rules)
                {
                    if (rule.Type == FilterRuleType.Rapport)
                    {
                        if (!string.IsNullOrEmpty(entry.Rapport)
                            && !string.IsNullOrEmpty(rule.Name))
                        {
                            if (entry.Rapport.Contains(rule.Name, stringComparison))
                            {
                                _logger.LogInformation("Rapport {}", entry.Rapport);
                            }
                        }

                        if (!string.IsNullOrEmpty(entry.Rapport)
                            && rule.Rarity.HasValue)
                        {
                            if (entry.RapportRarity == rule.Rarity)
                            {
                                _logger.LogInformation("{} rapport {}", rule.Rarity, entry.Rapport);
                            }
                        }
                    }

                    if (rule.Type == FilterRuleType.Card)
                    {
                        if (!string.IsNullOrEmpty(entry.Card)
                            && !string.IsNullOrEmpty(rule.Name))
                        {
                            if (entry.Card.Contains(rule.Name, stringComparison))
                            {
                                _logger.LogInformation("Card {}", entry.Card);
                            }
                        }
                    }
                }
            }

            await _taskManager.Delay(_option.ProbeDelay);
        }
    }
}
