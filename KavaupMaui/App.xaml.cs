using KavaupMaui.Constant;
using KavaupMaui.ViewModels;
using KavaupMaui.Views;
using Microsoft.Extensions.Configuration;
using Kava.Mvvm;
using System.Reflection;
using Kava.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Kava.Logging.CrashReporter;

namespace KavaupMaui;

public partial class App : Application
{
	private IConfiguration _configuration;
	private KavaCrashReporter _crashReporter;

	public App(IConfiguration configuration, KavaCrashReporter crashReporter)
	{
		_crashReporter = crashReporter;
		_configuration = configuration;
		InitializeComponent();
		Resources["DefaultStringResources"] = new Resx.AppResources();
		var apiSettings = configuration.GetRequiredSection("APISettings").Get<APISettings>();
		MainPage = new AppShell();
    }
}

