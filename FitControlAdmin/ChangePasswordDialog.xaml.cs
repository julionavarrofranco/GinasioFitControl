using FitControlAdmin.Services;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FitControlAdmin
{
    public partial class ChangePasswordDialog : Window
    {
        private readonly ApiService _apiService;
        private bool _isOldPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public ChangePasswordDialog(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            
            // Sync password fields
            OldPasswordBox.PasswordChanged += (s, e) =>
            {
                if (!_isOldPasswordVisible)
                {
                    OldPasswordTextBox.Text = OldPasswordBox.Password;
                }
            };
            
            OldPasswordTextBox.TextChanged += (s, e) =>
            {
                if (_isOldPasswordVisible)
                {
                    OldPasswordBox.Password = OldPasswordTextBox.Text;
                }
            };

            NewPasswordBox.PasswordChanged += (s, e) =>
            {
                if (!_isNewPasswordVisible)
                {
                    NewPasswordTextBox.Text = NewPasswordBox.Password;
                }
            };
            
            NewPasswordTextBox.TextChanged += (s, e) =>
            {
                if (_isNewPasswordVisible)
                {
                    NewPasswordBox.Password = NewPasswordTextBox.Text;
                }
            };

            ConfirmPasswordBox.PasswordChanged += (s, e) =>
            {
                if (!_isConfirmPasswordVisible)
                {
                    ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                }
            };
            
            ConfirmPasswordTextBox.TextChanged += (s, e) =>
            {
                if (_isConfirmPasswordVisible)
                {
                    ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                }
            };
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var oldPassword = _isOldPasswordVisible ? OldPasswordTextBox.Text : OldPasswordBox.Password;
            var newPassword = _isNewPasswordVisible ? NewPasswordTextBox.Text : NewPasswordBox.Password;
            var confirmPassword = _isConfirmPasswordVisible ? ConfirmPasswordTextBox.Text : ConfirmPasswordBox.Password;

            // Validation
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Por favor, preencha todos os campos.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError("A nova palavra-passe e a confirmação não coincidem.");
                return;
            }

            if (oldPassword == newPassword)
            {
                ShowError("A nova palavra-passe não pode ser igual à atual.");
                return;
            }

            ChangePasswordButton.IsEnabled = false;
            ErrorMessage.Visibility = Visibility.Collapsed;

            try
            {
                var (success, errorMessage) = await _apiService.ChangePasswordAsync(oldPassword, newPassword);

                if (success)
                {
                    MessageBox.Show("Palavra-passe alterada com sucesso!", 
                        "Sucesso", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError(errorMessage ?? "Erro ao alterar palavra-passe.");
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                ChangePasswordButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void ToggleOldPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isOldPasswordVisible = !_isOldPasswordVisible;

            if (_isOldPasswordVisible)
            {
                OldPasswordTextBox.Text = OldPasswordBox.Password;
                OldPasswordBox.Visibility = Visibility.Collapsed;
                OldPasswordTextBox.Visibility = Visibility.Visible;
                ToggleOldPasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-closed.png"));
            }
            else
            {
                OldPasswordBox.Password = OldPasswordTextBox.Text;
                OldPasswordTextBox.Visibility = Visibility.Collapsed;
                OldPasswordBox.Visibility = Visibility.Visible;
                ToggleOldPasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-open.png"));
            }
        }

        private void ToggleNewPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;

            if (_isNewPasswordVisible)
            {
                NewPasswordTextBox.Text = NewPasswordBox.Password;
                NewPasswordBox.Visibility = Visibility.Collapsed;
                NewPasswordTextBox.Visibility = Visibility.Visible;
                ToggleNewPasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-closed.png"));
            }
            else
            {
                NewPasswordBox.Password = NewPasswordTextBox.Text;
                NewPasswordTextBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;
                ToggleNewPasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-open.png"));
            }
        }

        private void ToggleConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordTextBox.Visibility = Visibility.Visible;
                ToggleConfirmPasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-closed.png"));
            }
            else
            {
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ToggleConfirmPasswordIcon.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/eye-open.png"));
            }
        }
    }
}

