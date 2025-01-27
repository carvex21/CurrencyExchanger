using CurrencyExchanger.Api.Models;
using FluentValidation;
using FluentValidation.Validators;

namespace CurrencyExchanger.Api.Validators;

public class AdjustBalanceRequestValidator : AbstractValidator<AdjustBalanceRequest>
{
    public AdjustBalanceRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .NotEmpty()
            .WithMessage("Amount requires a positive decimal");
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Matches("^[a-zA-Z]{3}$")
            .WithMessage("Currency requires 3 alphabetic characters");
        RuleFor(x => x.Strategy)
            .NotEmpty()
            .Must(str => str is "AddFundsStrategy" or "SubtractFundsStrategy" or "ForceSubtractFundsStrategy")
            .WithMessage("Strategy must be one of the following: AddFundsStrategy, SubtractFundsStrategy, or ForceSubtractFundsStrategy");
    }
}