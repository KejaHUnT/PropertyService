namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public interface IInvoiceService
    {
        // Runs on the 28th — creates NEXT month's invoice (rent only) for every occupied unit
        Task GenerateMonthlyInvoicesAsync();

        // Same generation logic, but for an explicit period — used for one-off backfills
        // (e.g. generating August's invoice manually before the 28th automation starts).
        Task GenerateInvoicesForPeriodAsync(int periodMonth, int periodYear);

        Task SyncInvoiceFromUnitPaymentsAsync(long unitPaymentsId);

        Task<Models.Dto.InvoiceDto?> GetByIdAsync(long id);
        Task<List<Models.Dto.InvoiceDto>> GetAllAsync();
        Task<List<Models.Dto.InvoiceDto>> GetByPropertyIdAsync(long propertyId);
        Task<List<Models.Dto.InvoiceDto>> GetByUnitIdAsync(long unitId);
        Task<List<Models.Dto.InvoiceDto>> GetByTenantIdAsync(long tenantId);
    }
}