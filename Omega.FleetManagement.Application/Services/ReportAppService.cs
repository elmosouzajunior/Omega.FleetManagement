using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Enums;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Application.Services
{
    public class ReportAppService : IReportAppService
    {
        private readonly FleetContext _context;

        public ReportAppService(FleetContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleCostPerKmReportItemDto>> GetVehicleCostPerKmAsync(Guid companyId)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.CompanyId == companyId)
                .Select(v => new
                {
                    v.Id,
                    v.LicensePlate,
                    v.Manufacturer
                })
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();

            var kmByVehicle = await _context.Trips
                .Where(t =>
                    t.CompanyId == companyId &&
                    t.Status == TripStatus.Finished &&
                    t.FinishKm > t.StartKm)
                .GroupBy(t => t.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    TotalKm = g.Sum(t => t.FinishKm - t.StartKm)
                })
                .ToDictionaryAsync(x => x.VehicleId, x => x.TotalKm);

            var vehicleExpenseByVehicle = await _context.Expenses
                .Where(e => e.CompanyId == companyId && e.VehicleId.HasValue)
                .GroupBy(e => e.VehicleId!.Value)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    TotalExpense = g.Sum(e => e.Value)
                })
                .ToDictionaryAsync(x => x.VehicleId, x => x.TotalExpense);

            var tripExpenseByVehicle = await (
                from e in _context.Expenses
                join t in _context.Trips on e.TripId equals t.Id
                where e.CompanyId == companyId && t.CompanyId == companyId
                group e by t.VehicleId into g
                select new
                {
                    VehicleId = g.Key,
                    TotalExpense = g.Sum(e => e.Value)
                })
                .ToDictionaryAsync(x => x.VehicleId, x => x.TotalExpense);

            return vehicles.Select(v =>
            {
                var totalKm = kmByVehicle.TryGetValue(v.Id, out var km) ? km : 0m;
                var directExpense = vehicleExpenseByVehicle.TryGetValue(v.Id, out var ev) ? ev : 0m;
                var tripExpense = tripExpenseByVehicle.TryGetValue(v.Id, out var et) ? et : 0m;
                var totalExpense = directExpense + tripExpense;
                var costPerKm = totalKm > 0 ? totalExpense / totalKm : 0m;

                return new VehicleCostPerKmReportItemDto(
                    v.Id,
                    v.LicensePlate,
                    v.Manufacturer ?? string.Empty,
                    totalKm,
                    totalExpense,
                    costPerKm
                );
            }).ToList();
        }
    }
}
