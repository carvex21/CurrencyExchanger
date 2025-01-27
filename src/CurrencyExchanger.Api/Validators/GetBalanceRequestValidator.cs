using CurrencyExchanger.Api.Models;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace CurrencyExchanger.Api.Validators;

public class GetBalanceRequestValidator: AbstractValidator<GetBalanceRequest>
{
    public GetBalanceRequestValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Matches("^[a-zA-Z]{3}$")
            .WithMessage("Currency requires 3 alphabetic characters");
    }
}