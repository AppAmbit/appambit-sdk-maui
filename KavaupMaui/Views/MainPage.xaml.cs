using KavaupMaui.ViewModels;

namespace KavaupMaui.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainVM vm)
	{
		BindingContext = vm;
		InitializeComponent();
	}
}


