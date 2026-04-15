using System.Windows;
using System.Windows.Input;

namespace POS_WPF.Dialogs
{
    public partial class frmConfirmDelete : Window
    {
        public bool Confirmed { get; private set; } = false;

        public frmConfirmDelete(string title, string message)
        {
            InitializeComponent();

            TxtTitle.Text = title;
            TxtMessage.Text = $"\"{message}\"";

            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    DragMove();
            };
        }

        private void BtnDeleteAnyway_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}