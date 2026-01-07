using FitControlAdmin.Models;
using FitControlAdmin.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FitControlAdmin
{
    public partial class CreateEditExerciseWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly ExerciseResponseDto? _existingExercise;
        private readonly bool _isEditMode;

        public CreateEditExerciseWindow(ApiService apiService, ExerciseResponseDto? existingExercise = null)
        {
            InitializeComponent();
            _apiService = apiService;
            _existingExercise = existingExercise;
            _isEditMode = existingExercise != null;

            // Popular ComboBox com grupos musculares
            foreach (GrupoMuscular grupo in Enum.GetValues(typeof(GrupoMuscular)))
            {
                GrupoMuscularComboBox.Items.Add(grupo);
            }

            if (_isEditMode)
            {
                TitleText.Text = "Editar Exercício";
                LoadExerciseData();
            }
            else
            {
                GrupoMuscularComboBox.SelectedIndex = 0;
            }

            // Atualizar preview da imagem quando URL mudar
            FotoUrlTextBox.TextChanged += FotoUrlTextBox_TextChanged;
        }

        private void LoadExerciseData()
        {
            if (_existingExercise == null) return;

            NomeTextBox.Text = _existingExercise.Nome;
            DescricaoTextBox.Text = _existingExercise.Descricao;
            FotoUrlTextBox.Text = _existingExercise.FotoUrl;
            AtivoCheckBox.IsChecked = _existingExercise.Ativo;
            
            // Selecionar grupo muscular
            GrupoMuscularComboBox.SelectedItem = _existingExercise.GrupoMuscular;
            
            // Carregar preview da imagem
            UpdateImagePreview(_existingExercise.FotoUrl);
        }

        private void FotoUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateImagePreview(FotoUrlTextBox.Text);
        }

        private void UpdateImagePreview(string url)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ImagePreview.Source = bitmap;
                    PreviewPlaceholderText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ImagePreview.Source = null;
                    PreviewPlaceholderText.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                ImagePreview.Source = null;
                PreviewPlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomeTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescricaoTextBox.Text) ||
                string.IsNullOrWhiteSpace(FotoUrlTextBox.Text) ||
                GrupoMuscularComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", 
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _existingExercise != null)
                {
                    // Atualizar exercício
                    var updateDto = new UpdateExerciseDto
                    {
                        Nome = NomeTextBox.Text,
                        Descricao = DescricaoTextBox.Text,
                        FotoUrl = FotoUrlTextBox.Text,
                        GrupoMuscular = (GrupoMuscular)GrupoMuscularComboBox.SelectedItem
                    };

                    var (success, errorMessage) = await _apiService.UpdateExerciseAsync(_existingExercise.IdExercicio, updateDto);
                    
                    if (success)
                    {
                        // Se o estado mudou, atualizar também
                        if (AtivoCheckBox.IsChecked != _existingExercise.Ativo)
                        {
                            await _apiService.ChangeExerciseStatusAsync(_existingExercise.IdExercicio, AtivoCheckBox.IsChecked ?? true);
                        }

                        MessageBox.Show("Exercício atualizado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(errorMessage ?? "Erro ao atualizar exercício.", 
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Criar exercício
                    var exerciseDto = new ExerciseDto
                    {
                        Nome = NomeTextBox.Text,
                        Descricao = DescricaoTextBox.Text,
                        FotoUrl = FotoUrlTextBox.Text,
                        GrupoMuscular = (GrupoMuscular)GrupoMuscularComboBox.SelectedItem
                    };

                    var (success, errorMessage, exercise) = await _apiService.CreateExerciseAsync(exerciseDto);
                    
                    if (success && exercise != null)
                    {
                        // Se não estiver marcado como ativo, desativar
                        if (AtivoCheckBox.IsChecked == false)
                        {
                            await _apiService.ChangeExerciseStatusAsync(exercise.IdExercicio, false);
                        }

                        MessageBox.Show("Exercício criado com sucesso!", 
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(errorMessage ?? "Erro ao criar exercício.", 
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

