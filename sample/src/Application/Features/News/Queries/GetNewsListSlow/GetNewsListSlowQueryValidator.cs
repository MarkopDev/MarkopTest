﻿using FluentValidation;

namespace Application.Features.News.Queries.GetNewsListSlow;

public class GetNewsListSlowQueryValidator : AbstractValidator<GetNewsListSlowQuery>
{
    public GetNewsListSlowQueryValidator()
    {
        RuleFor(p => p.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must greater than 1.");

        RuleFor(p => p.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must greater than 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must less than 100.");
    }
}