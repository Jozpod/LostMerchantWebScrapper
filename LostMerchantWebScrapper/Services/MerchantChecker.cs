using Humanizer;
using LostMerchantWebScrapper.Models;
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
        private CancellationTokenSource _cancellationTokenSource;

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
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task RunAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await InnerRunAsync(_cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Stopping merchant checker...");
            }
        }

        internal async Task InnerRunAsync(CancellationToken cancellationToken = default)
        {
            var isRunning = await _statusService.IsThaemineRunning();

            if (!isRunning)
            {
                _logger.LogInformation("Server is not running");
                await _taskManager.Delay(_option.ProbeDelay, _cancellationTokenSource.Token);
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
                await _taskManager.Delay(delay, _cancellationTokenSource.Token);
                return;
            }

            var entries = await _merchantService.GetEntriesAsync(cancellationToken: cancellationToken);

            if (!entries.Any())
            {
                _logger.LogInformation("No entries found");
                await _taskManager.Delay(_option.ProbeDelay, _cancellationTokenSource.Token);
                return;
            }

            Print(entries);

            await _taskManager.Delay(_option.ProbeDelay, _cancellationTokenSource.Token);
        }

        private void Print(IEnumerable<MerchantEntry> entries)
        {
            var rulesNoMatchInfo = true;
            var stringComparison = StringComparison.InvariantCultureIgnoreCase;

            foreach (var entry in entries)
            {
                foreach (var rule in _option.Rules)
                {
                    if (rule.Type == FilterRuleType.Rapport)
                    {
                        var rapport = entry.Rapport;

                        if (!string.IsNullOrEmpty(rapport)
                            && !string.IsNullOrEmpty(rule.Name))
                        {
                            if (rapport.Contains(rule.Name, stringComparison))
                            {
                                _logger.LogInformation("Rapport {}", rapport);
                                rulesNoMatchInfo = false;
                            }
                        }

                        if (!string.IsNullOrEmpty(rapport)
                            && rule.Rarity.HasValue)
                        {
                            if (entry.RapportRarity == rule.Rarity)
                            {
                                _logger.LogInformation("{} rapport {}", rule.Rarity, rapport);
                                rulesNoMatchInfo = false;
                            }
                        }
                    }

                    if (rule.Type == FilterRuleType.Card)
                    {
                        var card = entry.Card;
                        if (!string.IsNullOrEmpty(card)
                            && !string.IsNullOrEmpty(rule.Name))
                        {
                            if (card.Contains(rule.Name, stringComparison))
                            {
                                _logger.LogInformation("Card {}", entry.Card);
                                rulesNoMatchInfo = false;
                            }
                        }
                    }
                }
            }

            if (rulesNoMatchInfo)
            {
                _logger.LogInformation("Currently there are no rapports/cards which match your criteria");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
