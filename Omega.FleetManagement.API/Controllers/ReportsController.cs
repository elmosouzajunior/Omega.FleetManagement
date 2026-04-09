using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omega.FleetManagement.Application.Interfaces;

namespace Omega.FleetManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    [Authorize(Roles = "CompanyAdmin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportAppService _reportAppService;

        public ReportsController(IReportAppService reportAppService)
        {
            _reportAppService = reportAppService;
        }

        [HttpGet("cost-per-km")]
        public async Task<IActionResult> GetCostPerKmByVehicle([FromQuery] int? year = null)
        {
            if (!Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var companyId))
                return Unauthorized(new { message = "CompanyId inválido no token." });

            var result = await _reportAppService.GetVehicleCostPerKmAsync(companyId, year);
            return Ok(result);
        }
    }
}
