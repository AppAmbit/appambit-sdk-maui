namespace AppAmbitSdkCore.Utils;

public interface IIdentifiable
{
    string? Id { get; }

    public DateTime Timestamp { get; }
}
