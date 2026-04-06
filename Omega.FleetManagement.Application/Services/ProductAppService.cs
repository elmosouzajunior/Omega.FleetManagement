using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Domain.Entities;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Application.Services
{
    public class ProductAppService : IProductAppService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _uow;

        public ProductAppService(
            IProductRepository productRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
            _uow = uow;
        }

        public async Task<List<ProductResponseDto>> GetProductsAsync(Guid? companyId, bool includeInactive = false)
        {
            List<Product> products;

            if (companyId.HasValue)
                products = await _productRepository.GetByCompanyIdAsync(companyId.Value, includeInactive);
            else
                products = await _productRepository.GetAllAsync(includeInactive);

            return products.Select(product => new ProductResponseDto
            {
                Id = product.Id,
                CompanyId = product.CompanyId,
                Name = product.Name,
                Description = product.Description,
                IsActive = product.IsActive
            }).ToList();
        }

        public async Task<ProductResponseDto> CreateAsync(CreateProductRequest request)
        {
            if (request.CompanyId == Guid.Empty)
                throw new ArgumentException("Empresa e obrigatoria.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do produto e obrigatorio.");

            var companyExists = _companyRepository.GetAllQueryable().Any(c => c.Id == request.CompanyId);
            if (!companyExists)
                throw new ArgumentException("Empresa nao encontrada.");

            var product = new Product(request.CompanyId, request.Name.Trim(), request.Description?.Trim());
            await _productRepository.AddAsync(product);
            await _uow.CommitAsync();

            return new ProductResponseDto
            {
                Id = product.Id,
                CompanyId = product.CompanyId,
                Name = product.Name,
                Description = product.Description,
                IsActive = product.IsActive
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Nome do produto e obrigatorio.");

            var product = await _productRepository.GetByIdAsync(id, includeInactive: true);
            if (product == null)
                return false;

            product.SetName(request.Name.Trim());
            product.UpdateDescription(request.Description?.Trim());

            await _uow.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive)
        {
            var product = await _productRepository.GetByIdAsync(id, includeInactive: true);
            if (product == null)
                return false;

            if (isActive)
                product.Activate();
            else
                product.Deactivate();

            await _uow.CommitAsync();
            return true;
        }
    }
}
