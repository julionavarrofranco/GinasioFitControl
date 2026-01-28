using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System.Windows;
using System.Windows.Controls;

namespace FitControlAdmin
{
    public partial class CreateFuncionarioWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly EmployeeDisplayDto? _existing;

        public CreateFuncionarioWindow(ApiService apiService, EmployeeDisplayDto? existing = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _existing = existing;

            if (_existing != null)
            {
                Title = "Editar Funcionário";
                TitleTextBlock.Text = "Editar Funcionário";
                EmailTextBox.Text = _existing.Email;
                EmailTextBox.IsReadOnly = true;
                NomeTextBox.Text = _existing.Nome;
                TelemovelTextBox.Text = _existing.Telemovel;
                AtivoCheckBox.IsChecked = _existing.Ativo;
                AtivoCheckBox.IsEnabled = true;
                SetFuncaoSelection(_existing.Funcao.ToString());
            }
            else
            {
                Title = "Criar Funcionário";
                TitleTextBlock.Text = "Criar Funcionário";
                FuncaoComboBox.SelectedIndex = 0;
                AtivoCheckBox.IsChecked = true;
                AtivoCheckBox.IsEnabled = false;
            }
        }

        private void SetFuncaoSelection(string funcao)
        {
            for (int i = 0; i < FuncaoComboBox.Items.Count; i++)
            {
                if (FuncaoComboBox.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Content?.ToString(), funcao, System.StringComparison.OrdinalIgnoreCase))
                {
                    FuncaoComboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(NomeTextBox.Text) ||
                string.IsNullOrWhiteSpace(TelemovelTextBox.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios (Email, Nome, Telemóvel).",
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FuncaoComboBox.SelectedItem is not ComboBoxItem selectedItem ||
                string.IsNullOrWhiteSpace(selectedItem.Content?.ToString()))
            {
                MessageBox.Show("Por favor, selecione uma função.",
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_existing != null)
                {
                    var updateDto = new UpdateEmployeeDto
                    {
                        Nome = NomeTextBox.Text.Trim(),
                        Telemovel = TelemovelTextBox.Text.Trim(),
                        Funcao = selectedItem.Content.ToString()
                    };
                    var (ok, err) = await _apiService.UpdateEmployeeAsync(_existing.IdFuncionario, updateDto);
                    if (!ok)
                    {
                        MessageBox.Show(err ?? "Erro ao atualizar funcionário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var ativo = AtivoCheckBox.IsChecked ?? true;
                    if (ativo != _existing.Ativo)
                    {
                        await _apiService.ChangeUserActiveStatusAsync(_existing.IdUser, ativo);
                    }
                    MessageBox.Show("Funcionário atualizado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var registerDto = new UserRegisterDto
                    {
                        Email = EmailTextBox.Text.Trim(),
                        Tipo = "Funcionario",
                        Nome = NomeTextBox.Text.Trim(),
                        Telemovel = TelemovelTextBox.Text.Trim(),
                        Funcao = selectedItem.Content.ToString()
                    };
                    var result = await _apiService.RegisterUserAsync(registerDto);
                    if (result.Success)
                    {
                        MessageBox.Show("Funcionário criado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage ?? "Erro ao criar funcionário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
