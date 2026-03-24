using Microsoft.AspNetCore.Mvc;

namespace Omega.FleetManagement.API.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult InternalServerError(ILogger logger, Exception ex, string context)
        {
            logger.LogError(ex, "Erro interno ao processar {Context}", context);
            return StatusCode(500, new { message = "Erro interno no servidor" });
        }
    }
}
