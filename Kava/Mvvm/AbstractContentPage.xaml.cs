namespace Kava.Mvvm;

public partial class AbstractContentPage : ContentPage
{
	public AbstractContentPage(BaseViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}
