using System;

namespace AppAmbit.Services.Auth;

internal class TokenService
{
    public static async Task<bool> TryRefreshTokenAsync()
    {
        const int maxRetries = 3;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            if (await ConsumerService.CreateToken())
                return true;

            retryCount++;
            await Task.Delay(500);
        }

        return false;
    }
}
