using System.Windows;
using System.Windows.Input;

namespace POS_WPF.Login
{
    public partial class frmLogin : Window
    {
        public frmLogin()
        {
            InitializeComponent();

            // Allow dragging the borderless window
            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    DragMove();
            };
        }

        // ── Close button ─────────────────────────────────────────────────────
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ── Login button ─────────────────────────────────────────────────────
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text) || string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            if (TxtUsername.Text != "admin" || TxtPassword.Password != "password")
            {
                ShowError("Invalid username or password.");
                return;
            }



            HideError();

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();


        }

        // ── Remember Me ──────────────────────────────────────────────────────
        private void ChkRememberMe_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ChkRememberMe_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        // ── Helper — show / hide error banner ────────────────────────────────
        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBanner.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorBanner.Visibility = Visibility.Collapsed;
        }
    }
}