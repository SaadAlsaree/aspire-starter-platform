using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Entities;
using Simble.Data.Entities;

namespace Simble.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Sample> Samples => Set<Sample>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        var auditedTypes = modelBuilder.Model
            .GetEntityTypes()
            .Select(entityType => entityType.ClrType)
            .Where(clrType => typeof(BaseEntity).IsAssignableFrom(clrType));

        foreach (var clrType in auditedTypes)
        {
            var entity = modelBuilder.Entity(clrType);

            entity.Property(nameof(BaseEntity.Version)).IsConcurrencyToken();
            entity.HasQueryFilter(BuildSoftDeleteFilter(clrType));
            entity.HasIndex(nameof(BaseEntity.IsDeleted));
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var isDeleted = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));

        return Expression.Lambda(Expression.Not(isDeleted), parameter);
    }
}
