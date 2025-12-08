using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AppAmbitMaui;

namespace AppAmbitTestingAppWindows
{
    public partial class CrashesPage : Page
    {
        public CrashesPage()
        {
            InitializeComponent();
            UserIdTextBox.Text = Guid.NewGuid().ToString();
            UserEmailTextBox.Text = "test@gmail.com";
            CustomLogErrorTextBox.Text = "Test Log Message";
        }

        private void onDidCrashInLastSession(object sender, EventArgs e)
        {
            bool crash = Crashes.DidCrashInLastSession().Result;
            if (crash)
            {
                ShowAlert("Crash", "Application did crash in the last session");
            }else
            {
                ShowAlert("Crash", "Application did not crash in the last session");
            }
        }

        private void onChangeUserId(object sender, RoutedEventArgs e)
        {
            Analytics.SetUserId(UserIdTextBox.Text);
            ShowAlert("Info", "User id changed");
        }

        private void onChangeUserEmail(object sender, RoutedEventArgs e)
        {
            Analytics.SetUserEmail(UserEmailTextBox.Text);
            ShowAlert("Info", "User email changed");
        }

        private void onSendCustomLogError(object sender, RoutedEventArgs e)
        {
            Crashes.LogError(CustomLogErrorTextBox.Text);
            ShowAlert("Info", "LogError Sent");
        }

        private void onSendExceptionLogError(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception();
            } catch (Exception ex) {
                Crashes.LogError(ex);
            }
            ShowAlert("Info", "LogError Sent");
        }

        private void onThrowNewCrash(object sender, RoutedEventArgs e)
        {
            throw new Exception();
        }

        private void onGeneratedTestCrash(object sender, RoutedEventArgs e)
        {
            Crashes.GenerateTestCrash();
        }

        private void ShowAlert(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
