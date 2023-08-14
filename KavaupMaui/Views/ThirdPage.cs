using Kava.Mvvm;
using KavaupMaui.ViewModels;
using Kava.Mvvm.Extensions;

namespace KavaupMaui.Views;

public class ThirdPage : BaseContentPage<ThirdVM>
{
	public ThirdPage(ThirdVM viewModel) :base(viewModel)
	{
		Content = new VerticalStackLayout
		{
			Children = {
                new Label {
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				}.BindToViewModel(viewModel, (Label.TextProperty, "Label"))
			}
		};
	}
}
