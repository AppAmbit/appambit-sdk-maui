using Kava.Mvvm;
using KavaupMaui.ViewModels;

namespace KavaupMaui.Views;

public partial class MainPage : BaseContentPage<MainVM>
{
	public MainPage(MainVM vm) : base(vm) { InitializeComponent(); }

}


