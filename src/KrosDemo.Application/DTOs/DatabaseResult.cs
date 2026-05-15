using KrosDemo.Application.Filters;

namespace KrosDemo.Application.DTOs;

public class DatabaseResult<T>
{
    public int TotalRecords { get; }
    public T Data { get; set; }

    public DatabaseResult(T data, int totalRecords)
    {
        Data = data;
        TotalRecords = totalRecords;
    }

    public PagedResponse<T> ToPagedResponse(PaginationFilter pagination)
    {
        return new PagedResponse<T>(Data, pagination, TotalRecords);
    }
}