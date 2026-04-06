using Microsoft.Extensions.Logging;
using POS_BLL;
using POS_WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace POS_WPF.Client
{
    public partial class frmAddEditClient : Window
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;

        private readonly ILogger _logger = AppLogger.CreateLogger<frmAddEditClient>();

        private bool _isLoadingForm = false;

        int _ClientID;
        clsClient _Client;

        // ── Constructors ────────────────────────────────────────────────────────────

        public frmAddEditClient()
        {
            InitializeComponent();
            FormMode = enMode.AddNew;
        }

        public frmAddEditClient(int clientID)
        {
            InitializeComponent();
            _ClientID = clientID;
            FormMode = enMode.Update;
        }

        // ── Window events ───────────────────────────────────────────────────────────

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ResetDefaultValues();

            _isLoadingForm = true;

            if (FormMode == enMode.Update)
                _LoadData();

            _isLoadingForm = false;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();

        // ── Live validation handlers ────────────────────────────────────────────────

        private void FirstNameInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Names don't need a duplicate check — just let the control self-validate.
            if (_isLoadingForm) return;
            (sender as ModernInput)?.Validate(live: true);
        }

        private void LastNameInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isLoadingForm) return;
            (sender as ModernInput)?.Validate(live: true);
        }

        private void PhoneInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateUniqueField(sender as ModernInput,
                "This phone number is already registered.",
                text => clsClient.IsPhoneExist(text),
                text => clsClient.IsPhoneExist(text, _ClientID));
        }

        private void EmailInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateUniqueField(sender as ModernInput,
                "This email address is already registered.",
                text => clsClient.IsEmailExist(text),
                text => clsClient.IsEmailExist(text, _ClientID));
        }

        // ── Shared live-validation helper ───────────────────────────────────────────

        private void ValidateUniqueField(ModernInput control, string errorMessage,
            Func<string, bool> existsFunc, Func<string, bool> existsExceptIdFunc)
        {
            if (_isLoadingForm || control == null) return;
            if (string.IsNullOrWhiteSpace(control.Text)) return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();
                bool exists = FormMode == enMode.Update
                    ? existsExceptIdFunc(text)
                    : existsFunc(text);

                return exists ? errorMessage : null;
            });
        }

        // ── Data loading / resetting ────────────────────────────────────────────────

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

        private void _ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Client = new clsClient();
                txtbTitle.Text = "Add New Client";

                FirstName.Text = "";
                LastName.Text = "";
                Phone.Text = "";
                Email.Text = "";
            }
            else
            {
                txtbTitle.Text = "Edit Client";
            }
        }

        // ── Save ────────────────────────────────────────────────────────────────────

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            ValidationResult validationResult = ValidateAllFields();

            if (!validationResult.IsValid)
            {
                ShowErrorMessage(validationResult.Errors);
                ScrollToFirstError(validationResult.FirstInvalidControl);
                return;
            }

            bool saved = ProcessFormData();

            if (saved)
                ShowSuccessMessage();
        }

        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // ── First Name ──────────────────────────────────────────────────────────
            FirstName.ValidateForce();
            if (!FirstName.IsValid)
            {
                errors.Add($"• {FirstName.ValidationMessageText}");
                if (result.FirstInvalidControl == null) result.FirstInvalidControl = FirstName;
            }

            // ── Last Name ───────────────────────────────────────────────────────────
            LastName.ValidateForce();
            if (!LastName.IsValid)
            {
                errors.Add($"• {LastName.ValidationMessageText}");
                if (result.FirstInvalidControl == null) result.FirstInvalidControl = LastName;
            }

            // ── Phone (format + duplicate) ──────────────────────────────────────────
            Phone.ValidateForce();
            if (!Phone.IsValid)
            {
                errors.Add($"• {Phone.ValidationMessageText}");
                if (result.FirstInvalidControl == null) result.FirstInvalidControl = Phone;
            }
            else
            {
                Phone.Validate(live: false, externalValidator: text =>
                {
                    text = text.Trim();
                    bool exists = FormMode == enMode.Update
                        ? clsClient.IsPhoneExist(text, _ClientID)
                        : clsClient.IsPhoneExist(text);
                    return exists ? "This phone number is already registered." : null;
                });

                if (!Phone.IsValid)
                {
                    errors.Add($"• {Phone.ValidationMessageText}");
                    if (result.FirstInvalidControl == null) result.FirstInvalidControl = Phone;
                }
            }

            // ── Email (format + duplicate) ──────────────────────────────────────────
            Email.ValidateForce();
            if (!Email.IsValid)
            {
                errors.Add($"• {Email.ValidationMessageText}");
                if (result.FirstInvalidControl == null) result.FirstInvalidControl = Email;
            }
            else
            {
                Email.Validate(live: false, externalValidator: text =>
                {
                    text = text.Trim();
                    bool exists = FormMode == enMode.Update
                        ? clsClient.IsEmailExist(text, _ClientID)
                        : clsClient.IsEmailExist(text);
                    return exists ? "This email address is already registered." : null;
                });

                if (!Email.IsValid)
                {
                    errors.Add($"• {Email.ValidationMessageText}");
                    if (result.FirstInvalidControl == null) result.FirstInvalidControl = Email;
                }
            }

            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        private bool ProcessFormData()
        {
            _Client.FirstName = FirstName.Text.Trim();
            _Client.LastName = LastName.Text.Trim();
            _Client.Phone = Phone.Text.Trim();
            _Client.Email = Email.Text.Trim();

            try
            {
                if (!_Client.Save())
                {
                    _logger.LogWarning("Failed to save client: {ClientName}",
                        $"{_Client.FirstName} {_Client.LastName}");
                    MessageBox.Show("Failed to save client.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string successMsg = FormMode == enMode.AddNew
                    ? "Client saved successfully."
                    : "Client updated successfully.";

                MessageBox.Show(successMsg, "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (FormMode == enMode.AddNew)
                    IsSaved = true;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving client: {ClientName}",
                    $"{_Client.FirstName} {_Client.LastName}");
                MessageBox.Show(
                    "An unexpected error occurred while saving the client. Please contact support.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ── UI feedback ─────────────────────────────────────────────────────────────

        private void ShowErrorMessage(List<string> errors)
        {
            ErrorMessageText.Text = string.Join("\n", errors);
            ErrorMessageBox.Visibility = Visibility.Visible;
            ErrorMessageBox.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)));
            FindScrollViewer(this)?.ScrollToTop();
        }

        private void ShowSuccessMessage()
        {
            SuccessMessageBox.Visibility = Visibility.Visible;
            SuccessMessageBox.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)));

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
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var result = FindScrollViewer(
                    System.Windows.Media.VisualTreeHelper.GetChild(obj, i));
                if (result != null) return result;
            }
            return null;
        }

        // ── Helper ──────────────────────────────────────────────────────────────────

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }
    }
}