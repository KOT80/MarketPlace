using MediatR;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Commands.ArchiveProduct;

public sealed record ArchiveProductCommand(Guid Id) : IRequest<ProductDto>;
