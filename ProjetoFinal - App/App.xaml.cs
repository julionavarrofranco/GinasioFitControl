using System.Windows;
using FitControlAdmin.Views;

namespace FitControlAdmin;

// Entrada da aplicação; abre a janela de login
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}

