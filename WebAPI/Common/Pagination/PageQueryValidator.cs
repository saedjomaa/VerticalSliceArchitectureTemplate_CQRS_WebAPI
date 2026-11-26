using System.Linq.Expressions;
using FluentValidation;

namespace WebAPI.Common.Pagination;

internal abstract class PageQueryValidator<T> : AbstractValidator<T>
{
    protected PageQueryValidator(Expression<Func<T, int>> page, Expression<Func<T, int>> pageSize)
    {
        RuleFor(page).GreaterThanOrEqualTo(Pagination.DefaultPage);
        RuleFor(pageSize).InclusiveBetween(1, Pagination.MaximumPageSize);
    }
}
