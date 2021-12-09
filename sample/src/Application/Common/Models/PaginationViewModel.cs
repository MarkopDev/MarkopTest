using System.Collections.Generic;

namespace Application.Common.Models;

public class PaginationViewModel<T>
{
    public int Total { get; set; }
    public ICollection<T> Data { get; set; }
}