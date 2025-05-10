using AppAmbit.Services;

public static class TokenManager
{
    public static void SetToken(string token)
    {
        var apiService = new APIService();
        apiService.SetToken(token);
    }
}