// frmAddEditClient.cs
using POS_BLL;
using POS_WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Client
{
    public partial class frmAddEditClient : Window
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;

        private bool _isLoadingForm = false;

        private int _ClientID;
        private clsClient _Client;

        // ============================
        // CONSTRUCTORS
        // ============================
        public frmAddEditClient()
        {
            InitializeComponent();
            _FormMode = enMode.AddNew;
        }

        public frmAddEditClient(int clientID)
        {
            InitializeComponent();
            _ClientID = clientID;
            _FormMode = enMode.Update;
        }

        // ============================
        // WINDOW EVENTS
        // ============================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoadingForm = true;

            _ResetDefaultValues();

            if (_FormMode == enMode.Update)
                _LoadData();

            _isLoadingForm = false;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        // ============================
        // LOAD & RESET
        // ============================
        private void _ResetDefaultValues()
        {
            if (_FormMode == enMode.AddNew)
            {
                _Client = new clsClient();
                txtbTitle.Text = "Add New Client";
                FirstName.Text = string.Empty;
                LastName.Text = string.Empty;
                Phone.Text = string.Empty;
                Email.Text = string.Empty;
            }
            else
            {
                txtbTitle.Text = "Edit Client";
            }
        }

        private void _LoadData()
        {
            _Client = clsClient.FindByID(_ClientID);

            if (_Client == null)
            {
                MessageBox.Show("Client record not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            FirstName.Text = _Client.FirstName;
            LastName.Text = _Client.LastName;
            Phone.Text = _Client.Phone;
            Email.Text = _Client.Email;
        }

        // ============================
        // LIVE VALIDATION
        // ============================
        private void FirstNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingForm) return;
            (sender as ModernInput)?.Validate(live: true);
        }

        private void LastNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingForm) return;
            (sender as ModernInput)?.Validate(live: true);
        }

        private void PhoneInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ValidateUniqueField(
                sender as ModernInput,
                "This phone number is already registered.",
                text => clsClient.IsPhoneExist(text),
                text => clsClient.IsPhoneExist(text, _ClientID));
        }

        private void EmailInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ValidateUniqueField(
                sender as ModernInput,
                "This email address is already registered.",
                text => clsClient.IsEmailExist(text),
                text => clsClient.IsEmailExist(text, _ClientID));
        }

        /// <summary>
        /// Runs live duplicate validation on a unique field (phone or email).
        /// </summary>
        private void _ValidateUniqueField(ModernInput control, string errorMessage,
            Func<string, bool> existsFunc, Func<string, bool> existsExceptIdFunc)
        {
            if (_isLoadingForm || control == null) return;
            if (string.IsNullOrWhiteSpace(control.Text)) return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();
                bool exists = _FormMode == enMode.Update
                    ? existsExceptIdFunc(text)
                    : existsFunc(text);

                return exists ? errorMessage : null;
            });
        }

        // ============================
        // SAVE
        // ============================
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            ValidationResult validation = _ValidateAllFields();

            if (!validation.IsValid)
            {
                ShowErrorMessage(validation.Errors);
                ScrollToFirstError(validation.FirstInvalidControl);
                return;
            }

            _ProcessFormData();
        }

        private void _ProcessFormData()
        {
            _Client.FirstName = FirstName.Text.Trim();
            _Client.LastName = LastName.Text.Trim();
            _Client.Phone = Phone.Text.Trim();
            _Client.Email = Email.Text.Trim();

            try
            {
                bool saved = _Client.Save();

                if (!saved)
                {
                    // Save() returned false — validation blocked it
                    ShowErrorMessage(new List<string>
                    {
                        "• Failed to save. The phone or email may already be registered."
                    });
                    return;
                }

                IsSaved = true;
                ShowSuccessMessage();
            }
            catch (Exception)
            {
                // DAL already logged this — just show a friendly message
                MessageBox.Show(
                    "An unexpected error occurred while saving. Please contact support.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================
        // VALIDATION
        // ============================
        private ValidationResult _ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // ── First Name ──────────────────────────────────────────────────────────
            FirstName.ValidateForce();
            if (!FirstName.IsValid)
            {
                errors.Add($"• {FirstName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = FirstName;
            }

            // ── Last Name ───────────────────────────────────────────────────────────
            LastName.ValidateForce();
            if (!LastName.IsValid)
            {
                errors.Add($"• {LastName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = LastName;
            }

            // ── Phone (format + duplicate) ──────────────────────────────────────────
            Phone.ValidateForce();
            if (!Phone.IsValid)
            {
                errors.Add($"• {Phone.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = Phone;
            }
            else
            {
                Phone.Validate(live: false, externalValidator: text =>
                {
                    bool exists = _FormMode == enMode.Update
                        ? clsClient.IsPhoneExist(text.Trim(), _ClientID)
                        : clsClient.IsPhoneExist(text.Trim());
                    return exists ? "This phone number is already registered." : null;
                });

                if (!Phone.IsValid)
                {
                    errors.Add($"• {Phone.ValidationMessageText}");
                    if (result.FirstInvalidControl == null)
                        result.FirstInvalidControl = Phone;
                }
            }

            // ── Email (format + duplicate) ──────────────────────────────────────────
            Email.ValidateForce();
            if (!Email.IsValid)
            {
                errors.Add($"• {Email.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = Email;
            }
            else
            {
                Email.Validate(live: false, externalValidator: text =>
                {
                    bool exists = _FormMode == enMode.Update
                        ? clsClient.IsEmailExist(text.Trim(), _ClientID)
                        : clsClient.IsEmailExist(text.Trim());
                    return exists ? "This email address is already registered." : null;
                });

                if (!Email.IsValid)
                {
                    errors.Add($"• {Email.ValidationMessageText}");
                    if (result.FirstInvalidControl == null)
                        result.FirstInvalidControl = Email;
                }
            }

            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        // ============================
        // UI HELPERS
        // ============================
        private void ShowErrorMessage(List<string> errors)
        {
            ErrorMessageText.Text = string.Join("\n", errors);
            ErrorMessageBox.Visibility = Visibility.Visible;

            ErrorMessageBox.BeginAnimation(OpacityProperty,
                new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(300) });

            FindScrollViewer(this)?.ScrollToTop();
        }

        private void ShowSuccessMessage()
        {
            SuccessMessageBox.Visibility = Visibility.Visible;

            SuccessMessageBox.BeginAnimation(OpacityProperty,
                new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(300) });

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            timer.Tick += (s, e) => { HideMessages(); timer.Stop(); };
            timer.Start();

            FindScrollViewer(this)?.ScrollToTop();
        }

        private void HideMessages()
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Collapsed;
        }

        private void ScrollToFirstError(FrameworkElement control) =>
            control?.BringIntoView();

        private ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer sv) return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var result = FindScrollViewer(VisualTreeHelper.GetChild(obj, i));
                if (result != null) return result;
            }

            return null;
        }

        // ============================
        // HELPER CLASS
        // ============================
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }
    }
}