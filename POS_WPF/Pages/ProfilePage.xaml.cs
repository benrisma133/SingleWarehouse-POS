using POS_BLL;
using POS_WPF.Global;
using POS_WPF.Login;
using System.Windows;
using System.Windows.Controls;

namespace POS_WPF.Pages
{
    public partial class ProfilePage : UserControl
    {
        // ── keep a local reference so Save knows what to update ──────────────
        private clsUser _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // ════════════════════════════════════════════════════════════
        //  LOAD — populate all fields from GlobalCurrentUser
        // ════════════════════════════════════════════════════════════
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _currentUser = clsGlobal.CurrentUser;

            if (_currentUser == null)
                return;

            // ── Hero card ────────────────────────────────────────────
            string first = _currentUser.FirstName ?? "";
            string last = _currentUser.LastName ?? "";

            TxtFullName.Text = $"{first} {last}".Trim();
            TxtUsername.Text = _currentUser.Username;
            TxtPersonID.Text = _currentUser.PersonID.ToString();

            // Initials (up to 2 chars)
            string initials = "";
            if (first.Length > 0) initials += first[0];
            if (last.Length > 0) initials += last[0];
            TxtInitials.Text = initials.ToUpper();

            // ── Editable fields ──────────────────────────────────────
            InputFirstName.Text = first;
            InputLastName.Text = last;
            InputPhone.Text = _currentUser.Phone ?? "";
            InputEmail.Text = _currentUser.Email ?? "";
            InputAddress.Text = _currentUser.Address ?? "";
            InputDob.Text = _currentUser.DateOfBirth ?? "";
            InputUsername.Text = _currentUser.Username ?? "";

            // Gender combo
            CmbGender.SelectedIndex = _currentUser.Gender == 1 ? 1 : 0;
        }

        // ════════════════════════════════════════════════════════════
        //  SAVE — update user info
        // ════════════════════════════════════════════════════════════
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // ── 1. Basic validation ──────────────────────────────────
            if (string.IsNullOrWhiteSpace(InputFirstName.Text) ||
                string.IsNullOrWhiteSpace(InputLastName.Text) ||
                string.IsNullOrWhiteSpace(InputUsername.Text))
            {
                MessageBox.Show(
                    "First name, last name and username are required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // ── 2. Check username uniqueness (ignore current user) ───
            string newUsername = InputUsername.Text.Trim();

            if (!newUsername.Equals(_currentUser.Username,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                if (clsUser.IsUsernameExist(newUsername, _currentUser.PersonID))
                {
                    MessageBox.Show(
                        $"The username \"{newUsername}\" is already taken.\nPlease choose a different one.",
                        "Username Taken",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            // ── 3. Apply changes to the BLL object ──────────────────
            _currentUser.FirstName = InputFirstName.Text.Trim();
            _currentUser.LastName = InputLastName.Text.Trim();
            _currentUser.Phone = InputPhone.Text.Trim();
            _currentUser.Email = InputEmail.Text.Trim();
            _currentUser.Address = InputAddress.Text.Trim();
            _currentUser.DateOfBirth = InputDob.Text.Trim();
            _currentUser.Username = newUsername;
            _currentUser.Gender = CmbGender.SelectedIndex; // 0 = Male, 1 = Female

            // ── 4. Persist ───────────────────────────────────────────
            bool saved = _currentUser.Save();

            if (!saved)
            {
                MessageBox.Show(
                    "Something went wrong while saving your profile.\nPlease try again.",
                    "Save Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // ── 5. Sync GlobalCurrentUser ────────────────────────────
            clsGlobal.CurrentUser = _currentUser;

            // ── 6. Refresh hero card ─────────────────────────────────
            string first = _currentUser.FirstName;
            string last = _currentUser.LastName;

            TxtFullName.Text = $"{first} {last}".Trim();
            TxtUsername.Text = _currentUser.Username;

            string initials = "";
            if (first.Length > 0) initials += first[0];
            if (last.Length > 0) initials += last[0];
            TxtInitials.Text = initials.ToUpper();

            // ── 7. Success feedback ──────────────────────────────────
            MessageBox.Show(
                "Your profile has been updated successfully.",
                "Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ════════════════════════════════════════════════════════════
        //  CHANGE PASSWORD
        // ════════════════════════════════════════════════════════════
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Open the change-password dialog
            var dialog = new POS_WPF.Dialogs.frmChangePassword(_currentUser);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        // ════════════════════════════════════════════════════════════
        //  LOGOUT
        // ════════════════════════════════════════════════════════════
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            // Clear remembered credentials
            clsGlobal.RememberUsernameAndPassword("", "");

            // Clear current user
            clsGlobal.CurrentUser = null;

            // Open login and close main window
            frmLogin login = new frmLogin();
            login.Show();

            // Walk up to the parent Window and close it
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}