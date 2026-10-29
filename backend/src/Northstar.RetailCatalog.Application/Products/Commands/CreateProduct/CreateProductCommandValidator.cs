using FluentValidation;
using Northstar.RetailCatalog.Domain.Common;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
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

        RuleFor(c => c.Input.Price)
            .GreaterThanOrEqualTo(0m).WithMessage("Цена не может быть отрицательной.")
            .LessThanOrEqualTo(999_999_999_999.99m)
            .WithMessage("Цена не может превышать 999 999 999 999.99 (ограничение numeric(18,2) с запасом).");

        RuleFor(c => c.Input.Price)
            .Must(static price => decimal.Round(price, 2) == price)
            .WithMessage("Цена допускается с точностью не более 2 знаков после запятой.");
    }
}
