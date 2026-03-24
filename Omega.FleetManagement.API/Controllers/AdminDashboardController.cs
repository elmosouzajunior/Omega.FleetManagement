using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
    [Authorize(Roles = "CompanyAdmin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardAppService _dashboardAppService;

        public AdminDashboardController(IDashboardAppService dashboardAppService)
        {
            _dashboardAppService = dashboardAppService;
        }

        [Authorize(Roles = "CompanyAdmin")]
        [HttpGet("admin-summary")]
        public async Task<IActionResult> GetAdminSummary()
        {
            if (!TryGetCompanyId(out var companyId))
                return Unauthorized(new { message = "CompanyId inválido no token." });

            var result = await _dashboardAppService.GetAdminSummaryAsync(companyId);
            return Ok(result);
        }

        private bool TryGetCompanyId(out Guid companyId)
        {
            return Guid.TryParse(User.FindFirst("CompanyId")?.Value, out companyId);
        }
    }
}
