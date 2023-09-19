using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Kava.Mvvm;
using KavaupMaui.Views;

namespace KavaupMaui.ViewModels
{
    [RegisterRoute(route: "Third", pageType: typeof(ThirdPage))]
    public class ThirdVM : BaseViewModel
	{
        private string _label;
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public ThirdVM()
		{
            Label = "Welcome to the third Page";
        }


        public override void Initialize(object[] arguments)
        {
            Label = arguments[0] as string;
            base.Initialize(arguments);
        }


    }
}

