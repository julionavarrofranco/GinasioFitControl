using FitControlAdmin.Services;
using System.Windows;

namespace FitControlAdmin
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Por favor, preencha email e palavra-passe.");
                return;
            }

            LoginButton.IsEnabled = false;
            ErrorMessage.Visibility = Visibility.Collapsed;

            try
            {
                var tokenResponse = await _apiService.LoginAsync(email, password);

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    var mainWindow = new MainWindow(_apiService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Email ou palavra-passe incorretos.");
                }
            }
            catch
            {
                ShowError("Erro ao conectar com o servidor. Verifique se o backend está rodando.");
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}

