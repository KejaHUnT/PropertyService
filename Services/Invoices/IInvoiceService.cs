using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.Enums;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public interface IInvoiceService
    {
        // Runs on the 28th — creates next month's invoice (rent only) for every occupied unit
        Task GenerateMonthlyInvoicesAsync();

        // Called when a manager updates a unit's water bill for a period
        Task<InvoiceDto> UpdateWaterBillAsync(long unitId, int periodMonth, int periodYear, decimal waterBillAmount);

        // Called from the payment webhook to mirror UnitPayments.Status onto the linked invoice
        Task SyncInvoiceStatusFromUnitPaymentsAsync(long unitPaymentsId, UnitPaymentStatus status);

        Task<InvoiceDto?> GetByIdAsync(long id);
        Task<List<InvoiceDto>> GetAllAsync();
        Task<List<InvoiceDto>> GetByPropertyIdAsync(long propertyId);
        Task<List<InvoiceDto>> GetByUnitIdAsync(long unitId);
        Task<List<InvoiceDto>> GetByTenantIdAsync(long tenantId);
    }
}