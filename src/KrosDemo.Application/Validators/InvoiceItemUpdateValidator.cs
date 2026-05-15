using FluentValidation;
using KrosDemo.Application.DTOs;

namespace KrosDemo.Application.Validators;

public class InvoiceItemUpdateValidator : AbstractValidator<InvoiceItemUpdateDTO>
{
    public InvoiceItemUpdateValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).NotEqual(0).WithMessage("UnitPrice must not be zero.");
        RuleFor(x => x.VatRate).InclusiveBetween(0, 100);
    }
}