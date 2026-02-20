using System;
using System.Collections.Generic;
using System.Windows;
using FitControlAdmin.Models;

namespace FitControlAdmin.Views
{
    public partial class CreateScheduledClassDialog : Window
    {
        public int SelectedClassId { get; private set; }
        public int SelectedSala { get; private set; } = 1;
        public DateTime SelectedDate { get; private set; }

        private static readonly List<SalaItem> Salas = new List<SalaItem>
        {
            new SalaItem("Sala 1", 1),
            new SalaItem("Sala 2", 2),
            new SalaItem("Sala 3", 3),
            new SalaItem("Sala 4", 4),
            new SalaItem("Sala 5", 5)
        };

        public CreateScheduledClassDialog(List<AulaResponseDto> classes)
        {
            InitializeComponent();
            
            ClassComboBox.ItemsSource = classes;
            
            SalaComboBox.ItemsSource = Salas;
            SalaComboBox.SelectedValue = 1;
            
            // Com uma só aula, pré-selecionar e desativar ComboBox
            if (classes != null && classes.Count == 1)
            {
                ClassComboBox.SelectedIndex = 0;
                ClassComboBox.IsEnabled = false;
                Title = $"Agendar: {classes[0].Nome}";
            }
            else if (classes != null && classes.Count > 1)
            {
                ClassComboBox.IsEnabled = true;
            }
            
            // Antecedência mínima 1 dia; máximo 2 semanas
            DatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
            DatePicker.DisplayDateEnd = DateTime.Today.AddDays(14);
            DatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClassComboBox.SelectedValue == null)
            {
                MessageBox.Show("Por favor, selecione uma aula.", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SalaComboBox.SelectedValue is not int sala || sala < 1 || sala > 5)
            {
                MessageBox.Show("Por favor, selecione uma sala de aula (1 a 5).", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Por favor, selecione uma data.", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedClassId = (int)ClassComboBox.SelectedValue;
            SelectedSala = sala;
            SelectedDate = DatePicker.SelectedDate.Value;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    internal class SalaItem
    {
        public string Display { get; }
        public int Value { get; }
        public SalaItem(string display, int value) { Display = display; Value = value; }
    }
}
