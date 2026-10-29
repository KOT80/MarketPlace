using MediatR;
using Microsoft.AspNetCore.Mvc;
using Northstar.RetailCatalog.Application.Categories.DTOs;
using Northstar.RetailCatalog.Application.Categories.Queries.GetCategories;

namespace Northstar.RetailCatalog.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetCategoriesQuery(), cancellationToken));
    }
}
