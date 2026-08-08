using KejaHUnt_PropertiesAPI.Services.Invoices;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public class MonthlyInvoiceBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyInvoiceBackgroundService> _logger;
        private DateTime? _lastRunDate = null;

        private const int TriggerDay = 28;

        public MonthlyInvoiceBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyInvoiceBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonthlyInvoiceBackgroundService started, watching for day {Day}", TriggerDay);

            while (!stoppingToken.IsCancellationRequested)
            {
                var today = DateTime.UtcNow.Date;

                if (today.Day == TriggerDay && _lastRunDate != today)
                {
                    _logger.LogInformation("Day {Day} reached — running monthly invoice generation", TriggerDay);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                        await invoiceService.GenerateMonthlyInvoicesAsync();

                        _lastRunDate = today;
                        _logger.LogInformation("Monthly invoice generation completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Monthly invoice generation failed");
                    }
                }

                // Check once every 6 hours — no need to check more often for a once-a-month trigger
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}