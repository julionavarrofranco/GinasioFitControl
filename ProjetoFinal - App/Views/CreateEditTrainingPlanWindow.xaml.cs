using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FitControlAdmin.Models;
using FitControlAdmin.Services;

namespace FitControlAdmin.Views
{
    public partial class CreateEditTrainingPlanWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly int _idFuncionario;
        private readonly int? _idPlano;
        private readonly List<TrainingPlanExerciseDto> _exercises = new();
        // Em modo edição: IDs dos exercícios já no plano (para Update em vez de Add)
        private readonly HashSet<int> _loadedExerciseIds = new();

        public CreateEditTrainingPlanWindow(ApiService apiService, int idFuncionario, int? idPlano, string? planNome = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _idFuncionario = idFuncionario;
            _idPlano = idPlano;

            Title = idPlano.HasValue ? "Editar Plano de Treino" : "Criar Plano de Treino";
            NomeTextBox.Text = planNome ?? "";
            ObservacoesTextBox.Text = "";

            ExercisesDataGrid.ItemsSource = _exercises;

            if (idPlano.HasValue)
                Loaded += (s, _) => LoadPlanForEdit(idPlano.Value);
        }

        private async void LoadPlanForEdit(int idPlano)
        {
            try
            {
                var detail = await _apiService.GetTrainingPlanDetailAsync(idPlano);
                if (detail != null)
                {
                    NomeTextBox.Text = detail.Nome ?? "";
                    ObservacoesTextBox.Text = detail.Observacoes ?? "";
                    _exercises.Clear();
                    _loadedExerciseIds.Clear();
                    if (detail.Exercicios != null)
                    {
                        foreach (var ex in detail.Exercicios.OrderBy(e => e.Ordem))
                        {
                            _exercises.Add(ex);
                            _loadedExerciseIds.Add(ex.IdExercicio);
                        }
                    }
                    ExercisesDataGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar plano: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exercises = await _apiService.GetExercisesByStateAsync(ativo: true);
                if (exercises == null || exercises.Count == 0)
                {
                    MessageBox.Show("Não há exercícios ativos disponíveis.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new AddExerciseToPlanDialog(exercises);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true && dlg.DisplayInfo != null)
                {
                    if (dlg.DisplayInfo.Ordem == 0)
                        dlg.DisplayInfo.Ordem = _exercises.Count + 1;
                    _exercises.Add(dlg.DisplayInfo);
                    ExercisesDataGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not TrainingPlanExerciseDto item)
                return;
            try
            {
                var exercises = await _apiService.GetExercisesByStateAsync(ativo: true);
                if (exercises == null || exercises.Count == 0)
                {
                    MessageBox.Show("Não há exercícios ativos disponíveis.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var dlg = new AddExerciseToPlanDialog(exercises, item);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true && dlg.DisplayInfo != null)
                {
                    var idx = _exercises.IndexOf(item);
                    if (idx >= 0)
                    {
                        _exercises[idx] = dlg.DisplayInfo;
                        ExercisesDataGrid.Items.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TrainingPlanExerciseDto item)
            {
                _exercises.Remove(item);
                ExercisesDataGrid.Items.Refresh();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var nome = NomeTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(nome))
            {
                MessageBox.Show("Introduza o nome do plano.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int idPlano;

                if (_idPlano.HasValue)
                {
                    idPlano = _idPlano.Value;
                    var (ok, err) = await _apiService.UpdateTrainingPlanAsync(idPlano, new UpdateTrainingPlanDto
                    {
                        Nome = nome,
                        Observacoes = ObservacoesTextBox.Text?.Trim()
                    });
                    if (!ok)
                    {
                        MessageBox.Show(err ?? "Erro ao atualizar plano.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    var (ok, err, data) = await _apiService.CreateTrainingPlanAsync(_idFuncionario, new TrainingPlanDto
                    {
                        Nome = nome,
                        Observacoes = ObservacoesTextBox.Text?.Trim()
                    });
                    if (!ok || data == null)
                    {
                        MessageBox.Show(err ?? "Erro ao criar plano.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    idPlano = data.IdPlano;
                }

                var addErrors = new List<string>();
                foreach (var ex in _exercises)
                {
                    if (_idPlano.HasValue && _loadedExerciseIds.Contains(ex.IdExercicio))
                    {
                        var updateDto = new UpdateExercisePlanDto
                        {
                            Series = ex.Series,
                            Repeticoes = ex.Repeticoes,
                            Carga = ex.Carga,
                            Ordem = ex.Ordem
                        };
                        var (updOk, updErr) = await _apiService.UpdateExerciseInPlanAsync(idPlano, ex.IdExercicio, updateDto);
                        if (!updOk)
                            addErrors.Add($"'{ex.NomeExercicio}': {updErr ?? "erro ao atualizar"}");
                    }
                    else
                    {
                        var dto = new ExercisePlanDto
                        {
                            IdExercicio = ex.IdExercicio,
                            Series = ex.Series,
                            Repeticoes = ex.Repeticoes,
                            Carga = ex.Carga,
                            Ordem = ex.Ordem
                        };
                        var (addOk, addErr) = await _apiService.AddExerciseToPlanAsync(idPlano, dto);
                        if (!addOk)
                            addErrors.Add($"'{ex.NomeExercicio}': {addErr ?? "erro desconhecido"}");
                    }
                }

                if (addErrors.Count > 0)
                {
                    MessageBox.Show(
                        "O plano foi guardado, mas alguns exercícios não puderam ser adicionados:\n\n" +
                        string.Join("\n", addErrors) +
                        "\n\nPor exemplo, um exercício pode já existir no plano. Remova duplicados ou use \"Editar\" para alterar os valores e guarde novamente.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(_idPlano.HasValue ? "Plano atualizado com sucesso." : "Plano criado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
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
