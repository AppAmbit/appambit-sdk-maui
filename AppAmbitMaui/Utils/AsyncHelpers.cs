using System.Diagnostics;

namespace AppAmbit;

internal static class AsyncHelpers
{
    public static void RunSync(Func<Task> task)
    {
        try
        {
            Task.Run(task).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppAmbitSdk] Exception during synchronous task execution: {ex}");
        }
    }

    public static T? RunSync<T>(Func<Task<T>> task)
    {
        try
        {
            return Task.Run(task).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppAmbitSdk] Exception during synchronous task execution: {ex}");
            return default;
        }
    }
}