using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.DTOs;
using Omega.FleetManagement.Application.Interfaces;
using Omega.FleetManagement.Application.Services;

namespace Omega.FleetManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/companyAdmins")]
    [Authorize(Roles = "Master")] // Garante que apenas o Super Admin acesse
    public class CompanyAdminsController : ControllerBase
    {
        private readonly ICompanyAdminAppService _companyAdminAppService;

        public CompanyAdminsController(ICompanyAdminAppService companyAdminAppService)
        {
            _companyAdminAppService = companyAdminAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var admins = await _companyAdminAppService.GetAllCompanyAdmins();

            if (admins == null || !admins.Any())
                return NoContent();
            return Ok(admins);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyAdminRequest request)
        {
            if (request == null)
                return BadRequest("Dados inválidos.");

            var result = await _companyAdminAppService.CreateCompanyAdmin(request);

            if (result)
                return Ok(new { message = "Empresa e Administrador cadastrados com sucesso!" });

            return BadRequest(new { message = "Erro ao realizar o cadastro. Verifique se o CPF já existe." });
        }

    }
}