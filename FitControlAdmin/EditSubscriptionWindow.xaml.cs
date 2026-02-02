using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FitControlAdmin
{
    public partial class EditSubscriptionWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly SubscriptionResponseDto? _subscription;
        private readonly bool _isCreateMode;

        public EditSubscriptionWindow(ApiService apiService, SubscriptionResponseDto? subscription)
        {
            InitializeComponent();
            _apiService = apiService;
            _subscription = subscription;
            _isCreateMode = subscription == null;

            if (_isCreateMode)
            {
                Title = "Criar Subscrição";
                TitleTextBlock.Text = "Criar Subscrição";
                SaveButton.Content = "Criar";
            }
            else
            {
                Title = "Editar Subscrição";
                TitleTextBlock.Text = "Editar Subscrição";
            }

            LoadSubscriptionData();
            PopulateTipoComboBox();

            if (_isCreateMode)
            {
                AtivoCheckBox.IsChecked = true;
                AtivoCheckBox.IsEnabled = false;
            }
            else
                AtivoCheckBox.IsEnabled = true;
        }

        private void LoadSubscriptionData()
        {
            if (_subscription != null)
            {
                NomeTextBox.Text = _subscription.Nome;
                PrecoTextBox.Text = _subscription.Preco.ToString("F2");
                AtivoCheckBox.IsChecked = _subscription.Ativo;
            }
            else
            {
                // Default values for new subscription
                AtivoCheckBox.IsChecked = true;
            }
        }

        private void PopulateTipoComboBox()
        {
            TipoComboBox.Items.Clear();
            foreach (TipoSubscricao tipo in Enum.GetValues(typeof(TipoSubscricao)))
            {
                var displayName = FormatEnumName(tipo.ToString());
                var comboBoxItem = new ComboBoxItem
                {
                    Content = displayName,
                    Tag = tipo
                };
                TipoComboBox.Items.Add(comboBoxItem);

                // Select current tipo only if editing existing subscription
                if (_subscription != null && tipo == _subscription.Tipo)
                {
                    TipoComboBox.SelectedItem = comboBoxItem;
                }
            }
        }

        private static string FormatEnumName(string enumName)
        {
            // Casos especiais
            if (enumName == "Mensal") return "Mensal";
            if (enumName == "Trimestral") return "Trimestral";
            if (enumName == "Anual") return "Anual";

            // Adiciona espaço antes de letras maiúsculas (exceto a primeira)
            return System.Text.RegularExpressions.Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomeTextBox.Text))
            {
                MessageBox.Show("Por favor, introduza um nome para a subscrição.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TipoComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione um tipo para a subscrição.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PrecoTextBox.Text, out decimal preco) || preco < 0)
            {
                MessageBox.Show("Por favor, introduza um preço válido (maior ou igual a 0).", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedTipoItem = TipoComboBox.SelectedItem as ComboBoxItem;
                var tipo = (TipoSubscricao)selectedTipoItem.Tag;

                if (_isCreateMode)
                {
                    // Create new subscription
                    var createDto = new CreateSubscriptionDto
                    {
                        Nome = NomeTextBox.Text,
                        Tipo = tipo,
                        Preco = preco
                    };

                    var (success, errorMessage, subscription) = await _apiService.CreateSubscriptionAsync(createDto);

                        if (success)
                    {
                        MessageBox.Show("Subscrição criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Erro ao criar subscrição: {errorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Update existing subscription
                    var updateDto = new UpdateSubscriptionDto
                    {
                        Nome = NomeTextBox.Text,
                        Tipo = tipo,
                        Preco = preco
                    };

                    var (success, errorMessage) = await _apiService.UpdateSubscriptionAsync(_subscription.IdSubscricao, updateDto);

                    if (success)
                    {
                        if (AtivoCheckBox.IsChecked != _subscription.Ativo)
                        {
                            await _apiService.ChangeSubscriptionStatusAsync(_subscription.IdSubscricao, AtivoCheckBox.IsChecked ?? true);
                        }
                        MessageBox.Show("Subscrição atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Erro ao atualizar subscrição: {errorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
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
