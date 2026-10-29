using FluentValidation;
using Northstar.RetailCatalog.Domain.Common;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Input.Sku)
            .Must(sku => !string.IsNullOrWhiteSpace(sku))
            .WithMessage("Артикул обязателен.")
            .Must(sku => SkuNormalizer.Normalize(sku).Length > 0)
            .WithMessage("Артикул должен содержать хотя бы один не пробельный символ.")
            .Must(sku => SkuNormalizer.Normalize(sku).Length <= SkuNormalizer.MaxLength)
            .WithMessage($"Длина нормализованного артикула не может превышать {SkuNormalizer.MaxLength} символов.");

        RuleFor(c => c.Input.Name)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(Product.MaxNameLength).WithMessage($"Название не может превышать {Product.MaxNameLength} символов.");

        RuleFor(c => c.Input.Description)
            .MaximumLength(Product.MaxDescriptionLength)
            .WithMessage($"Описание не может превышать {Product.MaxDescriptionLength} символов.")
            .When(c => c.Input.Description is not null);

        RuleFor(c => c.Input.CategoryId)
            .NotEmpty().WithMessage("Категория обязательна.");
    }
}
