using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class ReceiptDocumentTypeAppService : IReceiptDocumentTypeAppService
    {
        private readonly IReceiptDocumentTypeRepository _receiptDocumentTypeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _uow;

        public ReceiptDocumentTypeAppService(
            IReceiptDocumentTypeRepository receiptDocumentTypeRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow)
        {
            _receiptDocumentTypeRepository = receiptDocumentTypeRepository;
            _companyRepository = companyRepository;
            _uow = uow;
        }

        public async Task<List<ReceiptDocumentTypeResponseDto>> GetReceiptDocumentTypesAsync(Guid? companyId, bool includeInactive = false)
        {
            List<ReceiptDocumentType> items;

            if (companyId.HasValue)
                items = await _receiptDocumentTypeRepository.GetByCompanyIdAsync(companyId.Value, includeInactive);
            else
                items = await _receiptDocumentTypeRepository.GetAllAsync(includeInactive);

            return items.Select(item => new ReceiptDocumentTypeResponseDto
            {
                Id = item.Id,
                CompanyId = item.CompanyId,
                Name = item.Name,
                Description = item.Description,
                IsActive = item.IsActive
            }).ToList();
        }

        public async Task<ReceiptDocumentTypeResponseDto> CreateAsync(CreateReceiptDocumentTypeRequest request)
        {
            if (request.CompanyId == Guid.Empty)
                throw new ArgumentException("Empresa e obrigatoria.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do tipo de documento e obrigatorio.");

            var companyExists = _companyRepository.GetAllQueryable().Any(c => c.Id == request.CompanyId);
            if (!companyExists)
                throw new ArgumentException("Empresa nao encontrada.");

            var item = new ReceiptDocumentType(request.CompanyId, request.Name.Trim(), request.Description?.Trim());
            await _receiptDocumentTypeRepository.AddAsync(item);
            await _uow.CommitAsync();

            return new ReceiptDocumentTypeResponseDto
            {
                Id = item.Id,
                CompanyId = item.CompanyId,
                Name = item.Name,
                Description = item.Description,
                IsActive = item.IsActive
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateReceiptDocumentTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do tipo de documento e obrigatorio.");

            var item = await _receiptDocumentTypeRepository.GetByIdAsync(id, includeInactive: true);
            if (item == null)
                return false;

            item.SetName(request.Name.Trim());
            item.UpdateDescription(request.Description?.Trim());
            await _uow.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive)
        {
            var item = await _receiptDocumentTypeRepository.GetByIdAsync(id, includeInactive: true);
            if (item == null)
                return false;

            if (isActive) item.Activate();
            else item.Deactivate();

            await _uow.CommitAsync();
            return true;
        }
    }
}
