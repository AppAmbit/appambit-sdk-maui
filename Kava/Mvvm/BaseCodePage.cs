using Kava.Mvvm;

namespace Kava;

public class BaseCodePage<TViewModel> : ContentPage where TViewModel : BaseViewModel
{
    public TViewModel ViewModel { private set; get; }

    public BaseCodePage(TViewModel viewModel)
	{
        this.ViewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.ConsumeArguments();
    }
}