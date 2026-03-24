using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
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
                IsActive = t.IsActive
            }).ToList();
        }

        public async Task CreateAsync(CreateExpenseTypeRequest request)
        {
            if (request.CompanyId == Guid.Empty)
                throw new ArgumentException("Empresa e obrigatoria.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do tipo de despesa e obrigatorio.");

            var companyExists = _companyRepository.GetAllQueryable().Any(c => c.Id == request.CompanyId);
            if (!companyExists)
                throw new ArgumentException("Empresa nao encontrada.");

            var expenseType = new ExpenseType(request.CompanyId, request.Name.Trim(), request.Description?.Trim());
            await _expenseTypeRepository.AddAsync(expenseType);
            await _uow.CommitAsync();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateExpenseTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do tipo de despesa e obrigatorio.");

            var expenseType = await _expenseTypeRepository.GetByIdAsync(id, includeInactive: true);
            if (expenseType == null)
                return false;

            expenseType.SetName(request.Name.Trim());
            expenseType.UpdateDescription(request.Description?.Trim());

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
    }
}
