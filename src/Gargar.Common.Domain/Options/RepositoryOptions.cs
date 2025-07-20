using Microsoft.EntityFrameworkCore;

namespace Gargar.Common.Domain.Options;

public class RepositoryOptions<TDbContext> where TDbContext : DbContext
{
    public int RelatedPropertiesMaxDepth { get; set; } = 3;

    public SaveChangesStrategy SaveChangesStrategy { get; set; } = SaveChangesStrategy.PerUnitOfWork;

    public BulkCopyOptions BulkCopyOptions { get; set; } = new BulkCopyOptions();
}

public enum SaveChangesStrategy
{
    PerOperation,
    PerUnitOfWork
}