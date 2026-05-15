using KrosDemo.Application.Filters;

namespace KrosDemo.Application.DTOs;

public class PagedResponse<T>
{
    public T Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }

    public PagedResponse(T data, int pageNumber = 0, int pageSize = 0, int totalRecords = 0)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
    }

    public PagedResponse(T data, PaginationFilter pagination, int totalRecords)
    {
        Data = data; 
        PageNumber = pagination.PageNumber ?? 1;
        PageSize = pagination.PageSize ?? totalRecords;
        TotalRecords = totalRecords;
    }
}