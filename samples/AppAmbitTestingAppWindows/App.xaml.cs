using AppAmbit;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AppAmbitTestingAppWindows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppAmbitWindows.Start("<YOUR-APPKEY>");
    }
}