using FluentValidation;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;

public sealed class ChangePriceCommandValidator : AbstractValidator<ChangePriceCommand>
{
    public ChangePriceCommandValidator()
    {
        RuleFor(c => c.NewPrice)
            .NotNull().WithMessage("price обязательно.")
            .GreaterThanOrEqualTo(0m).WithMessage("Цена не может быть отрицательной.")
            .LessThanOrEqualTo(Product.MaxPrice)
            .WithMessage($"Цена не может превышать {Product.MaxPrice}.");

        RuleFor(c => c.NewPrice)
            .Must(static price => price is null || decimal.Round(price.Value, 2) == price.Value)
            .WithMessage("Цена допускается с точностью не более 2 знаков после запятой.");
    }
}
