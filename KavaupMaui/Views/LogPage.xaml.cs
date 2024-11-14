using Kava.Mvvm;
using KavaupMaui.ViewModels;

namespace KavaupMaui.Views;

public partial class LogPage : BaseContentPage<LogVM>
{
public LogPage(LogVM vm) : base(vm) { InitializeComponent(); }

}
