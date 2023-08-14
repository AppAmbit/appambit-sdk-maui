using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Kava.Mvvm;
using KavaupMaui.Views;

namespace KavaupMaui.ViewModels
{
	[RegisterRoute(route:"Second", pageType:typeof(SecondPage))]
	public class SecondVM : BaseViewModel
    {
        public ICommand GoToNextPage { get; private set; }
        public SecondVM()
		{
            GoToNextPage = new AsyncRelayCommand(GoToThirdPage);

        }

        public async Task GoToThirdPage()
        {
            await Shell.Current.Navigate(typeof(ThirdVM), "hi");
        }
    }
}

