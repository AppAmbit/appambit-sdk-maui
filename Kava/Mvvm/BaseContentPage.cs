namespace Kava.Mvvm;

public partial class BaseContentPage<TViewModel> : AbstractContentPage where TViewModel : BaseViewModel
{
	public TViewModel ViewModel { private set; get; }

	public BaseContentPage(TViewModel viewModel) : base(viewModel)
	{
		this.ViewModel = viewModel;

	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ViewModel.ConsumeArguments();
	}
}