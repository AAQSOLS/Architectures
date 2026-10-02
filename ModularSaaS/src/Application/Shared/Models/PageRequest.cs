using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Shared.Models;

public sealed record PageRequest(
    int PageNumber = PaginationConstants.DefaultPageNumber,
    int PageSize = PaginationConstants.DefaultPageSize)
{
    public int PageNumber { get; init; } = PageNumber < PaginationConstants.DefaultPageNumber
        ? PaginationConstants.DefaultPageNumber
        : PageNumber;

    public int PageSize { get; init; } = PageSize switch
    {
        < PaginationConstants.DefaultPageNumber => PaginationConstants.DefaultPageSize,
        > PaginationConstants.MaxPageSize => PaginationConstants.MaxPageSize,
        _ => PageSize
    };

    public int Skip => (PageNumber - 1) * PageSize;

    public int Take => PageSize;
}
