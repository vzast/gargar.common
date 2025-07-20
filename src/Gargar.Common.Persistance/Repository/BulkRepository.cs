//using Gargar.Common.Domain.Internal;
//using Gargar.Common.Domain.Repository;
//using Gargar.Common.Persistance.Extentions;
//using Gargar.Common.Persistance.Options;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Gargar.Common.Persistance.Repository;
//public class BulkRepository<TDbContext, TEntity> : IBulkRepository<TEntity>
//       where TEntity : class
//       where TDbContext : DbContext
//{
//    protected readonly TDbContext Context;
//    protected readonly RepositoryOptions<TDbContext> RepositoryOptions;
//    protected readonly IEntityPropertiesProvider EntityPropertiesProvider;
//    protected readonly IPropertyGetterCache PropertyGetterCache;

//    public BulkRepository(
//        TDbContext context,
//        RepositoryOptions<TDbContext> repositoryOptions,
//        IEntityPropertiesProvider entityPropertiesProvider,
//        IPropertyGetterCache propertyGetterCache)
//    {
//        Context = context;
//        RepositoryOptions = repositoryOptions;
//        EntityPropertiesProvider = entityPropertiesProvider;
//        PropertyGetterCache = propertyGetterCache;
//    }

//    public async Task BulkInsertAsync(
//        IEnumerable<TEntity> entities,
//        Action<BulkInsertOptions<TEntity>>? bulkOptionsConfig = null,
//        Action<BulkCopyOptions>? bulkCopyOptionsConfig = null,
//        CancellationToken cancellationToken = default)
//    {
//        var entityList = entities.ToList(); // Materialize the enumerable into a list

//        if (entityList.Count == 0)
//            return;

//        var entityType = GetEntityType(entityList);
//        var tableName = entityType.GetTableNameWithSchema();
//        var bulkCopyOptions = GetBulkCopyOptions(bulkCopyOptionsConfig);
//        var bulkOptions = new BulkInsertOptions<TEntity>();
//        bulkOptionsConfig?.Invoke(bulkOptions);

//        var propertiesToInsert = EntityPropertiesProvider.GetPropertiesForInsert(entityType, bulkOptions.ToBulkLocalOptions());
//        var columNames = propertiesToInsert.Select(x => x.Property).GetColumnNames(entityType).ToArray();

//        var reader = new EntityDataReader<TEntity>(Context, PropertyGetterCache, entityList, propertiesToInsert, entityType.GetStoreObject());

//        // Execution
//        await Context.ExecuteInTransactionAsync(bulkCopyOptions, async (connection, transaction) =>
//        {
//            await reader
//                .UseSqlBulkCopyAsync(tableName, connection, transaction, bulkCopyOptions, columNames, cancellationToken)
//                .ConfigureAwait(false);
//        }, cancellationToken).ConfigureAwait(false);
//    }

//    public async Task BulkUpdateAsync(
//        IEnumerable<TEntity> entities,
//        Action<BulkUpdateOptions<TEntity>>? bulkOptionsConfig = null,
//        Action<BulkCopyOptions>? bulkCopyOptionsConfig = null,
//        CancellationToken cancellationToken = default)
//    {
//        var entityList = entities.ToList(); // Materialize the enumerable into a list

//        if (entityList.Count == 0)
//            return;

//        var entityType = GetEntityType(entityList);
//        var bulkCopyOptions = GetBulkCopyOptions(bulkCopyOptionsConfig);
//        var bulkOptions = new BulkUpdateOptions<TEntity>();
//        bulkOptionsConfig?.Invoke(bulkOptions);

//        var propertiesForUpdate = EntityPropertiesProvider.GetPropertiesForUpdate(entityType, bulkOptions.ToBulkLocalOptions());
//        var columNames = propertiesForUpdate.Select(x => x.Property).GetColumnNames(entityType).ToArray();
//        var updateByPropertiesColumNames = EntityPropertiesProvider
//            .GetPrimaryKeysOrPropertiesForMerge(entityType, bulkOptions.ToBulkLocalOptions())
//            .Select(x => x.Property)
//            .GetColumnNames(entityType)
//            .ToArray();

//        var reader = new EntityDataReader<TEntity>(Context, PropertyGetterCache, entityList, propertiesForUpdate, entityType.GetStoreObject());

