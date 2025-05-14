using System;

namespace AppAmbit.Services.Auth;

internal class TokenService
{
    public async Task<bool> TryRefreshTokenAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return false;

        const int maxRetries = 3;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            if (await ConsumerService.CreateToken())
                return true;

            retryCount++;
            await Task.Delay(1000);
        }

        return false;
    }
}
