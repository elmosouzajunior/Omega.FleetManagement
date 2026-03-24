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

            return BadRequest(new { message = "Erro ao realizar o cadastro. Verifique se o e-mail já existe." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyAdminRequest request)
        {
            try
            {
                var updated = await _companyAdminAppService.UpdateCompanyAdminAsync(id, request);
                if (!updated)
                    return NotFound(new { message = "Administrador não encontrado." });

                return Ok(new { message = "Administrador atualizado com sucesso!" });
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

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                var deactivated = await _companyAdminAppService.DeactivateCompanyAdminAsync(id);
                if (!deactivated)
                    return NotFound(new { message = "Administrador não encontrado." });

                return Ok(new { message = "Administrador desativado com sucesso!" });
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

        [HttpPatch("{id}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id)
        {
            try
            {
                var reactivated = await _companyAdminAppService.ReactivateCompanyAdminAsync(id);
                if (!reactivated)
                    return NotFound(new { message = "Administrador não encontrado." });

                return Ok(new { message = "Administrador reativado com sucesso!" });
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
