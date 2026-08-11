namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public interface IInvoiceService
    {
        // Runs on the 28th — creates next month's invoice (rent only) for every occupied unit
        Task GenerateMonthlyInvoicesAsync();

        // Pulls current RentAmount/WaterAmount/ExpectedAmount/Status from the linked UnitPayments
        // row and syncs them onto the invoice. Called after anything changes UnitPayments —
        // a payment webhook, or WaterBillingService applying a water charge.
        Task SyncInvoiceFromUnitPaymentsAsync(long unitPaymentsId);

        Task<Models.Dto.InvoiceDto?> GetByIdAsync(long id);
        Task<List<Models.Dto.InvoiceDto>> GetAllAsync();
        Task<List<Models.Dto.InvoiceDto>> GetByPropertyIdAsync(long propertyId);
        Task<List<Models.Dto.InvoiceDto>> GetByUnitIdAsync(long unitId);
        Task<List<Models.Dto.InvoiceDto>> GetByTenantIdAsync(long tenantId);
    }
}