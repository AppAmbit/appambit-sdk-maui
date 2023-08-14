using System;
namespace Kava.Mvvm.Extensions
{
    public static class View_ViewModelBinding
    {

        public static View BindToViewModel(this View view, BaseViewModel vm, params (BindableProperty, string)[] bindings)
        {
            view.BindingContext = vm;
            foreach (var binding in bindings)
            {
                BindableProperty propertyType = binding.Item1;
                string propertyName = binding.Item2;
                view.SetBinding(propertyType, propertyName, mode:BindingMode.OneWay);
            }
            return view;
        }
    }
}

