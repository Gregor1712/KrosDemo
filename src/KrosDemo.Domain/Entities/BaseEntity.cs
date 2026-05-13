using CsvHelper.Configuration.Attributes;

namespace KrosDemo.Domain.Entities;

public class BaseEntity
{
    [Ignore]
    public int Id { get; set; }
}