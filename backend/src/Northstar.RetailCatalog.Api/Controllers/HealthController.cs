using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Infrastructure.Data;

namespace Northstar.RetailCatalog.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(RetailCatalogDbContext dbContext, ILogger<HealthController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Database unavailable");
            }

            return Ok(new { status = "Healthy", database = "Available", timestampUtc = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Database unavailable");
        }
    }
}
