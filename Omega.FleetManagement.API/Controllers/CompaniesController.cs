using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/companies")]
    [Authorize(Roles = "Master")] // Garante que apenas o Super Admin acesse
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyAppService _companyAppService;

        public CompaniesController(ICompanyAppService companyAppService)
        {
            _companyAppService = companyAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request)
        {
            if (request == null)
                return BadRequest("Dados inválidos.");

            var result = await _companyAppService.RegisterCompanyAndAdmin(request);

            if (result)
                return Ok(new { message = "Empresa e Administrador cadastrados com sucesso!" });

            return BadRequest(new { message = "Erro ao realizar o cadastro. Verifique se o CPF já existe." });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyAppService.GetAllCompanies();

            if (companies == null || !companies.Any())
                return NoContent();
            return Ok(companies);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request)
        {
            try
            {
                var updated = await _companyAppService.UpdateCompanyAsync(id, request);
                if (!updated)
                    return NotFound(new { message = "Empresa não encontrada." });

                return Ok(new { message = "Empresa atualizada com sucesso!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro Crítico]: {ex.Message}");
                return StatusCode(500, new { message = "Erro interno no servidor", details = ex.Message });
            }
        }

    }
}
