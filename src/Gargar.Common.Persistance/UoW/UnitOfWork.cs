using Gargar.Common.Application.Interfaces;

namespace Gargar.Common.Persistance.UoW;

public sealed class UnitOfWork(IServiceProvider serviceProvider) : IUnitOfWork, IDisposable
{
    private static readonly Lock s_lock = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private UnitOfWorkScope? _currentScope;

    public async Task ExecuteAsync(Func<IUnitOfWorkScope, CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        var scope = await CreateScopeAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await action(scope, cancellationToken).ConfigureAwait(false);
            await scope.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            scope.Dispose();
        }
    }

    public async Task<IUnitOfWorkScope> CreateScopeAsync(CancellationToken cancellationToken = default)
    {
        lock (s_lock)
        {
            if (_currentScope == null || _currentScope.IsCompleted)
            {
                _currentScope = new UnitOfWorkScope(_serviceProvider);
            }
        }

        await _currentScope.BeginAsync(cancellationToken).ConfigureAwait(false);

        return _currentScope;
    }

    public void Dispose()
    {
        _currentScope?.Dispose();
    }
}