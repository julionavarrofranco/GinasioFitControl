using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Windows;

namespace FitControlAdmin
{
    public partial class CreateEditUserWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly UserDto? _existingUser;
        private readonly bool _isEditMode;

        public CreateEditUserWindow(ApiService apiService, UserDto? existingUser = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _existingUser = existingUser;
            _isEditMode = existingUser != null;

            if (_isEditMode)
            {
                TitleText.Text = "Editar Utilizador";
                LoadUserData();
            }

            TipoComboBox.SelectionChanged += TipoComboBox_SelectionChanged;
            TipoComboBox_SelectionChanged(null, null);
        }

        private void LoadUserData()
        {
            if (_existingUser == null) return;

            EmailTextBox.Text = _existingUser.Email;
            EmailTextBox.IsReadOnly = true;
            NomeTextBox.Text = _existingUser.Nome;
            TelemovelTextBox.Text = _existingUser.Telemovel;
            AtivoCheckBox.IsChecked = _existingUser.Ativo;

            if (_existingUser.Tipo == "Membro")
            {
                TipoComboBox.SelectedIndex = 0;
                if (_existingUser.DataNascimento.HasValue)
                    DataNascimentoDatePicker.SelectedDate = _existingUser.DataNascimento.Value;
                if (_existingUser.IdSubscricao.HasValue)
                    IdSubscricaoTextBox.Text = _existingUser.IdSubscricao.Value.ToString();
            }
            else
            {
                TipoComboBox.SelectedIndex = 1;
                if (!string.IsNullOrEmpty(_existingUser.Funcao))
                {
                    for (int i = 0; i < FuncaoComboBox.Items.Count; i++)
                    {
                        if (FuncaoComboBox.Items[i] is System.Windows.Controls.ComboBoxItem item &&
                            item.Content.ToString() == _existingUser.Funcao)
                        {
                            FuncaoComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        private void TipoComboBox_SelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs? e)
        {
            if (TipoComboBox.SelectedIndex == 0) // Membro
            {
                MembroPanel.Visibility = Visibility.Visible;
                FuncionarioPanel.Visibility = Visibility.Collapsed;
            }
            else // Funcionario
            {
                MembroPanel.Visibility = Visibility.Collapsed;
                FuncionarioPanel.Visibility = Visibility.Visible;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(NomeTextBox.Text) ||
                string.IsNullOrWhiteSpace(TelemovelTextBox.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", 
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _existingUser != null)
                {
                    // Atualizar
                    var updateDto = new UserUpdateDto
                    {
                        Nome = NomeTextBox.Text,
                        Telemovel = TelemovelTextBox.Text,
                        Ativo = AtivoCheckBox.IsChecked
                    };

                    if (TipoComboBox.SelectedIndex == 0) // Membro
                    {
                        if (DataNascimentoDatePicker.SelectedDate.HasValue)
                            updateDto.DataNascimento = DataNascimentoDatePicker.SelectedDate.Value;
                        if (int.TryParse(IdSubscricaoTextBox.Text, out int idSub))
                            updateDto.IdSubscricao = idSub;
                    }
                    else // Funcionario
                    {
                        if (FuncaoComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                            updateDto.Funcao = selectedItem.Content.ToString();
                    }

                    var success = await _apiService.UpdateUserAsync(_existingUser.IdUser, updateDto);
                    if (success)
                    {
                        MessageBox.Show("Utilizador atualizado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Erro ao atualizar utilizador.", 
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Criar
                    var registerDto = new UserRegisterDto
                    {
                        Email = EmailTextBox.Text,
                        Tipo = TipoComboBox.SelectedIndex == 0 ? "Membro" : "Funcionario",
                        Nome = NomeTextBox.Text,
                        Telemovel = TelemovelTextBox.Text
                    };

                    if (TipoComboBox.SelectedIndex == 0) // Membro
                    {
                        if (DataNascimentoDatePicker.SelectedDate.HasValue)
                            registerDto.DataNascimento = DataNascimentoDatePicker.SelectedDate.Value;
                        if (int.TryParse(IdSubscricaoTextBox.Text, out int idSub))
                            registerDto.IdSubscricao = idSub;
                    }
                    else // Funcionario
                    {
                        if (FuncaoComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
                            registerDto.Funcao = selectedItem.Content.ToString();
                    }

                    var registerResult = await _apiService.RegisterUserAsync(registerDto);
                    if (registerResult.Success)
                    {
                        MessageBox.Show("Utilizador criado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        var errorMessage = registerResult.ErrorMessage ?? "Erro ao criar utilizador.";
                        MessageBox.Show(errorMessage, 
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", 
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

