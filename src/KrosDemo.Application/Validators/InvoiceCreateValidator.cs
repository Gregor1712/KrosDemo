using FluentValidation;
using KrosDemo.Application.DTOs;

namespace KrosDemo.Application.Validators;

public class InvoiceCreateValidator : AbstractValidator<InvoiceCreateDTO>
{
    public InvoiceCreateValidator()
    {
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerBusinessId).MaximumLength(50);
        RuleFor(x => x.IssueDate).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty().GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("DueDate must be on or after IssueDate.");
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);

        RuleForEach(x => x.Items).SetValidator(new InvoiceItemCreateValidator());
    }
}