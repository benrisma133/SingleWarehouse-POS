// frmAddEditProduct.cs
using POS_BLL;
using POS_WPF.Category;
using POS_WPF.Controls;
using POS_WPF.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Product
{
    public partial class frmAddEditProduct : Window
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;
        public Action<int, string> OnProductSaved;

        private bool _isLoadingForm = false;

        private int _ProductID;
        private clsProduct _Product;

        // ============================
        // CONSTRUCTORS
        // ============================
        public frmAddEditProduct()
        {
            InitializeComponent();
            _FormMode = enMode.AddNew;
        }

        public frmAddEditProduct(int productID)
        {
            InitializeComponent();
            _ProductID = productID;
            _FormMode = enMode.Update;
        }

        // ============================
        // WINDOW EVENTS
        // ============================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoadingForm = true;

            _ResetDefaultValues();
            _LoadCategoriesToComboBox();
            _LoadModelsToComboBox();

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
                _Product = new clsProduct();
                txtbTitle.Text = "Add New Product";
                ProductName.Text = string.Empty;
                ProductDescription.Text = string.Empty;
                ProductPrice.Text = string.Empty;
                ProductQuantity.Text = string.Empty;
            }
            else
            {
                txtbTitle.Text = "Edit Product";
            }
        }

        private void _LoadData()
        {
            _Product = clsProduct.FindByID(_ProductID);

            if (_Product == null)
            {
                MessageBox.Show("Product record not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            ProductName.Text = _Product.ProductName;
            ProductDescription.Text = _Product.Description;
            ProductPrice.Text = _Product.Price.ToString();
            ProductQuantity.Text = _Product.Quantity.ToString();

            foreach (ComboBoxItem item in cmbCategory.Items)
            {
                if ((int)item.Tag == _Product.CategoryID)
                {
                    cmbCategory.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in cmbModel.Items)
            {
                if ((int)item.Tag == _Product.ModelID)
                {
                    cmbModel.SelectedItem = item;
                    break;
                }
            }
        }

        private void _LoadCategoriesToComboBox()
        {
            try
            {
                cmbCategory.Items.Clear();
                DataTable dt = clsCategory.GetAll();

                foreach (DataRow row in dt.Rows)
                {
                    cmbCategory.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["CategoryID"])
                    });
                }

                if (cmbCategory.Items.Count > 0)
                    cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _LoadModelsToComboBox()
        {
            try
            {
                cmbModel.Items.Clear();
                DataTable dt = clsModel.GetAll();

                foreach (DataRow row in dt.Rows)
                {
                    cmbModel.Items.Add(new ComboBoxItem
                    {
                        Content = row["Name"].ToString(),
                        Tag = Convert.ToInt32(row["ModelID"])
                    });
                }

                if (cmbModel.Items.Count > 0)
                    cmbModel.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading models:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            _Product.ProductName = ProductName.Text.Trim();
            _Product.Description = ProductDescription.Text.Trim();
            _Product.Price = decimal.Parse(ProductPrice.Text.Trim());
            _Product.Quantity = int.Parse(ProductQuantity.Text.Trim());

            _Product.CategoryID = cmbCategory.SelectedItem is ComboBoxItem cat
                ? (int)cat.Tag : -1;

            _Product.ModelID = cmbModel.SelectedItem is ComboBoxItem mod
                ? (int)mod.Tag : -1;

            try
            {
                bool saved = _Product.Save();

                if (!saved)
                {
                    ShowErrorMessage(new List<string>
                    {
                        "• Failed to save. Please check the entered data and try again."
                    });
                    return;
                }

                IsSaved = true;
                OnProductSaved?.Invoke(_Product.ProductID, _Product.ProductName);
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

            // ── Product Name ────────────────────────────────────────────────────────
            ProductName.ValidateForce();
            if (!ProductName.IsValid)
            {
                errors.Add($"• {ProductName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductName;
            }

            // ── Description ─────────────────────────────────────────────────────────
            ProductDescription.ValidateForce();
            if (!ProductDescription.IsValid)
            {
                errors.Add($"• {ProductDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductDescription;
            }

            // ── Price (format + range) ───────────────────────────────────────────────
            ProductPrice.ValidateForce();
            if (!ProductPrice.IsValid)
            {
                errors.Add($"• {ProductPrice.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductPrice;
            }
            else if (!decimal.TryParse(ProductPrice.Text, out decimal price) || price <= 0)
            {
                errors.Add("• Price must be a valid number greater than zero.");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductPrice;
            }

            // ── Quantity (format + range) ────────────────────────────────────────────
            ProductQuantity.ValidateForce();
            if (!ProductQuantity.IsValid)
            {
                errors.Add($"• {ProductQuantity.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductQuantity;
            }
            else if (!int.TryParse(ProductQuantity.Text, out int qty) || qty < 0)
            {
                errors.Add("• Quantity must be a valid non-negative number.");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = ProductQuantity;
            }

            // ── Category ─────────────────────────────────────────────────────────────
            if (cmbCategory.SelectedItem == null)
            {
                errors.Add("• Please select a category.");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = cmbCategory;
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
        // ADD BUTTONS
        // ============================
        private void btnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditCategory frm = new frmAddEditCategory();
            frm.ShowDialog();
            _LoadCategoriesToComboBox();
        }

        private void btnAddModel_Click(object sender, RoutedEventArgs e)
        {
            frmAddEditModel frm = new frmAddEditModel();
            frm.ShowDialog();
            _LoadModelsToComboBox();
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