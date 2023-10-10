using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Kava.Helpers;
using Kava.Logging;
using Kava.Mvvm;
using KavaupMaui.Views;
using Serilog;

namespace KavaupMaui.ViewModels;

[RegisterRoute(route: "Log", pageType: typeof(LogPage))]
public class LogVM : BaseViewModel
{
    private readonly LogManager _logManager;

    private string _logEntry = "test";//string.Empty;
    private string _logs = string.Empty;

    public string LogEntry 
    {
        get => _logEntry;
        set => SetProperty(ref _logEntry, value);
    }

    public string Logs 
    {
        get => _logs; 
        set => SetProperty(ref _logs, value);
    }
    
    public ICommand EnterLogCommand { get; private set; }
    public ICommand ClearLogsCommand { get; private set; }
    
    public LogVM(LogManager logManager,
        ILogService logService)
    {
        _logManager = logManager;
        EnterLogCommand = new AsyncRelayCommand(CreateLog);
        ClearLogsCommand = new AsyncRelayCommand(ClearLogsAsync);
        Task.Run(async () => await GetLogs());
    }

    // Create a log entry from the LogEntry property
    private async Task CreateLog()
    {
        if (string.IsNullOrEmpty(LogEntry?.Trim()))
            return;
        
        _logManager.Log(LogEntry);
        LogEntry = string.Empty;
        await GetLogs();
    }
    
    private async Task GetLogs()
    {
        var logs = await _logManager.GetLogs();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Logs = string.Join(Environment.NewLine, logs);
        });
    }
    
    private async Task ClearLogsAsync()
    {
        _logManager.ClearLogs();
        await GetLogs();
    }
}