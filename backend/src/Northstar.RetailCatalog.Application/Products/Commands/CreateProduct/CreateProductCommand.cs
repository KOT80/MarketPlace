using MediatR;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(CreateProductInputDto Input) : IRequest<ProductDto>;
