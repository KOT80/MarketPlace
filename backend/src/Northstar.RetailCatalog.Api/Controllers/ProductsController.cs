using MediatR;
using Microsoft.AspNetCore.Mvc;
using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Products.Commands.ArchiveProduct;
using Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;
using Northstar.RetailCatalog.Application.Products.Commands.CreateProduct;
using Northstar.RetailCatalog.Application.Products.Commands.RestoreProduct;
using Northstar.RetailCatalog.Application.Products.Commands.UpdateProduct;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Application.Products.Queries.GetPriceHistory;
using Northstar.RetailCatalog.Application.Products.Queries.GetProductById;
using Northstar.RetailCatalog.Application.Products.Queries.GetProducts;

namespace Northstar.RetailCatalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> GetProducts(
        [FromQuery] ProductQueryRequest request, CancellationToken cancellationToken)
    {
        return await sender.Send(new GetProductsQuery(request), cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        return await sender.Send(new GetProductByIdQuery(id), cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductInputDto input, CancellationToken cancellationToken)
    {
        var dto = await sender.Send(new CreateProductCommand(input), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id, [FromBody] UpdateProductInputDto input, CancellationToken cancellationToken)
    {
        return await sender.Send(new UpdateProductCommand(id, input), cancellationToken);
    }

    [HttpPatch("{id:guid}/price")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> ChangePrice(
        Guid id, [FromBody] ChangePriceRequest request, CancellationToken cancellationToken)
    {
        return await sender.Send(new ChangePriceCommand(id, request.Price), cancellationToken);
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Archive(
        Guid id, CancellationToken cancellationToken)
    {
        return await sender.Send(new ArchiveProductCommand(id), cancellationToken);
    }

    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Restore(
        Guid id, CancellationToken cancellationToken)
    {
        return await sender.Send(new RestoreProductCommand(id), cancellationToken);
    }

    [HttpGet("{id:guid}/price-history")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductPriceHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductPriceHistoryDto>>> GetPriceHistory(
        Guid id, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new GetPriceHistoryQuery(id), cancellationToken));
    }
}
