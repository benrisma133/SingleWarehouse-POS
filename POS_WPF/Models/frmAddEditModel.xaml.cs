// frmAddEditModel.cs
using POS_BLL;
using POS_WPF.Controls;
using POS_WPF.Serie;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Models
{
    public partial class frmAddEditModel : Window
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;
        public Action<int, string> OnModelSaved;

        private bool _isLoadingForm = false;

        private int _ModelID;
        private clsModel _Model;
        private int _SelectedSerieID = -1;

        // ============================
        // CONSTRUCTORS
        // ============================
        public frmAddEditModel()
        {
            InitializeComponent();
            _FormMode = enMode.AddNew;
        }

        public frmAddEditModel(int modelID)
        {
            InitializeComponent();
            _ModelID = modelID;
            _FormMode = enMode.Update;
        }

        // ============================
        // WINDOW EVENTS
        // ============================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoadingForm = true;

            _ResetDefaultValues();
            _LoadSeriesToComboBox();

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

        private void cmbSerie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSerie.SelectedItem is ComboBoxItem selected)
                _SelectedSerieID = (int)selected.Tag;
        }

        private void btnManageWarehouses_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditSerie frmAdd = new frmAddEditSerie();
            frmAdd.ShowDialog();
            _LoadSeriesToComboBox();
        }

        // ============================
        // LOAD & RESET
        // ============================
        private void _ResetDefaultValues()
        {
            if (_FormMode == enMode.AddNew)
            {
                _Model = new clsModel();
                txtbTitle.Text = "Add New Model";
                ModelName.Text = string.Empty;
                ModelDescription.Text = string.Empty;
            }
            else
            {
                txtbTitle.Text = "Edit Model";
            }
        }

        private void _LoadData()
        {
            _Model = clsModel.FindByID(_ModelID);

            if (_Model == null)
            {
                MessageBox.Show("Model record not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            ModelName.Text = _Model.Name;
            ModelDescription.Text = _Model.Description;

            // Select the matching serie in the combo box
            foreach (ComboBoxItem item in cmbSerie.Items)
            {
                if ((int)item.Tag == _Model.SerieID)
                {
                    cmbSerie.SelectedItem = item;
                    _SelectedSerieID = _Model.SerieID ?? -1;
                    break;
                }
            }
        }

        private void _LoadSeriesToComboBox()
        {
            try
            {
                cmbSerie.Items.Clear();

                DataTable dt = clsSeries.GetAll();

                foreach (DataRow row in dt.Rows)
                {
                    cmbSerie.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["SeriesID"])
                    });
                }

                if (cmbSerie.Items.Count > 0)
                    cmbSerie.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading series: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================
        // LIVE VALIDATION
        // ============================
        private void ModelNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingForm) return;

            var control = sender as ModernInput;
            if (control == null || string.IsNullOrWhiteSpace(control.Text)) return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();

                bool exists = _FormMode == enMode.Update
                    ? clsModel.IsModelExistsByNameExcludingID(text, _ModelID)
                    : clsModel.IsModelExistsByName(text);

                return exists ? "This model name already exists." : null;
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
            _Model.Name = ModelName.Text.Trim();
            _Model.Description = ModelDescription.Text.Trim();
            _Model.SerieID = cmbSerie.SelectedItem is ComboBoxItem selectedItem
                ? (int)selectedItem.Tag
                : (int?)null;

            try
            {
                bool saved = _Model.Save();

                if (!saved)
                {
                    ShowErrorMessage(new List<string>
                    {
                        "• Failed to save. The model name may already exist."
                    });
                    return;
                }

                IsSaved = true;
                OnModelSaved?.Invoke(_Model.ModelID, _Model.Name);
                ShowSuccessMessage();
            }
            catch (Exception)
            {
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

            // Required + format check
            ModelName.ValidateForce();

            // Duplicate name check
            ModelName.Validate(live: false, externalValidator: text =>
            {
                bool exists = _FormMode == enMode.Update
                    ? clsModel.IsModelExistsByNameExcludingID(text.Trim(), _ModelID)
                    : clsModel.IsModelExistsByName(text.Trim());

                return exists ? "This model name already exists." : null;
            });

            if (!ModelName.IsValid)
            {
                errors.Add($"• {ModelName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ModelName;
            }

            ModelDescription.ValidateForce();
            if (!ModelDescription.IsValid)
            {
                errors.Add($"• {ModelDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ModelDescription;
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