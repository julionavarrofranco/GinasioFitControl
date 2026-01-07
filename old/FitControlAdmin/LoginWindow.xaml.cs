using FitControlAdmin.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FitControlAdmin
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;
        private bool _isPasswordVisible = false;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            
            // Sync password fields
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (!_isPasswordVisible)
                {
                    PasswordTextBox.Text = PasswordBox.Password;
                }
            };
            
            PasswordTextBox.TextChanged += (s, e) =>
            {
                if (_isPasswordVisible)
                {
                    PasswordBox.Password = PasswordTextBox.Text;
                }
            };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text.Trim();
            var password = _isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;

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
            catch(Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Show password - change to closed eye icon
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                TogglePasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-closed.png"));
            }
            else
            {
                // Hide password - change to open eye icon
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                TogglePasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-open.png"));
            }
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordOverlay.Visibility = Visibility.Visible;
            ForgotPasswordEmailTextBox.Text = EmailTextBox.Text.Trim();
            ForgotPasswordMessage.Visibility = Visibility.Collapsed;
            ForgotPasswordError.Visibility = Visibility.Collapsed;
        }

        private void CancelForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordOverlay.Visibility = Visibility.Collapsed;
            ForgotPasswordEmailTextBox.Text = string.Empty;
            ForgotPasswordMessage.Visibility = Visibility.Collapsed;
            ForgotPasswordError.Visibility = Visibility.Collapsed;
        }

        // TODO: Implementar funcionalidade de envio de email
        /*
        private async void SendResetEmailButton_Click(object sender, RoutedEventArgs e)

        private void CancelForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordOverlay.Visibility = Visibility.Collapsed;
            ForgotPasswordEmailTextBox.Text = string.Empty;
            ForgotPasswordMessage.Visibility = Visibility.Collapsed;
            ForgotPasswordError.Visibility = Visibility.Collapsed;
        }

        private async void SendResetEmailButton_Click(object sender, RoutedEventArgs e)
        {
            var email = ForgotPasswordEmailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                ForgotPasswordError.Text = "Por favor, insira um email.";
                ForgotPasswordError.Visibility = Visibility.Visible;
                ForgotPasswordMessage.Visibility = Visibility.Collapsed;
                return;
            }

            SendResetEmailButton.IsEnabled = false;
            ForgotPasswordError.Visibility = Visibility.Collapsed;
            ForgotPasswordMessage.Visibility = Visibility.Collapsed;

            try
            {
                var (success, errorMessage) = await _apiService.ResetPasswordAsync(email);

                if (success)
                {
                    ForgotPasswordMessage.Text = "Email enviado com sucesso! Verifique a sua caixa de entrada.";
                    ForgotPasswordMessage.Visibility = Visibility.Visible;
                    ForgotPasswordError.Visibility = Visibility.Collapsed;
                    
                    // Auto-close after 3 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3);
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        ForgotPasswordOverlay.Visibility = Visibility.Collapsed;
                        ForgotPasswordEmailTextBox.Text = string.Empty;
                        ForgotPasswordMessage.Visibility = Visibility.Collapsed;
                        ForgotPasswordError.Visibility = Visibility.Collapsed;
                    };
                    timer.Start();
                }
                else
                {
                    ForgotPasswordError.Text = errorMessage ?? "Erro ao enviar email.";
                    ForgotPasswordError.Visibility = Visibility.Visible;
                    ForgotPasswordMessage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ForgotPasswordError.Text = ex.Message;
                ForgotPasswordError.Visibility = Visibility.Visible;
                ForgotPasswordMessage.Visibility = Visibility.Collapsed;
            }
            finally
            {
                SendResetEmailButton.IsEnabled = true;
            }
        }
        */

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}

