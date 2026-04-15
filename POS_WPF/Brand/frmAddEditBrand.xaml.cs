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

namespace POS_WPF.Brand
{
    public partial class frmAddEditBrand : Window
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;

        private bool _isLoadingForm = false;

        private int _BrandID;
        private clsBrand _Brand;

        // ============================
        // CONSTRUCTORS
        // ============================
        public frmAddEditBrand()
        {
            InitializeComponent();
            _FormMode = enMode.AddNew;
        }

        public frmAddEditBrand(int brandID)
        {
            InitializeComponent();
            _BrandID = brandID;
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
                _Brand = new clsBrand();
                txtbTitle.Text = "Add New Brand";
                BrandName.Text = string.Empty;
                BrandDescription.Text = string.Empty;
            }
            else
            {
                txtbTitle.Text = "Edit Brand";
            }
        }

        private void _LoadData()
        {
            _Brand = clsBrand.FindByID(_BrandID);

            if (_Brand == null)
            {
                MessageBox.Show("Brand record not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            BrandName.Text = _Brand.Name;
            BrandDescription.Text = _Brand.Description;
        }

        // ============================
        // LIVE VALIDATION
        // ============================
        private void BrandNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingForm) return;

            var control = sender as ModernInput;
            if (control == null || string.IsNullOrWhiteSpace(control.Text)) return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();

                bool exists = _FormMode == enMode.Update
                    ? clsBrand.IsBrandExistByName(text, _BrandID)
                    : clsBrand.IsBrandExistByName(text);

                return exists ? "This brand name already exists." : null;
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
            _Brand.Name = BrandName.Text.Trim();
            _Brand.Description = BrandDescription.Text.Trim();

            try
            {
                bool saved = _Brand.Save();

                if (!saved)
                {
                    // Save() returned false — validation blocked it (name taken, etc.)
                    // No need to log — nothing exceptional happened
                    ShowErrorMessage(new List<string> { "• Failed to save. The brand name may already exist." });
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
                    MessageBoxImage.Error
                );
            }
        }

        // ============================
        // VALIDATION
        // ============================
        private ValidationResult _ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // Required + format check
            BrandName.ValidateForce();
            if (!BrandName.IsValid)
            {
                errors.Add($"• {BrandName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = BrandName;
            }

            // Duplicate name check
            BrandName.Validate(live: false, externalValidator: text =>
            {
                bool exists = _FormMode == enMode.Update
                    ? clsBrand.IsBrandExistByName(text.Trim(), _BrandID)
                    : clsBrand.IsBrandExistByName(text.Trim());

                return exists ? "This brand name already exists." : null;
            });

            BrandDescription.ValidateForce();
            if (!BrandDescription.IsValid)
            {
                errors.Add($"• {BrandDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = BrandDescription;
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

        private void ScrollToFirstError(FrameworkElement control)
        {
            control?.BringIntoView();
        }

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