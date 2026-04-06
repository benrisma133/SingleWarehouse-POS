using POS_BLL;
using POS_WPF.Controls;
using POS_WPF.Pages;
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
    /// <summary>
    /// Interaction logic for frmAddEditModel.xaml
    /// </summary>
    public partial class frmAddEditModel : Window
    {
        public Action<int, string> OnModelSaved;

        private List<int> _selectedWarehouseIDs = new List<int>();

        private bool _isLoadingWarehouses = false;

        private bool _isLoadingForm = false;
        public bool IsSaved { get; private set; } = false;

        enum enMode { AddNew = 1, Update = 2 }
        enMode FormMode = enMode.AddNew;

        clsModel _Model;
        int _ModelID = -1;
        int _SelectedSerieID = -1;

        public frmAddEditModel()
        {
            InitializeComponent();
            FormMode = enMode.AddNew;
        }

        public frmAddEditModel(int ModelID)
        {
            InitializeComponent();
            FormMode = enMode.Update;
            _ModelID = ModelID;
        }

        private void ModelNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput(sender as ModernInput,
                "This model name already exists.",
                clsModel.IsModelExistsByName,
                name => clsModel.IsModelExistsByNameExcludingID(name ,_ModelID));
        }

        private void ValidateInput(ModernInput control, string errorMessage,
            Func<string, bool> existsFunc, Func<string, bool> existsExceptIdFunc)
        {
            if (_isLoadingForm) return;
            if (control == null) return;

            if (string.IsNullOrWhiteSpace(control.Text))
                return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();

                if (FormMode == enMode.Update)
                {
                    if (existsExceptIdFunc != null && existsExceptIdFunc(text))
                        return errorMessage;
                }
                else
                {
                    if (existsFunc(text))
                        return errorMessage;
                }

                return null;
            });
        }

        private void LoadSeriesToComboBox()
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
                MessageBox.Show("Error loading brands: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetDefaultValues();

            LoadSeriesToComboBox();

            _isLoadingForm = true;
            _isLoadingWarehouses = true;
            if (FormMode == enMode.Update)
            {
                // UPDATE: load actual selection
                LoadData();
            }
            _isLoadingForm = false;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        void ResetDefaultValues()
        {
            if (FormMode == enMode.AddNew)
            {
                _Model = new clsModel();
                txtbTitle.Text = "Add New Model";

                ModelName.Text = "";
                ModelDescription.Text = "";
            }
            else
            {
                txtbTitle.Text = "Edit Model";
            }
        }

        void LoadData()
        {
            _Model = clsModel.FindByID(_ModelID);

            if (_Model == null)
            {
                MessageBox.Show("Model record not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }


            ModelName.Text = _Model.Name;
            ModelDescription.Text = _Model.Description;

            // Select the matching brand in the combo box
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

        void ProcessFormData()
        {

            _Model.Name = ModelName.Text.Trim();
            _Model.Description = ModelDescription.Text;
            _Model.SerieID = cmbSerie.SelectedItem is ComboBoxItem selectedItem
                ? (int)selectedItem.Tag
                : (int?)null;

            if (_Model.Save())
            {

                // ---------------- ADD NEW ----------------
                if (FormMode == enMode.AddNew)
                {

                    OnModelSaved?.Invoke(_Model.ModelID, _Model.Name);

                    MessageBox.Show("New model added successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    IsSaved = true;
                    FormMode = enMode.Update; // Switch to update mode
                    txtbTitle.Text = "Edit Model";
                    _ModelID = _Model.ModelID;
                    return;
                }

                // ---------------- UPDATE ----------------
                if (FormMode == enMode.Update)
                {

                    MessageBox.Show("Model updated successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    IsSaved = true;
                }
            }
            else
            {
                MessageBox.Show("Error saving model record.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            // Hide previous messages
            HideMessages();

            // Validate all fields - This will now show errors even if fields are empty
            ValidationResult validationResults = ValidateAllFields();

            if (validationResults.IsValid)
            {
                // All fields are valid - show success
                ShowSuccessMessage();

                // Optional: Process the form data
                ProcessFormData();
            }
            else
            {
                // Show error messages
                ShowErrorMessage(validationResults.Errors);

                // Scroll to first error
                ScrollToFirstError(validationResults.FirstInvalidControl);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ValidateTextBox(TextBox tb)
        {
            if (!(tb.Tag is TextBlock label) || label == null) return;

            bool isEmpty = string.IsNullOrWhiteSpace(tb.Text);

            // Clone the current BorderBrush to avoid frozen brush issue
            var currentBrush = (tb.BorderBrush as SolidColorBrush)?.Clone() ?? new SolidColorBrush(Colors.Gray);
            tb.BorderBrush = currentBrush; // assign the clone back

            // Animate BorderBrush
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = isEmpty ? Color.FromRgb(231, 76, 60) : Color.FromRgb(204, 204, 204),
                Duration = TimeSpan.FromMilliseconds(300)
            };
            currentBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            // Label text & color
            string baseText = label.Tag?.ToString() ?? "";
            label.Text = isEmpty ? $"{baseText} *" : baseText;
            label.Foreground = new SolidColorBrush(isEmpty ? Color.FromRgb(231, 76, 60) : Color.FromRgb(85, 85, 85));
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
                ValidateTextBox(tb);
        }
         
        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {

            if(sender is TextBox tb)
                ValidateTextBox(tb);

        }

        private ValidationResult ValidateAllFields()
        {
            var result = new ValidationResult { IsValid = true };
            var errors = new List<string>();

            // Validate FullName - FORCE validation even if not interacted
            ModelName.ValidateForce();
            // Force validation with duplicate-name check
            ModelName.Validate(live: false, externalValidator: text =>
            {
                if (FormMode == enMode.Update)
                {
                    if (clsModel.IsModelExistsByNameExcludingID(text.Trim(), _ModelID))
                        return "This model name already exists.";
                }
                else
                {
                    if (clsModel.IsModelExistsByName(text.Trim()))
                        return "This model name already exists.";
                }

                return null;
            });

            // Check the result
            if (!ModelName.IsValid)
            {
                errors.Add($"• {ModelName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ModelName;
            }




            // Validate Bio - FORCE validation
            ModelDescription.ValidateForce();
            if (!ModelDescription.IsValid)
            {
                errors.Add($"• {ModelDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ModelDescription;
            }


            // Set result
            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        private void ShowErrorMessage(List<string> errors)
        {
            ErrorMessageText.Text = string.Join("\n", errors);
            ErrorMessageBox.Visibility = Visibility.Visible;

            // Animate in
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = System.TimeSpan.FromMilliseconds(300)
            };
            ErrorMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            // Optional: Scroll to top to show error
            var scrollViewer = FindScrollViewer(this);
            scrollViewer?.ScrollToTop();
        }

        private void ShowSuccessMessage()
        {
            SuccessMessageBox.Visibility = Visibility.Visible;

            // Animate in
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = System.TimeSpan.FromMilliseconds(300)
            };
            SuccessMessageBox.BeginAnimation(OpacityProperty, fadeIn);

            // Optional: Auto-hide after 5 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(8)
            };
            timer.Tick += (s, e) =>
            {
                HideMessages();
                timer.Stop();
            };
            timer.Start();

            // Scroll to top to show success
            var scrollViewer = FindScrollViewer(this);
            scrollViewer?.ScrollToTop();
        }

        private void HideMessages()
        {
            ErrorMessageBox.Visibility = Visibility.Collapsed;
            SuccessMessageBox.Visibility = Visibility.Collapsed;
        }

        private void ScrollToFirstError(FrameworkElement control)
        {
            if (control != null)
            {
                control.BringIntoView();
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        // Helper class for validation results
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }

        private void btnManageWarehouses_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditSerie frmAdd = new frmAddEditSerie();
            frmAdd.ShowDialog();
            LoadSeriesToComboBox();
        }

        private void cmbSerie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSerie.SelectedItem is ComboBoxItem selected)
                _SelectedSerieID = (int)selected.Tag;
        }
    }

}
