namespace IRB.RevigoWeb
{
	/// <summary>
	/// To do: Implement the BackgroundService for use on linux systems
	/// </summary>
	public class RevigoWebWorker : BackgroundService
	{
		private readonly ILogger<RevigoWebWorker> _logger;

		public RevigoWebWorker(ILogger<RevigoWebWorker> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}
