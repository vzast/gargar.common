namespace Gargar.Common.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class RepositoryAttribute : Attribute
{
    public bool CreateGenericRepository { get; set; } = true;
    public bool CreateQueryRepository { get; set; } = true;
    public bool CreateBulkRepository { get; set; } = true;
}