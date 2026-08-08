using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Models.Enums;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Services.WaterBills;

public class WaterBillingService : IWaterBillingService
    {
        private readonly ApplicationDbContext _db; // transaction coordination only — all reads/writes go through repositories
        private readonly IWaterRateRepository _rateRepository;
        private readonly IWaterMeterReadingRepository _readingRepository;
        private readonly IWaterBillRepository _billRepository;
        private readonly IUnitPaymentsRepository _unitPaymentsRepository;

        public WaterBillingService(
            ApplicationDbContext db,
            IWaterRateRepository rateRepository,
            IWaterMeterReadingRepository readingRepository,
            IWaterBillRepository billRepository,
            IUnitPaymentsRepository unitPaymentsRepository)
        {
            _db = db;
            _rateRepository = rateRepository;
            _readingRepository = readingRepository;
            _billRepository = billRepository;
            _unitPaymentsRepository = unitPaymentsRepository;
        }

        public async Task<WaterRate> SetPropertyRateAsync(SetWaterRateDto dto)
        {
            if (dto.PricePerUnit <= 0)
                throw new ArgumentException("Price per unit must be greater than zero.", nameof(dto.PricePerUnit));

            return await _rateRepository.SetActiveRateAsync(dto.PropertyId, dto.PricePerUnit, dto.EffectiveFrom);
        }

        public async Task<List<WaterRate>> GetRateHistoryAsync(long propertyId) =>
            await _rateRepository.GetHistoryByPropertyIdAsync(propertyId);

        public async Task<GenerateWaterBillsResponseDto> GenerateMonthlyBillsAsync(GenerateWaterBillsRequestDto request)
        {
            if (request.Readings.Count == 0)
                throw new ArgumentException("At least one reading is required.", nameof(request.Readings));

            var activeRate = await _rateRepository.GetActiveByPropertyIdAsync(request.PropertyId)
                ?? throw new InvalidOperationException("No active water rate set for this property. Set one before generating bills.");

            var unitIds = request.Readings.Select(r => r.UnitId).Distinct().ToList();

            // Bulk-load everything needed up front — no per-unit round trips inside the loop.
            var units = await _db.Units
                .Where(u => unitIds.Contains(u.Id) && u.PropertyId == request.PropertyId)
                .ToDictionaryAsync(u => u.Id);

            var unitsWithExistingReading = await _readingRepository
                .GetUnitIdsWithReadingForPeriodAsync(unitIds, request.BillingMonth, request.BillingYear);

            var lastReadings = await _readingRepository.GetLastReadingsBulkAsync(unitIds);

            var results = new List<WaterBillResultDto>();

            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();

                foreach (var input in request.Readings)
                {
                    if (!units.TryGetValue(input.UnitId, out var unit))
                    {
                        results.Add(new WaterBillResultDto(input.UnitId, "", null, null, false, false,
                            "Unit not found on this property."));
                        continue;
                    }

                    if (unitsWithExistingReading.Contains(input.UnitId))
                    {
                        results.Add(new WaterBillResultDto(input.UnitId, unit.DoorNumber, null, null, false, false,
                            "Reading already recorded for this billing period."));
                        continue;
                    }

                    lastReadings.TryGetValue(input.UnitId, out var lastReading);
                    var isFirstReading = lastReading is null;

                    if (!isFirstReading && input.CurrentReading < lastReading!.CurrentReading)
                    {
                        results.Add(new WaterBillResultDto(input.UnitId, unit.DoorNumber, null, null, false, false,
                            $"Current reading ({input.CurrentReading}) is below the last recorded reading ({lastReading.CurrentReading})."));
                        continue;
                    }

                    var previousReading = isFirstReading ? input.CurrentReading : lastReading!.CurrentReading;
                    var unitsConsumed = isFirstReading ? 0 : input.CurrentReading - previousReading;

                    var reading = await _readingRepository.CreateAsync(new WaterMeterReading
                    {
                        UnitId = input.UnitId,
                        BillingYear = request.BillingYear,
                        BillingMonth = request.BillingMonth,
                        PreviousReading = previousReading,
                        CurrentReading = input.CurrentReading,
                        UnitsConsumed = unitsConsumed,
                        Status = isFirstReading ? WaterMeterReadingStatus.Baseline : WaterMeterReadingStatus.Confirmed
                    });

                    if (isFirstReading)
                    {
                        results.Add(new WaterBillResultDto(input.UnitId, unit.DoorNumber, 0, null, true, false, null));
                        continue;
                    }

                    var billAmount = (decimal)unitsConsumed * activeRate.PricePerUnit;

                    var bill = await _billRepository.CreateAsync(new WaterBill
                    {
                        WaterMeterReadingId = reading.Id,
                        UnitId = input.UnitId,
                        PricePerUnit = activeRate.PricePerUnit,
                        Amount = billAmount,
                        BillingYear = request.BillingYear,
                        BillingMonth = request.BillingMonth,
                        Status = WaterBillStatus.Unpaid
                    });

                    // Attach to the unit's payment record for this period, if one already exists.
                    // Tenant-driven creation of that record is out of scope here by design.
                    var appliedPayment = await _unitPaymentsRepository.ApplyWaterChargeAsync(
                        input.UnitId, request.BillingMonth, request.BillingYear, billAmount, bill.Id);

                    var wasApplied = appliedPayment != null;

                    results.Add(new WaterBillResultDto(
                        input.UnitId, unit.DoorNumber, unitsConsumed, billAmount, false, wasApplied,
                        wasApplied ? null : "No payment record exists yet for this period — bill generated but not yet applied to an expected amount."));
                }

                await transaction.CommitAsync();
            });

            return new GenerateWaterBillsResponseDto(
                BillsGenerated: results.Count(r => r.Error is null && !r.IsBaseline),
                Baselined: results.Count(r => r.IsBaseline),
                Failed: results.Count(r => r.Error is not null && !r.IsBaseline && !r.AppliedToPayment && r.UnitsConsumed is null),
                NotAppliedToPayment: results.Count(r => !r.IsBaseline && r.UnitsConsumed is not null && !r.AppliedToPayment),
                Results: results);
        }

        public async Task<List<WaterBill>> GetPropertyBillsAsync(long propertyId, int year, int month) =>
            await _billRepository.GetByPropertyAndPeriodAsync(propertyId, month, year);

        public async Task<List<WaterMeterReading>> GetUnitReadingHistoryAsync(long unitId) =>
            await _readingRepository.GetHistoryByUnitIdAsync(unitId);
    }