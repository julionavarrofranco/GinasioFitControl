using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FitControlAdmin
{
    public partial class EditPaymentWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly PaymentResponseDto _payment;
        private readonly System.Collections.Generic.List<SubscriptionResponseDto>? _subscriptions;

        public EditPaymentWindow(ApiService apiService, PaymentResponseDto payment, System.Collections.Generic.List<SubscriptionResponseDto>? subscriptions)
        {
            InitializeComponent();
            _apiService = apiService;
            _payment = payment;
            _subscriptions = subscriptions;

            LoadData();
        }

        private void LoadData()
        {
            // Load subscriptions
            if (_subscriptions != null)
            {
                SubscricaoComboBox.ItemsSource = _subscriptions;
                var selectedSub = _subscriptions.FirstOrDefault(s => FormatEnumName(s.Tipo.ToString()) == _payment.Subscricao);
                if (selectedSub != null)
                    SubscricaoComboBox.SelectedItem = selectedSub;
            }

            // Load payment methods
            MetodoPagamentoComboBox.Items.Clear();
            foreach (MetodoPagamento metodo in Enum.GetValues(typeof(MetodoPagamento)))
            {
                var displayName = FormatEnumName(metodo.ToString());
                var item = new ComboBoxItem { Content = displayName, Tag = metodo };
                MetodoPagamentoComboBox.Items.Add(item);
                
                if (metodo.ToString() == _payment.MetodoPagamento)
                    MetodoPagamentoComboBox.SelectedItem = item;
            }

            // Load payment states
            EstadoPagamentoComboBox.Items.Clear();
            foreach (EstadoPagamento estado in Enum.GetValues(typeof(EstadoPagamento)))
            {
                var displayName = FormatEnumName(estado.ToString());
                var item = new ComboBoxItem { Content = displayName, Tag = estado };
                EstadoPagamentoComboBox.Items.Add(item);
                
                if (estado.ToString() == _payment.EstadoPagamento)
                    EstadoPagamentoComboBox.SelectedItem = item;
            }
        }

        private static string FormatEnumName(string enumName)
        {
            if (enumName == "MBWay") return "MBWay";
            if (enumName == "Cartao") return "Cartão";
            if (enumName == "Bracos") return "Braços";
            
            return System.Text.RegularExpressions.Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubscricaoComboBox.SelectedItem == null ||
                MetodoPagamentoComboBox.SelectedItem == null ||
                EstadoPagamentoComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, preencha todos os campos.",
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var updateDto = new UpdatePaymentDto();
                bool hasChanges = false;

                // Update payment method if changed
                if (MetodoPagamentoComboBox.SelectedItem is ComboBoxItem selectedMetodo &&
                    selectedMetodo.Tag is MetodoPagamento metodoPagamento &&
                    metodoPagamento.ToString() != _payment.MetodoPagamento)
                {
                    updateDto.MetodoPagamento = metodoPagamento;
                    hasChanges = true;
                    System.Diagnostics.Debug.WriteLine($"EditPaymentWindow: MetodoPagamento changed from {_payment.MetodoPagamento} to {metodoPagamento}");
                }

                // Update payment state if changed
                if (EstadoPagamentoComboBox.SelectedItem is ComboBoxItem selectedEstado &&
                    selectedEstado.Tag is EstadoPagamento estadoPagamento &&
                    estadoPagamento.ToString() != _payment.EstadoPagamento)
                {
                    updateDto.EstadoPagamento = estadoPagamento;
                    hasChanges = true;
                    System.Diagnostics.Debug.WriteLine($"EditPaymentWindow: EstadoPagamento changed from {_payment.EstadoPagamento} to {estadoPagamento}");
                }

                // Update subscription if changed
                if (SubscricaoComboBox.SelectedItem is SubscriptionResponseDto selectedSub &&
                    FormatEnumName(selectedSub.Tipo.ToString()) != _payment.Subscricao)
                {
                    updateDto.IdSubscricao = selectedSub.IdSubscricao;
                    hasChanges = true;
                    System.Diagnostics.Debug.WriteLine($"EditPaymentWindow: Subscricao changed from '{_payment.Subscricao}' to '{FormatEnumName(selectedSub.Tipo.ToString())}' (IdSubscricao: {selectedSub.IdSubscricao})");
                }

                if (!hasChanges)
                {
                    MessageBox.Show("Nenhuma alteração foi realizada.",
                        "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var (success, errorMessage) = await _apiService.UpdatePaymentAsync(_payment.IdPagamento, updateDto);
                if (success)
                {
                    MessageBox.Show("Pagamento atualizado com sucesso!",
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(errorMessage ?? "Erro ao atualizar pagamento.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
