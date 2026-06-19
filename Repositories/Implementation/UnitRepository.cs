using KejaHUnt_PropertiesAPI.Data;
using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Dto;
using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Repositories.Implementation
{
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> CreateUnitAsync(Unit unit)
        {
            await _dbContext.Units.AddAsync(unit);
            await _dbContext.SaveChangesAsync();
            return unit;
        }

        public async Task<Unit?> DeleteAync(long id)
        {
            var existingUnit = await _dbContext.Units.FirstOrDefaultAsync(x => x.Id == id);

            if (existingUnit != null)
            {
                _dbContext.Units.Remove(existingUnit);
                await _dbContext.SaveChangesAsync();
                return existingUnit;
            }
            return null;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync()
        {
            return await _dbContext.Units.ToListAsync();
        }

        public async Task<Unit?> GetUnitByIdAsync(long id)
        {
            return await _dbContext.Units.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Unit?> UpdateAsync(Unit unit)
        {
            var existingUnit = await _dbContext.Units.FirstOrDefaultAsync(x => x.Id == unit.Id);

            if (existingUnit == null)
            {
                return null;
            }

            existingUnit.Price = unit.Price;
            existingUnit.Type = unit.Type;
            existingUnit.Bathrooms = unit.Bathrooms;
            existingUnit.Size = unit.Size;
            existingUnit.Floor = unit.Floor;
            existingUnit.DoorNumber = unit.DoorNumber;
            existingUnit.Status = unit.Status;
            existingUnit.PropertyId = unit.PropertyId;

            if (!string.IsNullOrEmpty(unit.ImageUrl))
            {
                existingUnit.ImageUrl = unit.ImageUrl;
            }

            await _dbContext.SaveChangesAsync();

            return existingUnit;
        }

        public async Task<IEnumerable<Unit>> GetUnitsByPropertyIdAsync(long propertyId)
        {
            return await _dbContext.Units
                .Where(u => u.PropertyId == propertyId)
                .ToListAsync();
        }

        public async Task<Unit?> UpdateUnitStatusAsync(UnitStatusDto request)
        {
            var existingUnit = await _dbContext.Units.FirstOrDefaultAsync(x => x.Id == request.UnitId);

            if (existingUnit == null)
            {
                throw new InvalidOperationException("Unit not found.");
            }

            var currentStatus = existingUnit.Status;
            var newStatus = request.Status;

            // Idempotent — already in the target status
            if (currentStatus == newStatus)
            {
                return existingUnit;
            }

            // Allowed transitions
            bool allowed = (currentStatus, newStatus) switch
            {
                ("Available", "Reserved") => true,
                ("Available", "Occupied") => true,   // future: payment flow
                ("Reserved", "Occupied") => true,    // manager approves existing tenant
                ("Reserved", "Available") => true,   // manager rejects
                ("Occupied", "Available") => true,   // manager releases unit
                _ => false
            };

            if (!allowed)
            {
                throw new InvalidOperationException(
                    $"Cannot change unit status from '{currentStatus}' to '{newStatus}'.");
            }

            existingUnit.Status = newStatus;
            await _dbContext.SaveChangesAsync();

            return existingUnit;
        }