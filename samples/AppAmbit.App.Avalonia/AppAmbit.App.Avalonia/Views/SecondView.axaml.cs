using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AppAmbitTestingAppAvalonia.Views;

public partial class SecondView : UserControl
{
    private object? _previousContent;

    public SecondView(object previousContent)
    {
        InitializeComponent();
        _previousContent = previousContent;
        btnBack.Click += BtnBack_Click;
    }

    public SecondView()
    {
        InitializeComponent();
        btnBack.Click += BtnBack_Click;
    }

    private void BtnBack_Click(object? sender, RoutedEventArgs e)
    {
        var root = this.GetVisualRoot();
        if (root is Window window)
        {
            window.Content = _previousContent;
            return;
        }

        try
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ISingleViewApplicationLifetime singleView)
            {
                if (_previousContent is Control c)
                {
                    singleView.MainView = c;
                }
            }
        }
        catch {}
    }
}
