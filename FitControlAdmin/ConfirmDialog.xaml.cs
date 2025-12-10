using System.Windows;

namespace FitControlAdmin
{
    public partial class ConfirmDialog : Window
    {
        public bool Result { get; private set; } = false;

        public ConfirmDialog(string message = "Tem certeza que deseja fazer logout?", string title = "Confirmar Logout")
        {
            InitializeComponent();
            MessageText.Text = message;
            this.Title = title;
        }

        private void SimButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            DialogResult = true;
            Close();
        }

        private void NaoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogResult = false;
            Close();
        }
    }
}