//        var entityTableName = entityType.GetTableNameWithSchema();
//        var tempTableName = "#Temp_Update_".AddGuid();
//        var createTempTableSql = QueryBuilderHelper.GetCreateTempTableQuery(entityTableName, tempTableName, columNames);

//        // Execution
//        await Context.ExecuteInTransactionAsync(bulkCopyOptions, async (connection, transaction) =>
//        {
//            await createTempTableSql.RunSqlCommand(connection, transaction, cancellationToken).ConfigureAwait(false);

//            await reader.UseSqlBulkCopyAsync(tempTableName, connection, transaction, bulkCopyOptions, columNames, cancellationToken).ConfigureAwait(false);

//            var updateQuery = QueryBuilderHelper
//                .GetMergeUpdateQuery(
//                    targetTableName: entityTableName,
//                    tempTableName: tempTableName,
//                    columNames: columNames.Except(updateByPropertiesColumNames),
//                    primaryKeysNames: updateByPropertiesColumNames);

//            await updateQuery.RunSqlCommand(connection, transaction, cancellationToken).ConfigureAwait(false);
//        }, cancellationToken).ConfigureAwait(false);
//    }

//    public async Task BulkDeleteAsync(
//        IEnumerable<TEntity> entities,
//        Action<BulkDeleteOptions<TEntity>>? bulkOptionsConfig = null,
//        Action<BulkCopyOptions>? bulkCopyOptionsConfig = null,
//        CancellationToken cancellationToken = default)
//    {
//        var entityList = entities.ToList(); // Materialize the enumerable into a list

//        if (entityList.Count == 0)
//            return;

//        var entityType = GetEntityType(entityList);
//        var bulkCopyOptions = GetBulkCopyOptions(bulkCopyOptionsConfig);
//        var bulkOptions = new BulkDeleteOptions<TEntity>();
//        bulkOptionsConfig?.Invoke(bulkOptions);

//        var deleteByProperties = EntityPropertiesProvider.GetPrimaryKeysOrPropertiesForMerge(entityType, bulkOptions.ToBulkLocalOptions());
//        var deleteByPropertiesColumNames = deleteByProperties.Select(x => x.Property).GetColumnNames(entityType).ToArray();
//        var reader = new EntityDataReader<TEntity>(Context, PropertyGetterCache, entityList, deleteByProperties, entityType.GetStoreObject());

//        var entityTableName = entityType.GetTableNameWithSchema();
//        var tempTableName = "#Temp_Delete_".AddGuid();
//        var createTempTableSql = QueryBuilderHelper.GetCreateTempTableQuery(entityTableName, tempTableName, deleteByPropertiesColumNames);

//        // Execution
//        await Context.ExecuteInTransactionAsync(bulkCopyOptions, async (connection, transaction) =>
//        {
//            await createTempTableSql.RunSqlCommand(connection, transaction, cancellationToken).ConfigureAwait(false);

//            await reader.UseSqlBulkCopyAsync(tempTableName, connection, transaction, bulkCopyOptions, deleteByPropertiesColumNames, cancellationToken).ConfigureAwait(false);

//            var deleteQuery = QueryBuilderHelper.GetMergeDeleteQuery(
//                targetTableName: entityTableName,
//                tempTableName: tempTableName,
//                primaryKeysNames: deleteByPropertiesColumNames);

//            await deleteQuery.RunSqlCommand(connection, transaction, cancellationToken).ConfigureAwait(false);
//        }, cancellationToken).ConfigureAwait(false);
//    }

//    private IEntityType GetEntityType(IEnumerable<TEntity> entities)
//    {
//        var type = entities.First().GetType();

//        return Context.Model.FindEntityType(type)
//               ?? throw new InvalidOperationException($"Entity type {type.Name} not found in the context model.");
//    }

//    private BulkCopyOptions GetBulkCopyOptions(Action<BulkCopyOptions>? bulkCopyOptionsConfig)
//    {
//        if (bulkCopyOptionsConfig == null)
//            return RepositoryOptions.BulkCopyOptions;

//        var options = new BulkCopyOptions();
//        bulkCopyOptionsConfig(options);

//        return options;
//    }
//}