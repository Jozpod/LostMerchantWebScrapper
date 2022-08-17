using Microsoft.Extensions.Internal;

namespace LostMerchantWebScrapper.Services
{
    internal class DefaultSystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTime.UtcNow;
    }
}
