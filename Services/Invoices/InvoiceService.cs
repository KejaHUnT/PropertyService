using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.Enums;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using KejaHUnt_PropertiesAPI.Services.Tenants;
using QuestPDF.Fluent;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUnitPaymentsRepository _unitPaymentsRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly ITenantServiceClient _tenantServiceClient;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IUnitPaymentsRepository unitPaymentsRepository,
            IUnitRepository unitRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            ITenantServiceClient tenantServiceClient,
            ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _unitPaymentsRepository = unitPaymentsRepository;
            _unitRepository = unitRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _tenantServiceClient = tenantServiceClient;
            _logger = logger;
        }

        // RUNS ON THE 28TH — generates NEXT month's invoice (rent only) for every occupied unit
        public async Task GenerateMonthlyInvoicesAsync()
        {
            var nextPeriod = DateTime.UtcNow.AddMonths(1);
            await GenerateForPeriodInternalAsync(nextPeriod.Month, nextPeriod.Year);
        }

        // Same generation logic, targeted at an explicit period — for one-off backfills
        public async Task GenerateInvoicesForPeriodAsync(int periodMonth, int periodYear)
        {
            await GenerateForPeriodInternalAsync(periodMonth, periodYear);
        }

        private async Task GenerateForPeriodInternalAsync(int periodMonth, int periodYear)
        {
            var units = await _unitRepository.GetAllAsync();
            var occupiedUnits = units.Where(u => u.Status != UnitStatus.Vacant).ToList();

            var sequence = await _invoiceRepository.GetCountByPeriodAsync(periodMonth, periodYear);

            foreach (var unit in occupiedUnits)
            {
                try
                {
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
                            RentAmount = unit.Price,
                            WaterAmount = 0,
                            PaidAmount = 0,
                            Status = UnitPaymentStatus.Pending
                        };
                        unitPayments.RecalculateExpectedAmount();
                        unitPayments = await _unitPaymentsRepository.CreateAsync(unitPayments);
                    }

                    sequence++;

                    var invoice = new Invoice
                    {
                        InvoiceNumber = $"KH-{periodYear:D4}{periodMonth:D2}-{sequence:D4}",
                        UnitId = unit.Id,
                        PropertyId = unit.PropertyId,
                        TenantId = tenant.Id,
                        UnitPaymentsId = unitPayments.Id,
                        PeriodMonth = periodMonth,
                        PeriodYear = periodYear,
                        RentAmount = unitPayments.RentAmount,
                        WaterBillAmount = unitPayments.WaterAmount,
                        TotalAmount = unitPayments.ExpectedAmount,
                        Status = unitPayments.Status,
                        DueDate = DateTime.SpecifyKind(new DateTime(periodYear, periodMonth, 10), DateTimeKind.Utc)
                    };

                    await _invoiceRepository.CreateAsync(invoice);
                    _logger.LogInformation("Generated invoice for unit {UnitId}, tenant {TenantId}, {Month}/{Year}", unit.Id, tenant.Id, periodMonth, periodYear);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate invoice for unit {UnitId}", unit.Id);
                }
            }
        }

        // Pulls current RentAmount/WaterAmount/ExpectedAmount/Status from UnitPayments and
        // syncs them onto the linked invoice. Called after a payment webhook or a water charge.
        public async Task SyncInvoiceFromUnitPaymentsAsync(long unitPaymentsId)
        {
            var invoice = await _invoiceRepository.GetByUnitPaymentsIdAsync(unitPaymentsId);
            if (invoice == null)
            {
                _logger.LogInformation("No invoice linked to UnitPayments {UnitPaymentsId}, nothing to sync", unitPaymentsId);
                return;
            }

            var unitPayments = await _unitPaymentsRepository.GetByIdAsync(unitPaymentsId);
            if (unitPayments == null)
            {
                _logger.LogWarning("UnitPayments {UnitPaymentsId} not found while syncing invoice {InvoiceId}", unitPaymentsId, invoice.Id);
                return;
            }

            invoice.RentAmount = unitPayments.RentAmount;
            invoice.WaterBillAmount = unitPayments.WaterAmount;
            invoice.TotalAmount = unitPayments.ExpectedAmount;
            invoice.Status = unitPayments.Status;

            await _invoiceRepository.UpdateFromUnitPaymentsAsync(invoice);
        }

        public async Task<InvoiceDto?> GetByIdAsync(long id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            return invoice == null ? null : await MapToDtoAsync(invoice);
        }

        public async Task<List<InvoiceDto>> GetAllAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return (await Task.WhenAll(invoices.Select(MapToDtoAsync))).ToList();
        }

        public async Task<List<InvoiceDto>> GetByPropertyIdAsync(long propertyId)
        {
            var invoices = await _invoiceRepository.GetByPropertyIdAsync(propertyId);
            return (await Task.WhenAll(invoices.Select(MapToDtoAsync))).ToList();
        }

        public async Task<List<InvoiceDto>> GetByUnitIdAsync(long unitId)
        {
            var invoices = await _invoiceRepository.GetByUnitIdAsync(unitId);
            return (await Task.WhenAll(invoices.Select(MapToDtoAsync))).ToList();
        }

        public async Task<List<InvoiceDto>> GetByTenantIdAsync(long tenantId)
        {
            var invoices = await _invoiceRepository.GetByTenantIdAsync(tenantId);
            return (await Task.WhenAll(invoices.Select(MapToDtoAsync))).ToList();
        }

        private async Task<InvoiceDto> MapToDtoAsync(Invoice invoice)
        {
            var transactions = await _paymentTransactionRepository.GetByUnitPaymentIdAsync(invoice.UnitPaymentsId);
            var successfulTransactions = transactions
                .Where(t => t.Status == PaymentTransactionStatus.Success)
                .OrderBy(t => t.CreatedAt)
                .Select(t => new PaymentTransactionDto
                {
                    Id = t.Id,
                    ExternalPaymentId = t.ExternalPaymentId,
                    Amount = t.Amount,
                    Status = t.Status,
                    Reference = t.Reference,
                    MpesaCode = t.MpesaCode,
                    CreatedAt = t.CreatedAt
                })
                .ToList();

            var tenant = await _tenantServiceClient.GetTenantByIdAsync(invoice.TenantId);

            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                UnitId = invoice.UnitId,
                DoorNumber = invoice.Unit?.DoorNumber,
                PropertyId = invoice.PropertyId,
                PropertyName = invoice.Property?.Name,
                PropertyLocation = invoice.Property?.Location,
                TenantId = invoice.TenantId,
                TenantName = tenant?.FullName,
                UnitPaymentsId = invoice.UnitPaymentsId,
                PeriodMonth = invoice.PeriodMonth,
                PeriodYear = invoice.PeriodYear,
                RentAmount = invoice.RentAmount,
                WaterBillAmount = invoice.WaterBillAmount,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                DueDate = invoice.DueDate,
                CreatedAt = invoice.CreatedAt,
                Transactions = successfulTransactions
            };
        }
        public async Task<(byte[] Bytes, string FileName)?> GenerateInvoicePdfAsync(long id)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null) return null;
        
            var dto = await MapToDtoAsync(invoice);
        
            byte[]? logoBytes = null;
            var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "kejahunt-logo.png");
            if (File.Exists(logoPath))
            {
                logoBytes = await File.ReadAllBytesAsync(logoPath);
            }
            else
            {
                _logger.LogWarning("Logo file not found at {Path}", logoPath);
            }
        
            var document = new InvoicePdfDocument(dto, logoBytes);
            var pdfBytes = document.GeneratePdf();
        
            var periodLabel = new DateTime(dto.PeriodYear, dto.PeriodMonth, 1).ToString("MMMM-yyyy");
            var fileName = $"{periodLabel}.pdf"; // e.g. "August-2026.pdf"
        
            return (pdfBytes, fileName);
        }    
    }
}