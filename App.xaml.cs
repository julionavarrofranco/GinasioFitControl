using System.Windows;

namespace FitControlAdmin;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}

