using KavaupMaui.Constant;
using KavaupMaui.ViewModels;
using KavaupMaui.Views;
using Microsoft.Extensions.Configuration;

namespace KavaupMaui;

public partial class App : Application
{
	private IConfiguration _configuration;
	public App(IConfiguration configuration, MainPage vm)
	{
		_configuration = configuration;
			InitializeComponent();
			Resources["DefaultStringResources"] = new Resx.AppResources();
			var apiSettings = configuration.GetRequiredSection("APISettings").Get<APISettings>();
			
   MainPage = vm;
	}
}

