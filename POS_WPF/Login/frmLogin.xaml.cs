using POS_BLL;
using POS_WPF.Global;
using POS_WPF.Utils;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string username = string.Empty;
            string hashedPassword = string.Empty;

            if (clsGlobal.GetStoredCredential(ref username, ref hashedPassword))
            {
                clsUser user = clsUser.FindByUsernameAndPassword(username, hashedPassword);

                if (user != null)
                {
                    clsGlobal.CurrentUser = user;

                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                    return;
                }

                // failed login → clear saved
                clsGlobal.RememberUsernameAndPassword("", "");
            }

            TxtUsername.Text = username;
            TxtPassword.Password = "";
            TxtUsername.Focus();
        }

        // ── Close button ─────────────────────────────────────────────────────
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ── Login button ─────────────────────────────────────────────────────
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text) ||
                string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            string username = TxtUsername.Text.Trim();
            string hashedPassword = clsUtil.HashPassword(TxtPassword.Password);

            clsUser user = clsUser.FindByUsernameAndPassword(username, hashedPassword);

            if (user != null)
            {
                // (optional later) if you add IsActive
                // if (!user.IsActive)
                // {
                //     ShowError("Your account is not active.");
                //     return;
                // }

                // Remember Me
                if (ChkRememberMe.IsChecked == true)
                {
                    clsGlobal.RememberUsernameAndPassword(username, hashedPassword);
                }
                else
                {
                    clsGlobal.RememberUsernameAndPassword("", "");
                }

                clsGlobal.CurrentUser = user;

                HideError();

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ShowError("Invalid username or password.");
                TxtUsername.Focus();
            }
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