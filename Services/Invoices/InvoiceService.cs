using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.Enums;
using KejaHUnt_PropertiesAPI.Repositories.Interface;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUnitPaymentsRepository _unitPaymentsRepository;
        private readonly IUnitRepository _unitRepository;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IUnitPaymentsRepository unitPaymentsRepository,
            IUnitRepository unitRepository)
        {
            _invoiceRepository = invoiceRepository;
            _unitPaymentsRepository = unitPaymentsRepository;
            _unitRepository = unitRepository;
        }

        public async Task<InvoiceDto> GenerateInvoiceAsync(CreateInvoiceDto dto)
        {
            var unit = await _unitRepository.GetUnitByIdAsync(dto.UnitId);
            if (unit == null)
                throw new ArgumentException($"Unit {dto.UnitId} not found");

            // Get or create the UnitPayments row for this period
            var unitPayments = await _unitPaymentsRepository.GetByUnitAndPeriodAsync(dto.UnitId, dto.PeriodMonth, dto.PeriodYear);
            if (unitPayments == null)
            {
                unitPayments = new UnitPayments
                {
                    UnitId = unit.Id,
                    PropertyId = unit.PropertyId,
                    TenantId = dto.TenantId,
                    PeriodMonth = dto.PeriodMonth,
                    PeriodYear = dto.PeriodYear,
                    ExpectedAmount = unit.Price,
                    PaidAmount = 0,
                    Status = UnitPaymentStatus.Pending
                };
                unitPayments = await _unitPaymentsRepository.CreateAsync(unitPayments);
            }

            var rentAmount = unitPayments.ExpectedAmount;
            var totalAmount = rentAmount + dto.WaterBillAmount;
            var dueDate = new DateTime(dto.PeriodYear, dto.PeriodMonth, 10);

            // Check if an invoice already exists for this unit/period — update it instead of duplicating
            var existing = await _invoiceRepository.GetByUnitAndPeriodAsync(dto.UnitId, dto.PeriodMonth, dto.PeriodYear);

            Invoice saved;
            if (existing != null)
            {
                existing.WaterBillAmount = dto.WaterBillAmount;
                existing.RentAmount = rentAmount;
                existing.TotalAmount = totalAmount;
                saved = await _invoiceRepository.UpdateAsync(existing);
            }
            else
            {
                var invoice = new Invoice
                {
                    UnitId = unit.Id,
                    PropertyId = unit.PropertyId,
                    TenantId = dto.TenantId,
                    UnitPaymentsId = unitPayments.Id,
                    PeriodMonth = dto.PeriodMonth,
                    PeriodYear = dto.PeriodYear,
                    RentAmount = rentAmount,
                    WaterBillAmount = dto.WaterBillAmount,
                    TotalAmount = totalAmount,
                    DueDate = dueDate
                };
                saved = await _invoiceRepository.CreateAsync(invoice);
            }

            // Re-fetch with Unit/Property included so the DTO has DoorNumber and PropertyName
            var refreshed = await _invoiceRepository.GetByIdAsync(saved.Id);
            return MapToDto(refreshed!);
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
                DueDate = invoice.DueDate,
                CreatedAt = invoice.CreatedAt
            };
        }
    }
}