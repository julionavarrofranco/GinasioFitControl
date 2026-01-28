using System;
using System.Windows;
using FitControlAdmin.Models;

namespace FitControlAdmin
{
    public partial class AddExerciseToPlanDialog : Window
    {
        public ExercisePlanDto? Result { get; private set; }
        public TrainingPlanExerciseDto? DisplayInfo { get; private set; }
        private readonly TrainingPlanExerciseDto? _existing;

        /// <summary>Modo adicionar.</summary>
        public AddExerciseToPlanDialog(System.Collections.Generic.List<ExerciseResponseDto> exercises)
            : this(exercises, null) { }

        /// <summary>Modo editar: passa o exercício existente para preencher séries/reps/carga/ordem.</summary>
        public AddExerciseToPlanDialog(System.Collections.Generic.List<ExerciseResponseDto> exercises, TrainingPlanExerciseDto? existing)
        {
            InitializeComponent();
            _existing = existing;
            ExerciseComboBox.ItemsSource = exercises;
            if (existing != null)
            {
                Title = "Editar exercício no plano";
                AddButton.Content = "Guardar";
                ExerciseComboBox.SelectedValue = existing.IdExercicio;
                SeriesTextBox.Text = existing.Series.ToString();
                RepsTextBox.Text = existing.Repeticoes.ToString();
                CargaTextBox.Text = existing.Carga.ToString();
                OrdemTextBox.Text = existing.Ordem > 0 ? existing.Ordem.ToString() : "";
                ExerciseComboBox.IsEnabled = false;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExerciseComboBox.SelectedValue == null)
            {
                MessageBox.Show("Selecione um exercício.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SeriesTextBox.Text, out int series) || series < 1)
            {
                MessageBox.Show("Séries deve ser um número positivo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(RepsTextBox.Text, out int reps) || reps < 1)
            {
                MessageBox.Show("Repetições deve ser um número positivo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(CargaTextBox.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out decimal carga))
                carga = 0;

            int? ordem = null;
            if (!string.IsNullOrWhiteSpace(OrdemTextBox.Text) && int.TryParse(OrdemTextBox.Text, out int o))
                ordem = o;

            var idExercicio = _existing?.IdExercicio ?? (int)ExerciseComboBox.SelectedValue;
            var selected = ExerciseComboBox.SelectedItem as ExerciseResponseDto;
            var nomeExercicio = selected?.Nome ?? _existing?.NomeExercicio ?? "";
            var grupo = selected?.GrupoMuscular ?? _existing?.GrupoMuscular ?? GrupoMuscular.CorpoInteiro;

            Result = new ExercisePlanDto
            {
                IdExercicio = idExercicio,
                Series = series,
                Repeticoes = reps,
                Carga = carga,
                Ordem = ordem
            };

            DisplayInfo = new TrainingPlanExerciseDto
            {
                IdExercicio = idExercicio,
                NomeExercicio = nomeExercicio,
                GrupoMuscular = grupo,
                Series = series,
                Repeticoes = reps,
                Carga = carga,
                Ordem = ordem ?? 0
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
