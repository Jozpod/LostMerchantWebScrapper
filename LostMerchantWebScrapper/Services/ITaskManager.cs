﻿namespace LostMerchantWebScrapper.Services
{
    public interface ITaskManager
    {
        Task Delay(TimeSpan delay);
    }
}