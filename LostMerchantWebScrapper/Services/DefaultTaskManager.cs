namespace LostMerchantWebScrapper.Services
{
    public class DefaultTaskManager : ITaskManager
    {
        public Task Delay(TimeSpan delay) => Task.Delay(delay);
    }
}