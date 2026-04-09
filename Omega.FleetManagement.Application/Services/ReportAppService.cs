using Microsoft.EntityFrameworkCore;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Enums;
using Omega.FleetManagement.Infrastructure.Data.Context;

namespace Omega.FleetManagement.Application.Services
{
    public class ReportAppService : IReportAppService
    {
        private const string DriverCommissionExpenseTypeName = "Comissao do Motorista";
        private const string CargoInsuranceExpenseTypeName = "Seguro da Carga";
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
                .Select(e => new
                {
                    VehicleId = e.VehicleId!.Value,
                    Month = e.Date.Month,
                    ExpenseTypeName = e.ExpenseType.Name,
                    CostCategory = (int)e.ExpenseType.CostCategory,
                    Value = e.Value
                })
                .ToListAsync();

            var expandedDirectVehicleExpenseMonthlyData = directVehicleExpenseMonthlyData
                .SelectMany(e =>
                {
                    var isFixed = e.CostCategory == (int)ExpenseCostCategory.Fixed;
                    var months = isFixed ? Enumerable.Range(1, 12) : [e.Month];
                    var dilutedValue = isFixed ? e.Value / 12m : e.Value;

                    return months.Select(month => new
                    {
                        e.VehicleId,
                        Month = month,
                        e.ExpenseTypeName,
                        e.CostCategory,
                        TotalExpense = dilutedValue
                    });
                })
                .GroupBy(e => new
                {
                    e.VehicleId,
                    e.Month,
                    e.ExpenseTypeName,
                    e.CostCategory
                })
                .Select(g => new
                {
                    g.Key.VehicleId,
                    g.Key.Month,
                    g.Key.ExpenseTypeName,
                    g.Key.CostCategory,
                    TotalExpense = g.Sum(e => e.TotalExpense)
                })
                .ToList();

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
                    ExpenseTypeName = e.ExpenseType.Name,
                    CostCategory = (int)e.ExpenseType.CostCategory
                } into g
                select new
                {
                    g.Key.VehicleId,
                    g.Key.Month,
                    g.Key.ExpenseTypeName,
                    g.Key.CostCategory,
                    TotalExpense = g.Sum(e => e.Value)
                })
                .ToListAsync();

            var tripAdditionalCostMonthlyData = await _context.Trips
                .Where(t =>
                    t.CompanyId == companyId &&
                    t.Status == TripStatus.Finished &&
                    t.UnloadingDate.HasValue &&
                    t.UnloadingDate.Value.Year == selectedYear)
                .Select(t => new
                {
                    t.VehicleId,
                    Month = t.UnloadingDate!.Value.Month,
                    t.CommissionValue,
                    t.CargoInsuranceValue
                })
                .ToListAsync();

            var kmByVehicleAndMonth = tripMonthlyData.ToDictionary(
                x => (x.VehicleId, x.Month),
                x => x.TotalKm);

            var directExpenseByVehicleMonthAndType = expandedDirectVehicleExpenseMonthlyData
                .GroupBy(x => (x.VehicleId, x.Month))
                .ToDictionary(
                    g => g.Key,
                    g => g.AsEnumerable().ToDictionary(
                        item => item.ExpenseTypeName ?? "Sem Tipo",
                        item => (item.TotalExpense, item.CostCategory)));

            var tripExpenseByVehicleMonthAndType = tripExpenseMonthlyData
                .GroupBy(x => (x.VehicleId, x.Month))
                .ToDictionary(
                    g => g.Key,
                    g => g.AsEnumerable().ToDictionary(
                        item => item.ExpenseTypeName ?? "Sem Tipo",
                        item => (item.TotalExpense, item.CostCategory)));

            var tripAdditionalCostsByVehicleMonthAndType = tripAdditionalCostMonthlyData
                .GroupBy(x => (x.VehicleId, x.Month))
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var additionalCosts = new Dictionary<string, (decimal TotalExpense, int CostCategory)>();
                        var totalCommission = g.Sum(item => item.CommissionValue);
                        var totalCargoInsurance = g.Sum(item => item.CargoInsuranceValue ?? 0m);

                        if (totalCommission > 0)
                            additionalCosts[DriverCommissionExpenseTypeName] = (totalCommission, (int)ExpenseCostCategory.Variable);

                        if (totalCargoInsurance > 0)
                            additionalCosts[CargoInsuranceExpenseTypeName] = (totalCargoInsurance, (int)ExpenseCostCategory.Variable);

                        return additionalCosts;
                    });

            var items = vehicles.Select(v =>
            {
                var annualVehicleKm = tripMonthlyData
                    .Where(item => item.VehicleId == v.Id)
                    .Sum(item => item.TotalKm);
                var dilutedFixedKmBase = annualVehicleKm > 0 ? annualVehicleKm / 12m : 0m;

                var months = Enumerable.Range(1, 12)
                    .Select(month =>
                    {
                        var totalKm = kmByVehicleAndMonth.TryGetValue((v.Id, month), out var km) ? km : 0m;
                        var directExpenseTypes = directExpenseByVehicleMonthAndType.TryGetValue((v.Id, month), out var vehicleExpenseTypes)
                            ? vehicleExpenseTypes
                            : new Dictionary<string, (decimal TotalExpense, int CostCategory)>();
                        var tripExpenseTypes = tripExpenseByVehicleMonthAndType.TryGetValue((v.Id, month), out var tripExpenseTypesValues)
                            ? tripExpenseTypesValues
                            : new Dictionary<string, (decimal TotalExpense, int CostCategory)>();
                        var tripAdditionalExpenseTypes = tripAdditionalCostsByVehicleMonthAndType.TryGetValue((v.Id, month), out var tripAdditionalExpenseTypeValues)
                            ? tripAdditionalExpenseTypeValues
                            : new Dictionary<string, (decimal TotalExpense, int CostCategory)>();

                        var mergedExpenseTypes = directExpenseTypes.Keys
                            .Concat(tripExpenseTypes.Keys)
                            .Concat(tripAdditionalExpenseTypes.Keys)
                            .Distinct()
                            .OrderBy(name => name)
                            .Select(expenseTypeName =>
                            {
                                var directExpense = directExpenseTypes.TryGetValue(expenseTypeName, out var directValue) ? directValue.TotalExpense : 0m;
                                var tripExpense = tripExpenseTypes.TryGetValue(expenseTypeName, out var tripValue) ? tripValue.TotalExpense : 0m;
                                var tripAdditionalExpense = tripAdditionalExpenseTypes.TryGetValue(expenseTypeName, out var additionalValue) ? additionalValue.TotalExpense : 0m;
                                var totalExpenseByType = directExpense + tripExpense + tripAdditionalExpense;
                                var costCategory = directExpenseTypes.TryGetValue(expenseTypeName, out var directExpenseTypeValue)
                                    ? directExpenseTypeValue.CostCategory
                                    : tripExpenseTypes.TryGetValue(expenseTypeName, out var tripExpenseTypeValue)
                                        ? tripExpenseTypeValue.CostCategory
                                        : tripAdditionalExpenseTypes.TryGetValue(expenseTypeName, out var additionalExpenseTypeValue)
                                            ? additionalExpenseTypeValue.CostCategory
                                            : (int)ExpenseCostCategory.Variable;
                                var kmBaseForCost = costCategory == (int)ExpenseCostCategory.Fixed
                                    ? dilutedFixedKmBase
                                    : totalKm;
                                var costPerKmByType = kmBaseForCost > 0 ? totalExpenseByType / kmBaseForCost : 0m;

                                return new VehicleCostPerKmExpenseTypeMetricDto(
                                    expenseTypeName,
                                    costCategory,
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

        public async Task<VehicleProfitReportDto> GetVehicleProfitAsync(Guid companyId, int? year = null)
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

            var tripResults = await _context.Trips
                .Where(t =>
                    t.CompanyId == companyId &&
                    t.Status == TripStatus.Finished &&
                    t.UnloadingDate.HasValue &&
                    t.UnloadingDate.Value.Year == selectedYear)
                .Select(t => new
                {
                    t.VehicleId,
                    t.FreightValue,
                    t.CommissionValue,
                    CargoInsuranceValue = t.CargoInsuranceValue ?? 0m,
                    TripExpense = t.Expenses.Sum(e => e.Value)
                })
                .ToListAsync();

            var directVehicleExpenses = await _context.Expenses
                .Where(e =>
                    e.CompanyId == companyId &&
                    e.VehicleId.HasValue &&
                    e.Date.Year == selectedYear)
                .Select(e => new
                {
                    VehicleId = e.VehicleId!.Value,
                    e.Value
                })
                .ToListAsync();

            var tripByVehicle = tripResults
                .GroupBy(t => t.VehicleId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Revenue = g.Sum(x => x.FreightValue),
                        CommissionExpense = g.Sum(x => x.CommissionValue),
                        CargoInsuranceExpense = g.Sum(x => x.CargoInsuranceValue),
                        TripExpense = g.Sum(x => x.TripExpense)
                    });

            var directExpenseByVehicle = directVehicleExpenses
                .GroupBy(x => x.VehicleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Value));

            var items = vehicles
                .Select(vehicle =>
                {
                    var tripData = tripByVehicle.TryGetValue(vehicle.Id, out var values)
                        ? values
                        : new
                        {
                            Revenue = 0m,
                            CommissionExpense = 0m,
                            CargoInsuranceExpense = 0m,
                            TripExpense = 0m
                        };

                    var directExpense = directExpenseByVehicle.TryGetValue(vehicle.Id, out var totalDirectExpense)
                        ? totalDirectExpense
                        : 0m;

                    var totalExpense = tripData.TripExpense + directExpense + tripData.CommissionExpense + tripData.CargoInsuranceExpense;
                    var netProfit = tripData.Revenue - totalExpense;

                    return new VehicleProfitReportItemDto(
                        vehicle.Id,
                        vehicle.LicensePlate,
                        vehicle.Manufacturer ?? string.Empty,
                        vehicle.IsActive,
                        tripData.Revenue,
                        tripData.TripExpense,
                        directExpense,
                        tripData.CommissionExpense,
                        tripData.CargoInsuranceExpense,
                        totalExpense,
                        netProfit
                    );
                })
                .OrderByDescending(item => item.NetProfit)
                .ToList();

            return new VehicleProfitReportDto(selectedYear, availableYears, items);
        }
    }
}
