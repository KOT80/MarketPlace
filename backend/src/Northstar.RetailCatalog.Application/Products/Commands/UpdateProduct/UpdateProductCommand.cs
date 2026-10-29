using MediatR;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, UpdateProductInputDto Input) : IRequest<ProductDto>;
