namespace MAPI
{
    public class KeepAliveService:BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("[INFO] KeepAliveService is running...");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Prevent shutdown
            }
        }
    }
}
