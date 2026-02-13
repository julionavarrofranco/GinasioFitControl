using System;
using System.Linq;
using System.Windows;
using FitControlAdmin.Models;
using FitControlAdmin.Services;

namespace FitControlAdmin
{
    public partial class ViewTrainingPlanWindow : Window
    {
        private readonly ApiService _apiService;

        public ViewTrainingPlanWindow(ApiService apiService, int idPlano)
        {
            InitializeComponent();
            _apiService = apiService;
            LoadPlan(idPlano);
        }

        private async void LoadPlan(int idPlano)
        {
            try
            {
                var detail = await _apiService.GetTrainingPlanDetailAsync(idPlano);
                if (detail != null)
                {
                    NomeTextBlock.Text = detail.Nome;
                    DataCriacaoTextBlock.Text = detail.DataCriacao.ToString("dd/MM/yyyy");
                    EstadoTextBlock.Text = detail.Ativo ? "Ativo" : "Inativo";
                    EstadoTextBlock.Foreground = detail.Ativo
                        ? (System.Windows.Media.Brush)new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x27, 0xae, 0x60))
                        : (System.Windows.Media.Brush)new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xe7, 0x4c, 0x3c));
                    ObservacoesTextBlock.Text = string.IsNullOrEmpty(detail.Observacoes) ? "-" : detail.Observacoes;
                    if (detail.Exercicios != null && detail.Exercicios.Count > 0)
                    {
                        ExercisesDataGrid.ItemsSource = detail.Exercicios.OrderBy(e => e.Ordem).ToList();
                    }
                    else
                    {
                        ExercisesDataGrid.ItemsSource = Array.Empty<TrainingPlanExerciseDto>();
                    }
                }
                else
                {
                    var summaryList = await _apiService.GetTrainingPlansByStateAsync(true);
                    var plan = summaryList?.FirstOrDefault(p => p.IdPlano == idPlano);
                    if (plan == null)
                    {
                        var inact = await _apiService.GetTrainingPlansByStateAsync(false);
                        plan = inact?.FirstOrDefault(p => p.IdPlano == idPlano);
                    }
                    if (plan != null)
                    {
                        NomeTextBlock.Text = plan.Nome;
                        DataCriacaoTextBlock.Text = plan.DataCriacao.ToString("dd/MM/yyyy");
                        EstadoTextBlock.Text = plan.Ativo ? "Ativo" : "Inativo";
                        ObservacoesTextBlock.Text = "-";
                        ExercisesDataGrid.ItemsSource = Array.Empty<TrainingPlanExerciseDto>();
                    }
                    else
                    {
                        MessageBox.Show("Plano não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar plano: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExercisesDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
