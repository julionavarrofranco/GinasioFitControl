using System;
using System.Collections.Generic;
using System.Windows;
using FitControlAdmin.Models;

namespace FitControlAdmin
{
    public partial class CreateScheduledClassDialog : Window
    {
        public int SelectedClassId { get; private set; }
        public DateTime SelectedDate { get; private set; }

        public CreateScheduledClassDialog(List<AulaResponseDto> classes)
        {
            InitializeComponent();
            
            ClassComboBox.ItemsSource = classes;
            
            // Se houver apenas 1 aula, pré-selecionar e desabilitar ComboBox
            if (classes != null && classes.Count == 1)
            {
                ClassComboBox.SelectedIndex = 0;
                ClassComboBox.IsEnabled = false;
                InfoTextBlock.Visibility = Visibility.Visible;
                Title = $"Agendar: {classes[0].Nome}";
            }
            else if (classes != null && classes.Count > 1)
            {
                ClassComboBox.IsEnabled = true;
                InfoTextBlock.Visibility = Visibility.Collapsed;
            }
            
            // Definir data mínima e máxima (permite datas passadas para testar "Aulas terminadas")
            DatePicker.DisplayDateStart = DateTime.Today.AddDays(-30);
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

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Por favor, selecione uma data.", "Aviso", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedClassId = (int)ClassComboBox.SelectedValue;
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
}
