using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System.Windows;
using System.Windows.Controls;

namespace FitControlAdmin.Views
{
    public partial class CreateFuncionarioWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly EmployeeDisplayDto? _existing;

        // Após guardar em edição: IdUser do funcionário (para o pai atualizar a lista)
        public int? SavedIdUser { get; private set; }
        // Após guardar em edição: estado Ativo (para o pai atualizar a lista)
        public bool? SavedAtivo { get; private set; }

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
                ParseAndSetTelemovel(_existing.Telemovel);
                AtivoCheckBox.IsChecked = _existing.Ativo;
                AtivoCheckBox.IsEnabled = true;
                SetFuncaoSelection(_existing.Funcao.ToString());
            }
            else
            {
                Title = "Criar Funcionário";
                TitleTextBlock.Text = "Criar Funcionário";
                FuncaoComboBox.SelectedIndex = 0;
                TelemovelCountryCodeComboBox.SelectedIndex = 0;
                AtivoCheckBox.IsChecked = true;
                AtivoCheckBox.IsEnabled = false;
            }
        }

        private void ParseAndSetTelemovel(string? telemovel)
        {
            if (string.IsNullOrWhiteSpace(telemovel))
            {
                TelemovelCountryCodeComboBox.SelectedIndex = 0;
                TelemovelTextBox.Text = "";
                return;
            }
            var t = telemovel.Trim();
            if (t.StartsWith("+351", StringComparison.Ordinal))
            {
                TelemovelCountryCodeComboBox.SelectedIndex = 0;
                TelemovelTextBox.Text = t.Length > 4 ? t.Substring(4).Trim() : "";
            }
            else if (t.StartsWith("+34", StringComparison.Ordinal))
            {
                TelemovelCountryCodeComboBox.SelectedIndex = 1;
                TelemovelTextBox.Text = t.Length > 3 ? t.Substring(3).Trim() : "";
            }
            else if (t.StartsWith("+44", StringComparison.Ordinal))
            {
                TelemovelCountryCodeComboBox.SelectedIndex = 2;
                TelemovelTextBox.Text = t.Length > 3 ? t.Substring(3).Trim() : "";
            }
            else
            {
                TelemovelCountryCodeComboBox.SelectedIndex = 0;
                TelemovelTextBox.Text = t;
            }
        }

        private string GetFullTelemovel()
        {
            var code = "+351";
            if (TelemovelCountryCodeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag != null)
                code = item.Tag.ToString() ?? "+351";
            var number = (TelemovelTextBox.Text ?? "").Trim().Replace(" ", "");
            return string.IsNullOrEmpty(number) ? "" : code + number;
        }

        private void SetFuncaoSelection(string funcao)
        {
            for (int i = 0; i < FuncaoComboBox.Items.Count; i++)
            {
                if (FuncaoComboBox.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Tag?.ToString(), funcao, System.StringComparison.OrdinalIgnoreCase))
                {
                    FuncaoComboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(NomeTextBox.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios (Email, Nome).",
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FuncaoComboBox.SelectedItem is not ComboBoxItem selectedItem ||
                string.IsNullOrWhiteSpace(selectedItem.Tag?.ToString()))
            {
                MessageBox.Show("Por favor, selecione uma função.",
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var funcaoValue = selectedItem.Tag.ToString();
                if (_existing != null)
                {
                    var updateDto = new UpdateEmployeeDto
                    {
                        Nome = NomeTextBox.Text.Trim(),
                        Telemovel = GetFullTelemovel(),
                        Funcao = funcaoValue
                    };
                    var (ok, err) = await _apiService.UpdateEmployeeAsync(_existing.IdFuncionario, updateDto);
                    if (!ok)
                    {
                        MessageBox.Show(err ?? "Erro ao atualizar funcionário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var ativo = AtivoCheckBox.IsChecked ?? true;
                    // Enviar estado Ativo à API para atualizar a lista
                    var (statusOk, statusErr) = await _apiService.ChangeUserActiveStatusAsync(_existing.IdUser, ativo);
                    if (!statusOk)
                    {
                        MessageBox.Show(statusErr ?? "Erro ao alterar estado ativo do funcionário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    SavedIdUser = _existing.IdUser;
                    SavedAtivo = ativo;
                    MessageBox.Show("Funcionário atualizado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var registerDto = new UserRegisterDto
                    {
                        Email = EmailTextBox.Text.Trim(),
                        Tipo = "Funcionario",
                        Nome = NomeTextBox.Text.Trim(),
                        Telemovel = GetFullTelemovel(),
                        Funcao = funcaoValue
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
