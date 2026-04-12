using POS_BLL;
using POS_WPF.Global;
using POS_WPF.Utils;
using System.Windows;
using System.Windows.Input;

namespace POS_WPF.Dialogs
{
    public partial class frmChangePassword : Window
    {
        private readonly clsUser _user;

        public frmChangePassword(clsUser user)
        {
            InitializeComponent();
            _user = user;

            // Allow dragging the borderless dialog
            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    DragMove();
            };
        }

        // ── Confirm ──────────────────────────────────────────────────────────
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            // ── 1. Validate not empty ────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(TxtOldPassword.Password))
            {
                ShowError("Please enter your current password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNewPassword.Password))
            {
                ShowError("Please enter a new password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtConfirmPassword.Password))
            {
                ShowError("Please confirm your new password.");
                return;
            }

            // ── 2. New passwords match ───────────────────────────────────────
            if (TxtNewPassword.Password != TxtConfirmPassword.Password)
            {
                ShowError("New password and confirmation do not match.");
                return;
            }

            // ── 3. New password is different from old ────────────────────────
            if (TxtOldPassword.Password == TxtNewPassword.Password)
            {
                ShowError("New password must be different from the current password.");
                return;
            }

            // ── 4. Minimum length ────────────────────────────────────────────
            if (TxtNewPassword.Password.Length < 5)
            {
                ShowError("New password must be at least 6 characters long.");
                return;
            }

            // ── 5. Hash then call BLL ────────────────────────────────────────
            string oldHash = clsUtil.HashPassword(TxtOldPassword.Password);
            string newHash = clsUtil.HashPassword(TxtNewPassword.Password);

            bool success = _user.ChangePassword(oldHash, newHash);

            if (!success)
            {
                ShowError("Current password is incorrect. Please try again.");
                return;
            }

            // ── 6. Sync global user ──────────────────────────────────────────
            clsGlobal.CurrentUser = _user;

            // ── 7. Success ───────────────────────────────────────────────────
            MessageBox.Show(
                "Your password has been changed successfully.",
                "Password Updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        // ── Cancel ───────────────────────────────────────────────────────────
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ── Helpers ──────────────────────────────────────────────────────────
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