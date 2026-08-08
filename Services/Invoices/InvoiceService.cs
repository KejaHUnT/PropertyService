using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.Enums;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using KejaHUnt_PropertiesAPI.Services.Tenants;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUnitPaymentsRepository _unitPaymentsRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ITenantServiceClient _tenantServiceClient;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IUnitPaymentsRepository unitPaymentsRepository,
            IUnitRepository unitRepository,
            ITenantServiceClient tenantServiceClient,
            ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _unitPaymentsRepository = unitPaymentsRepository;
            _unitRepository = unitRepository;
            _tenantServiceClient = tenantServiceClient;
            _logger = logger;
        }

        // RUNS ON THE 28TH — generates next month's invoice (rent only) for every occupied unit
        public async Task GenerateMonthlyInvoicesAsync()
        {
            var units = await _unitRepository.GetAllAsync();
            var occupiedUnits = units.Where(u => u.Status != UnitStatus.Vacant).ToList();

            var nextPeriod = DateTime.UtcNow.AddMonths(1);
            var periodMonth = nextPeriod.Month;
            var periodYear = nextPeriod.Year;

            foreach (var unit in occupiedUnits)
            {
                try
                {
                    // Skip if an invoice already exists for this unit/period (idempotent re-runs)
                    var existingInvoice = await _invoiceRepository.GetByUnitAndPeriodAsync(unit.Id, periodMonth, periodYear);
                    if (existingInvoice != null)
                    {
                        _logger.LogInformation("Invoice already exists for unit {UnitId} for {Month}/{Year}, skipping", unit.Id, periodMonth, periodYear);
                        continue;
                    }

                    var tenant = await _tenantServiceClient.GetActiveTenantByUnitIdAsync(unit.Id);
                    if (tenant == null)
                    {
                        _logger.LogInformation("No active tenant for unit {UnitId}, skipping invoice generation", unit.Id);
                        continue;
                    }

                    var unitPayments = await _unitPaymentsRepository.GetByUnitAndPeriodAsync(unit.Id, periodMonth, periodYear);
                    if (unitPayments == null)
                    {
                        unitPayments = new UnitPayments
                        {
                            UnitId = unit.Id,
                            PropertyId = unit.PropertyId,
                            TenantId = tenant.Id,
                            PeriodMonth = periodMonth,
                            PeriodYear = periodYear,
                            ExpectedAmount = unit.Price,
                            PaidAmount = 0,
                            Status = UnitPaymentStatus.Pending
                        };
                        unitPayments = await _unitPaymentsRepository.CreateAsync(unitPayments);
                    }

                    var invoice = new Invoice
                    {
                        UnitId = unit.Id,
                        PropertyId = unit.PropertyId,
                        TenantId = tenant.Id,
                        UnitPaymentsId = unitPayments.Id,
                        PeriodMonth = periodMonth,
                        PeriodYear = periodYear,
                        RentAmount = unitPayments.ExpectedAmount,
                        WaterBillAmount = 0,
                        TotalAmount = unitPayments.ExpectedAmount,
                        Status = UnitPaymentStatus.Pending,
                        DueDate = new DateTime(periodYear, periodMonth, 10)
                    };

                    await _invoiceRepository.CreateAsync(invoice);
                    _logger.LogInformation("Generated invoice for unit {UnitId}, tenant {TenantId}, {Month}/{Year}", unit.Id, tenant.Id, periodMonth, periodYear);
                }
                catch (Exception ex)
                {
                    // Don't let one unit's failure stop the whole batch
                    _logger.LogError(ex, "Failed to generate invoice for unit {UnitId}", unit.Id);
                }
            }
        }

        // MANAGER UPDATES WATER BILL FOR A UNIT/PERIOD
        public async Task<InvoiceDto> UpdateWaterBillAsync(long unitId, int periodMonth, int periodYear, decimal waterBillAmount)
        {
            var invoice = await _invoiceRepository.GetByUnitAndPeriodAsync(unitId, periodMonth, periodYear);
            if (invoice == null)
                throw new ArgumentException(
                    $"No invoice found for unit {unitId} for {periodMonth}/{periodYear}. It should have been created by the monthly generation job.");

            invoice.WaterBillAmount = waterBillAmount;
            invoice.TotalAmount = invoice.RentAmount + waterBillAmount;

            var updated = await _invoiceRepository.UpdateAsync(invoice);

            var refreshed = await _invoiceRepository.GetByIdAsync(updated.Id);
            return MapToDto(refreshed!);
        }

        // CALLED FROM THE PAYMENT WEBHOOK TO MIRROR UnitPayments.Status ONTO THE LINKED INVOICE
        public async Task SyncInvoiceStatusFromUnitPaymentsAsync(long unitPaymentsId, UnitPaymentStatus status)
        {
            var invoice = await _invoiceRepository.GetByUnitPaymentsIdAsync(unitPaymentsId);
            if (invoice == null)
            {
                _logger.LogInformation("No invoice linked to UnitPayments {UnitPaymentsId}, nothing to sync", unitPaymentsId);
                return;
            }

            invoice.Status = status;
            await _invoiceRepository.UpdateStatusAsync(invoice);
        }

        public async Task<InvoiceDto?> GetByIdAsync(long id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            return invoice == null ? null : MapToDto(invoice);
        }

        public async Task<List<InvoiceDto>> GetAllAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return invoices.Select(MapToDto).ToList();
        }

        public async Task<List<InvoiceDto>> GetByPropertyIdAsync(long propertyId)
        {
            var invoices = await _invoiceRepository.GetByPropertyIdAsync(propertyId);
            return invoices.Select(MapToDto).ToList();
        }

        public async Task<List<InvoiceDto>> GetByUnitIdAsync(long unitId)
        {
            var invoices = await _invoiceRepository.GetByUnitIdAsync(unitId);
            return invoices.Select(MapToDto).ToList();
        }

        public async Task<List<InvoiceDto>> GetByTenantIdAsync(long tenantId)
        {
            var invoices = await _invoiceRepository.GetByTenantIdAsync(tenantId);
            return invoices.Select(MapToDto).ToList();
        }

        private static InvoiceDto MapToDto(Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                UnitId = invoice.UnitId,
                DoorNumber = invoice.Unit?.DoorNumber,
                PropertyId = invoice.PropertyId,
                PropertyName = invoice.Property?.Name,
                TenantId = invoice.TenantId,
                UnitPaymentsId = invoice.UnitPaymentsId,
                PeriodMonth = invoice.PeriodMonth,
                PeriodYear = invoice.PeriodYear,
                RentAmount = invoice.RentAmount,
                WaterBillAmount = invoice.WaterBillAmount,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                DueDate = invoice.DueDate,
                CreatedAt = invoice.CreatedAt
            };
        }
    }
}