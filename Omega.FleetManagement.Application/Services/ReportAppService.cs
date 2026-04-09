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

        public async Task<VehicleCostPerKmReportDto> GetVehicleCostPerKmAsync(Guid companyId, int? year = null)
        {
            var currentYear = DateTime.UtcNow.Year;
            var selectedYear = year ?? currentYear;

            var tripYears = await _context.Trips
                .Where(t =>
                    t.CompanyId == companyId &&
                    t.Status == TripStatus.Finished &&
                    t.UnloadingDate.HasValue)
                .Select(t => t.UnloadingDate!.Value.Year)
                .Distinct()
                .ToListAsync();

            var expenseYears = await _context.Expenses
                .Where(e =>
                    e.CompanyId == companyId &&
                    e.Date != default)
                .Select(e => e.Date.Year)
                .Distinct()
                .ToListAsync();

            var availableYears = tripYears
                .Concat(expenseYears)
                .Append(currentYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            var vehicles = await _context.Vehicles
                .Where(v => v.CompanyId == companyId)
                .Select(v => new
                {
                    v.Id,
                    v.LicensePlate,
                    v.Manufacturer,
                    v.IsActive
                })
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();

            var tripMonthlyData = await _context.Trips
                .Where(t =>
                    t.CompanyId == companyId &&
                    t.Status == TripStatus.Finished &&
                    t.FinishKm > t.StartKm &&
                    t.UnloadingDate.HasValue &&
                    t.UnloadingDate.Value.Year == selectedYear)
                .GroupBy(t => new
                {
                    t.VehicleId,
                    Month = t.UnloadingDate!.Value.Month
                })
                .Select(g => new
                {
                    g.Key.VehicleId,
                    g.Key.Month,
                    TotalKm = g.Sum(t => t.FinishKm - t.StartKm)
                })
                .ToListAsync();

            var directVehicleExpenseMonthlyData = await _context.Expenses
                .Where(e =>
                    e.CompanyId == companyId &&
                    e.VehicleId.HasValue &&
                    e.Date.Year == selectedYear)
                .GroupBy(e => new
                {
                    VehicleId = e.VehicleId!.Value,
                    Month = e.Date.Month,
                    ExpenseTypeName = e.ExpenseType.Name
                })
                .Select(g => new
                {
                    g.Key.VehicleId,
                    g.Key.Month,
                    g.Key.ExpenseTypeName,
                    TotalExpense = g.Sum(e => e.Value)
                })
                .ToListAsync();

            var tripExpenseMonthlyData = await (
                from e in _context.Expenses
                join t in _context.Trips on e.TripId equals t.Id
                where e.CompanyId == companyId
                    && t.CompanyId == companyId
                    && t.Status == TripStatus.Finished
                    && t.UnloadingDate.HasValue
                    && t.UnloadingDate.Value.Year == selectedYear
                group e by new
                {
                    t.VehicleId,
                    Month = t.UnloadingDate!.Value.Month,
                    ExpenseTypeName = e.ExpenseType.Name
                } into g
                select new
                {
                    g.Key.VehicleId,
                    g.Key.Month,
                    g.Key.ExpenseTypeName,
                    TotalExpense = g.Sum(e => e.Value)
                })
                .ToListAsync();

            var kmByVehicleAndMonth = tripMonthlyData.ToDictionary(
                x => (x.VehicleId, x.Month),
                x => x.TotalKm);

            var directExpenseByVehicleMonthAndType = directVehicleExpenseMonthlyData
                .GroupBy(x => (x.VehicleId, x.Month))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(
                        item => item.ExpenseTypeName ?? "Sem Tipo",
                        item => item.TotalExpense));

            var tripExpenseByVehicleMonthAndType = tripExpenseMonthlyData
                .GroupBy(x => (x.VehicleId, x.Month))
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(
                        item => item.ExpenseTypeName ?? "Sem Tipo",
                        item => item.TotalExpense));

            var items = vehicles.Select(v =>
            {
                var months = Enumerable.Range(1, 12)
                    .Select(month =>
                    {
                        var totalKm = kmByVehicleAndMonth.TryGetValue((v.Id, month), out var km) ? km : 0m;
                        var directExpenseTypes = directExpenseByVehicleMonthAndType.TryGetValue((v.Id, month), out var vehicleExpenseTypes)
                            ? vehicleExpenseTypes
                            : new Dictionary<string, decimal>();
                        var tripExpenseTypes = tripExpenseByVehicleMonthAndType.TryGetValue((v.Id, month), out var tripExpenseTypesValues)
                            ? tripExpenseTypesValues
                            : new Dictionary<string, decimal>();

                        var mergedExpenseTypes = directExpenseTypes.Keys
                            .Concat(tripExpenseTypes.Keys)
                            .Distinct()
                            .OrderBy(name => name)
                            .Select(expenseTypeName =>
                            {
                                var directExpense = directExpenseTypes.TryGetValue(expenseTypeName, out var directValue) ? directValue : 0m;
                                var tripExpense = tripExpenseTypes.TryGetValue(expenseTypeName, out var tripValue) ? tripValue : 0m;
                                var totalExpenseByType = directExpense + tripExpense;
                                var costPerKmByType = totalKm > 0 ? totalExpenseByType / totalKm : 0m;

                                return new VehicleCostPerKmExpenseTypeMetricDto(
                                    expenseTypeName,
                                    totalExpenseByType,
                                    costPerKmByType
                                );
                            })
                            .ToList();

                        var totalExpense = mergedExpenseTypes.Sum(item => item.TotalExpense);
                        var costPerKm = totalKm > 0 ? totalExpense / totalKm : 0m;

                        return new VehicleCostPerKmMonthlyMetricDto(
                            month,
                            totalKm,
                            totalExpense,
                            costPerKm,
                            mergedExpenseTypes
                        );
                    })
                    .ToList();

                var annualTotalKm = months.Sum(m => m.TotalKm);
                var annualTotalExpense = months.Sum(m => m.TotalExpense);
                var annualAverageCostPerKm = annualTotalKm > 0 ? annualTotalExpense / annualTotalKm : 0m;

                return new VehicleCostPerKmReportItemDto(
                    v.Id,
                    v.LicensePlate,
                    v.Manufacturer ?? string.Empty,
                    v.IsActive,
                    months,
                    annualAverageCostPerKm
                );
            }).ToList();

            return new VehicleCostPerKmReportDto(selectedYear, availableYears, items);
        }
    }
}
