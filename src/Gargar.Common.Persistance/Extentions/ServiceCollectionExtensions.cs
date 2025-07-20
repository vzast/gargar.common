// Copyright (C) TBC Bank. All Rights Reserved.

using Gargar.Common.Application.Interfaces;
using Gargar.Common.Domain.Attributes;
using Gargar.Common.Domain.Extentionsl;
using Gargar.Common.Domain.Internal;
using Gargar.Common.Domain.Options;
using Gargar.Common.Domain.Repository;
using Gargar.Common.Persistance.Database;
using Gargar.Common.Persistance.Repository;
using Gargar.Common.Persistance.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gargar.Common.Persistance.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfCoreDbContext<TDbContext>(this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped,
        Action<RepositoryOptions<TDbContext>>? repositoryOptions = null)
        where TDbContext : DbContext
    {
        return serviceCollection.AddEfCoreDbContext<TDbContext>((_, options) =>
        {
            optionsAction?.Invoke(options);
        }, contextLifetime, optionsLifetime, repositoryOptions);
    }

    public static IServiceCollection AddEfCoreDbContext<TDbContext>(this IServiceCollection serviceCollection,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped,
        Action<RepositoryOptions<TDbContext>>? repositoryOptions = null)
        where TDbContext : DbContext
    {
        if (CheckContextTypeRegistration<TDbContext>())
        {
            return serviceCollection;
        }

        serviceCollection.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                optionsAction?.Invoke(sp, options);
            },
            contextLifetime, optionsLifetime);

        DbContexts.AddContextType<TDbContext>();

        return RegisterRepositoryAndOptions(serviceCollection, repositoryOptions);
    }

    public static IServiceCollection AddEfCoreDbContextPool<TDbContext>(this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder> optionsAction,
        int poolSize = 1024,
        Action<RepositoryOptions<TDbContext>>? repositoryOptions = null)
        where TDbContext : DbContext
    {
        if (CheckContextTypeRegistration<TDbContext>())
        {
            return serviceCollection;
        }

        serviceCollection.AddDbContextPool<TDbContext>(optionsAction, poolSize);
        DbContexts.AddContextType<TDbContext>();
        return RegisterRepositoryAndOptions(serviceCollection, repositoryOptions);
    }

    //public static IHostApplicationBuilder AddAspireDbContextPool<TDbContext>(this IHostApplicationBuilder builder,
    //    Action<DbContextOptionsBuilder> optionsAction,
    //    int poolSize = 1024,
    //    Action<RepositoryOptions<TDbContext>>? repositoryOptions = null)
    //    where TDbContext : DbContext
    //{
    //    if (CheckContextTypeRegistration<TDbContext>())
    //    {
    //        return builder;
    //    }

    //    builder.Services.AddDbContextPool<TDbContext>(optionsAction, poolSize);
    //    DbContexts.AddContextType<TDbContext>();
    //    builder.Services.AddDbContext<AppDbContext>(options =>
    //    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    //    RegisterRepositoryAndOptions(builder.Services, repositoryOptions);
    //    return builder;
    //}

    private static bool CheckContextTypeRegistration<TDbContext>() where TDbContext : DbContext
    {
        var contextType = typeof(TDbContext);
        return DbContexts.GetContextTypes.Contains(contextType);
    }

    private static IServiceCollection RegisterRepositoryAndOptions<TDbContext>(IServiceCollection serviceCollection, Action<RepositoryOptions<TDbContext>>? repositoryOptions) where TDbContext : DbContext
    {
        var repoOpts = new RepositoryOptions<TDbContext>();
        repositoryOptions?.Invoke(repoOpts);
        serviceCollection.AddSingleton(repoOpts);

        AddRepositories(serviceCollection, typeof(TDbContext));
        AddBulkRepositoryServices(serviceCollection);
        return serviceCollection;
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped(x => new Lazy<IUnitOfWork>(x.GetRequiredService<IUnitOfWork>));

        return serviceCollection;
    }

    private static void AddBulkRepositoryServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IPropertyGetterCache, PropertyGetterCache>();
        serviceCollection.AddSingleton<EntityPropertiesProvider, EntityPropertiesProvider>();
        serviceCollection.AddLogging();
    }

    internal static void AddRepositories(IServiceCollection serviceCollection, Type dbContextType)
    {
        var repoInterfaceType = typeof(IRepository<>);
        var repoImplementationType = typeof(EfCoreRepositoryBase<,>);
        var queryRepoInterfaceType = typeof(IQueryRepository<>);
        var queryRepoImplementationType = typeof(EfCoreQueryRepositoryBase<,>);
        //var bulkRepoInterfaceType = typeof(IBulkRepository<>);
        //var bulkRepoImplementationType = typeof(BulkRepository<,>);

        var lazyType = typeof(Lazy<>);
        var lazyRepoType = typeof(LazyRepository<>);
        var lazyQueryRepositoryType = typeof(LazyQueryRepository<>);
        //var lazyBulkRepositoryType = typeof(LazyBulkRepository<>);

        foreach (var entityType in GetGenericRepoTypes(dbContextType))
        {
            var genericRepoInterfaceType = repoInterfaceType.MakeGenericType(entityType);
            if (serviceCollection.Any(x => x.ServiceType == genericRepoInterfaceType))
                continue;

            var genericRepoImplementationType = repoImplementationType.MakeGenericType(dbContextType, entityType);
            serviceCollection.AddScoped(genericRepoInterfaceType, genericRepoImplementationType);
            serviceCollection.AddScoped(lazyType.MakeGenericType(genericRepoInterfaceType), lazyRepoType.MakeGenericType(entityType));
        }

        foreach (var entityType in GetQueryRepoTypes(dbContextType))
        {
            var genericRepoInterfaceType = queryRepoInterfaceType.MakeGenericType(entityType);
            if (serviceCollection.Any(x => x.ServiceType == genericRepoInterfaceType))
                continue;

            var genericRepoImplementationType = queryRepoImplementationType.MakeGenericType(dbContextType, entityType);
            serviceCollection.AddScoped(genericRepoInterfaceType, genericRepoImplementationType);
            serviceCollection.AddScoped(lazyType.MakeGenericType(genericRepoInterfaceType), lazyQueryRepositoryType.MakeGenericType(entityType));
        }

        //foreach (var entityType in GetBulkRepoTypes(dbContextType))
        //{
        //    var genericRepoInterfaceType = bulkRepoInterfaceType.MakeGenericType(entityType);
        //    if (serviceCollection.Any(x => x.ServiceType == genericRepoInterfaceType))
        //        continue;

        //    var genericRepoImplementationType = bulkRepoImplementationType.MakeGenericType(dbContextType, entityType);
        //    serviceCollection.AddScoped(genericRepoInterfaceType, genericRepoImplementationType);
        //    serviceCollection.AddScoped(lazyType.MakeGenericType(genericRepoInterfaceType), lazyBulkRepositoryType.MakeGenericType(entityType));
        //}
    }

    private static IEnumerable<Type> GetGenericRepoTypes(Type dbContextType)
    {
        return dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.PropertyType.IsAssignableToGenericType(typeof(DbSet<>))
                        && x.GetCustomAttributes<RepositoryAttribute>().FirstOrDefault(y => !y.CreateGenericRepository) == null)
            .Select(x => x.PropertyType.GenericTypeArguments[0]);
    }

    private static IEnumerable<Type> GetQueryRepoTypes(Type dbContextType)
    {
        return dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.PropertyType.IsAssignableToGenericType(typeof(DbSet<>))
                        && x.GetCustomAttributes<RepositoryAttribute>().FirstOrDefault(y => !y.CreateQueryRepository) == null
            ).Select(x => x.PropertyType.GenericTypeArguments[0]);
    }

    private static IEnumerable<Type> GetBulkRepoTypes(Type dbContextType)
    {
        return dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.PropertyType.IsAssignableToGenericType(typeof(DbSet<>))
                        && x.GetCustomAttributes<RepositoryAttribute>().FirstOrDefault(y => !y.CreateBulkRepository) == null
            ).Select(x => x.PropertyType.GenericTypeArguments[0]);
    }
}

internal class LazyRepository<T>(IServiceProvider provider) : Lazy<IRepository<T>>(provider.GetRequiredService<IRepository<T>>) where T : class
{
}

internal class LazyQueryRepository<T>(IServiceProvider provider) : Lazy<IQueryRepository<T>>(provider.GetRequiredService<IQueryRepository<T>>) where T : class
{
}

//internal class LazyBulkRepository<T> : Lazy<IBulkRepository<T>> where T : class
//{
//    public LazyBulkRepository(IServiceProvider provider)
//        : base(provider.GetRequiredService<IBulkRepository<T>>)
//    {
//    }
//}