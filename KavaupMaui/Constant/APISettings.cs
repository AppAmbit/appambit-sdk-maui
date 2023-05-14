namespace KavaupMaui.Constant;

public class APISettings
{
  public static string Url { get; set; }
  public static AuthSettings AuthSettings { get; set; }
}
public class AuthSettings
{
  public static string Domain { get; set; }
  public static string ClientId { get; set; }
  public static string Scope { get; set; }
  public static string RedirectUri { get; set; }
}
