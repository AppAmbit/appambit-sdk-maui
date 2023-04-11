using KavaupMaui.Views;

namespace KavaupMaui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
        Resources["DefaultStringResources"] = new Resx.AppResources();
        MainPage = new MainPage();
	}
}

