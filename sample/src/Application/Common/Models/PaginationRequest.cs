using Application.Common.Enums;
using MediatR;

namespace Application.Common.Models;

public class PaginationRequest<T> : IRequest<PaginationViewModel<T>>
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public SortType? SortType { get; set; }
    public string SortPropertyName { get; set; }
}