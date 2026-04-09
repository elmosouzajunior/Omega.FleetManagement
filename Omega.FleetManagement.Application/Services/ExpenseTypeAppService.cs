using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Enums;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class ExpenseTypeAppService : IExpenseTypeAppService
    {
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _uow;

        public ExpenseTypeAppService(
            IExpenseTypeRepository expenseTypeRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow)
        {
            _expenseTypeRepository = expenseTypeRepository;
            _companyRepository = companyRepository;
            _uow = uow;
        }

        public async Task<List<ExpenseTypeResponseDto>> GetExpenseTypesAsync(Guid? companyId, bool includeInactive = false)
        {
            List<ExpenseType> types;

            if (companyId.HasValue)
                types = await _expenseTypeRepository.GetByCompanyIdAsync(companyId.Value, includeInactive);
            else
                types = await _expenseTypeRepository.GetAllAsync(includeInactive);

            return types.Select(t => new ExpenseTypeResponseDto
            {
                Id = t.Id,
                CompanyId = t.CompanyId,
                Name = t.Name,
                Description = t.Description,
                CostCategory = (int)t.CostCategory,
                CostCategoryLabel = GetCostCategoryLabel(t.CostCategory),
                IsActive = t.IsActive
            }).ToList();
        }

        public async Task<ExpenseTypeResponseDto> CreateAsync(CreateExpenseTypeRequest request)
        {
            if (request.CompanyId == Guid.Empty)
                throw new ArgumentException("Empresa e obrigatoria.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do tipo de despesa e obrigatorio.");

            if (!Enum.IsDefined(typeof(ExpenseCostCategory), request.CostCategory))
                throw new ArgumentException("Categoria de custo invalida.");

            var companyExists = _companyRepository.GetAllQueryable().Any(c => c.Id == request.CompanyId);
            if (!companyExists)
                throw new ArgumentException("Empresa nao encontrada.");

            var expenseType = new ExpenseType(
                request.CompanyId,
                request.Name.Trim(),
                (ExpenseCostCategory)request.CostCategory,
                request.Description?.Trim());
            await _expenseTypeRepository.AddAsync(expenseType);
            await _uow.CommitAsync();

            return new ExpenseTypeResponseDto
            {
                Id = expenseType.Id,
                CompanyId = expenseType.CompanyId,
                Name = expenseType.Name,
                Description = expenseType.Description,
                CostCategory = (int)expenseType.CostCategory,
                CostCategoryLabel = GetCostCategoryLabel(expenseType.CostCategory),
                IsActive = expenseType.IsActive
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateExpenseTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do tipo de despesa e obrigatorio.");

            if (!Enum.IsDefined(typeof(ExpenseCostCategory), request.CostCategory))
                throw new ArgumentException("Categoria de custo invalida.");

            var expenseType = await _expenseTypeRepository.GetByIdAsync(id, includeInactive: true);
            if (expenseType == null)
                return false;

            expenseType.SetName(request.Name.Trim());
            expenseType.UpdateDescription(request.Description?.Trim());
            expenseType.SetCostCategory((ExpenseCostCategory)request.CostCategory);

            await _uow.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive)
        {
            var expenseType = await _expenseTypeRepository.GetByIdAsync(id, includeInactive: true);
            if (expenseType == null)
                return false;

            if (isActive)
                expenseType.Activate();
            else
                expenseType.Deactivate();

            await _uow.CommitAsync();
            return true;
        }

        private static string GetCostCategoryLabel(ExpenseCostCategory costCategory)
        {
            return costCategory switch
            {
                ExpenseCostCategory.Fixed => "Fixo",
                ExpenseCostCategory.Variable => "Variavel",
                _ => "Nao definido"
            };
        }
    }
}
