using System;

namespace AppAmbit.Services.Auth;

internal class TokenRefreshCoordinator
{
    private bool _isRefreshing = false;
    private readonly object _lock = new();

    public async Task<bool> Execute(Func<Task<bool>> refreshLogic)  
    {
        lock (_lock) 
        {
            if (_isRefreshing) 
                return false;

            _isRefreshing = true;
        }

        try
        {
            return await refreshLogic();
        }
        finally
        {
            lock (_lock) 
            {
                _isRefreshing = false;
            }
        }
    }
}
