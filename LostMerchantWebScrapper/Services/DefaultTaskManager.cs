namespace LostMerchantWebScrapper.Services
{
    public class DefaultTaskManager : ITaskManager
    {
        public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) 
            => Task.Delay(delay, cancellationToken);
    }
}