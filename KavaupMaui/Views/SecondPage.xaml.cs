using Kava.Mvvm;
using KavaupMaui.ViewModels;

namespace KavaupMaui.Views;

public partial class SecondPage : BaseContentPage<SecondVM>
{
    public SecondPage(SecondVM vm) : base(vm)
	{
		InitializeComponent();
	}
}
