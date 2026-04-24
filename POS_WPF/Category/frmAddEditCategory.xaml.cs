using POS_BLL;
using POS_WPF.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace POS_WPF.Category
{
    public partial class frmAddEditCategory : Window
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _FormMode = enMode.AddNew;

        public bool IsSaved { get; private set; } = false;
        public Action<int, string> OnCategorySaved;

        private bool _isLoadingForm = false;

        private int _CategoryID;
        private clsCategory _Category;

        private string _selectedIconData = null;
        private int _selectedIconID = -1;

        // ============================
        // CONSTRUCTORS
        // ============================
        public frmAddEditCategory()
        {
            InitializeComponent();
            _FormMode = enMode.AddNew;
        }

        public frmAddEditCategory(int categoryID)
        {
            InitializeComponent();
            _CategoryID = categoryID;
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
                _Category = new clsCategory();
                txtbTitle.Text = "Add New Category";
                CategoryName.Text = string.Empty;
                CategoryDescription.Text = string.Empty;
            }
            else
            {
                txtbTitle.Text = "Edit Category";
            }
        }

        private void _LoadData()
        {
            _Category = clsCategory.FindByID(_CategoryID);

            if (_Category == null)
            {
                MessageBox.Show("Category record not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            CategoryName.Text = _Category.Name;
            CategoryDescription.Text = _Category.Description;

            _LoadIconPreview();
        }

        private void _LoadIconPreview()
        {
            if (!_Category.IconID.HasValue)
                return;

            var icon = clsCategoryIcon.FindByID(_Category.IconID.Value);
            if (icon == null || string.IsNullOrWhiteSpace(icon.IconData))
                return;

            _selectedIconID = icon.IconID;
            _selectedIconData = icon.IconData;

            Canvas iconCanvas = _BuildIconCanvas(icon.IconData, strokeDivisor: 18);
            if (iconCanvas != null)
            {
                vbSelectedIcon.Child = iconCanvas;
                vbSelectedIcon.Visibility = Visibility.Visible;
                btnRemoveIcon.Visibility = Visibility.Visible;
            }
            else
            {
                vbSelectedIcon.Visibility = Visibility.Collapsed;
                btnRemoveIcon.Visibility = Visibility.Collapsed;
            }
        }

        // ============================
        // LIVE VALIDATION
        // ============================
        private void CategoryNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingForm) return;

            var control = sender as ModernInput;
            if (control == null || string.IsNullOrWhiteSpace(control.Text)) return;

            control.Validate(live: true, externalValidator: text =>
            {
                text = text.Trim();

                bool exists = _FormMode == enMode.Update
                    ? clsCategory.IsCategoryExistByNameExceptID(text, _CategoryID)
                    : clsCategory.IsCategoryExistByName(text);

                return exists ? "This category name already exists." : null;
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
            _Category.Name = CategoryName.Text.Trim();
            _Category.Description = CategoryDescription.Text.Trim();
            _Category.IconID = _selectedIconID != -1 ? _selectedIconID : (int?)null;

            try
            {
                bool saved = _Category.Save();

                if (!saved)
                {
                    ShowErrorMessage(new List<string>
                    {
                        "• Failed to save. The category name may already exist."
                    });
                    return;
                }

                IsSaved = true;
                OnCategorySaved?.Invoke(_Category.CategoryID, _Category.Name);
                ShowSuccessMessage();
            }
            catch (Exception)
            {
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
            CategoryName.ValidateForce();

            // Duplicate name check
            CategoryName.Validate(live: false, externalValidator: text =>
            {
                bool exists = _FormMode == enMode.Update
                    ? clsCategory.IsCategoryExistByNameExceptID(text.Trim(), _CategoryID)
                    : clsCategory.IsCategoryExistByName(text.Trim());

                return exists ? "This category name already exists." : null;
            });

            if (!CategoryName.IsValid)
            {
                errors.Add($"• {CategoryName.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = CategoryName;
            }

            CategoryDescription.ValidateForce();
            if (!CategoryDescription.IsValid)
            {
                errors.Add($"• {CategoryDescription.ValidationMessageText}");
                if (result.FirstInvalidControl == null)
                    result.FirstInvalidControl = CategoryDescription;
            }

            if (errors.Any())
            {
                result.IsValid = false;
                result.Errors = errors;
            }

            return result;
        }

        // ============================
        // ICON PICKER
        // ============================
        private void BtnSetIcon_Click(object sender, RoutedEventArgs e)
        {
            grdIconPickerOverlay.Visibility = Visibility.Visible;
            _LoadIcons();
        }

        private void BtnRemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            _selectedIconData = null;
            _selectedIconID = -1;
            vbSelectedIcon.Visibility = Visibility.Collapsed;
            btnRemoveIcon.Visibility = Visibility.Collapsed;

            _DeselectAllIconButtons();
        }

        private void BtnIconCancel_Click(object sender, RoutedEventArgs e)
        {
            grdIconPickerOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnIconSelect_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIconData == null)
            {
                MessageBox.Show("Please select an icon.", "No Icon Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _UpdateSelectedIconPreview();
            grdIconPickerOverlay.Visibility = Visibility.Collapsed;
        }

        private void _LoadIcons()
        {
            wpIcons.Children.Clear();
            wpIcons.Margin = new Thickness(8);

            DataTable dt = clsCategoryIcon.GetAll();

            foreach (DataRow row in dt.Rows)
            {
                int iconID = Convert.ToInt32(row["IconID"]);
                string iconName = row["IconName"].ToString();
                string iconJson = row["IconData"]?.ToString();

                if (string.IsNullOrWhiteSpace(iconJson))
                    continue;

                try
                {
                    Canvas canvas = _BuildIconCanvas(iconJson, strokeDivisor: 22);
                    if (canvas == null)
                        continue;

                    Viewbox iconViewbox = new Viewbox
                    {
                        Width = 40,
                        Height = 40,
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.Both,
                        Child = canvas
                    };

                    StackPanel iconStack = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    iconStack.Children.Add(iconViewbox);
                    iconStack.Children.Add(new TextBlock
                    {
                        Text = iconName,
                        FontSize = 10,
                        Foreground = Brushes.Black,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 4, 0, 0),
                        MaxWidth = 64
                    });

                    Button btn = new Button
                    {
                        Width = 68,
                        Height = 80,
                        Style = (Style)FindResource("IconPickerButtonStyle"),
                        Content = iconStack,
                        Padding = new Thickness(2)
                    };

                    var iconInfo = new IconInfo { IconID = iconID, Selected = false, ButtonRef = btn };
                    btn.Tag = iconInfo;

                    btn.MouseEnter += (s, ev) =>
                    {
                        if (!iconInfo.Selected)
                        {
                            btn.BorderBrush = Brushes.Gray;
                            btn.BorderThickness = new Thickness(2);
                        }
                    };

                    btn.MouseLeave += (s, ev) =>
                    {
                        if (!iconInfo.Selected)
                        {
                            btn.BorderBrush = Brushes.Transparent;
                            btn.BorderThickness = new Thickness(0);
                        }
                    };

                    btn.Click += (s, ev) =>
                    {
                        _selectedIconData = iconJson;
                        _selectedIconID = iconID;

                        _DeselectAllIconButtons();

                        iconInfo.Selected = true;
                        btn.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF8C42"));
                        btn.BorderThickness = new Thickness(2);
                    };

                    wpIcons.Children.Add(btn);
                }
                catch
                {
                    continue;
                }
            }
        }

        private void _UpdateSelectedIconPreview()
        {
            if (string.IsNullOrWhiteSpace(_selectedIconData))
            {
                vbSelectedIcon.Visibility = Visibility.Collapsed;
                btnRemoveIcon.Visibility = Visibility.Collapsed;
                return;
            }

            Canvas canvas = _BuildIconCanvas(_selectedIconData, strokeDivisor: 22);
            if (canvas != null)
            {
                vbSelectedIcon.Child = canvas;
                vbSelectedIcon.Visibility = Visibility.Visible;
                btnRemoveIcon.Visibility = Visibility.Visible;
            }
            else
            {
                vbSelectedIcon.Visibility = Visibility.Collapsed;
                btnRemoveIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void _DeselectAllIconButtons()
        {
            foreach (Button b in wpIcons.Children)
            {
                if (b.Tag is IconInfo info)
                {
                    info.Selected = false;
                    b.BorderBrush = Brushes.Transparent;
                    b.BorderThickness = new Thickness(0);
                }
            }
        }

        // ============================
        // ICON CANVAS BUILDER
        // ============================

        /// <summary>
        /// Parses a JSON array of SVG path strings and returns a sized Canvas,
        /// or null if the data is invalid / empty.
        /// </summary>
        private Canvas _BuildIconCanvas(string iconJson, double strokeDivisor = 22)
        {
            if (string.IsNullOrWhiteSpace(iconJson))
                return null;

            try
            {
                JArray paths = JArray.Parse(iconJson);
                if (paths.Count == 0)
                    return null;

                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;
                var pathList = new List<Path>();

                foreach (var token in paths)
                {
                    string pathData = token.ToString();
                    if (string.IsNullOrWhiteSpace(pathData))
                        continue;

                    Geometry geometry = Geometry.Parse(pathData);
                    Rect bounds = geometry.Bounds;

                    minX = Math.Min(minX, bounds.Left);
                    minY = Math.Min(minY, bounds.Top);
                    maxX = Math.Max(maxX, bounds.Right);
                    maxY = Math.Max(maxY, bounds.Bottom);

                    pathList.Add(new Path { Data = geometry });
                }

                if (pathList.Count == 0 || maxX <= minX || maxY <= minY)
                    return null;

                double iconSize = Math.Max(maxX - minX, maxY - minY);
                double strokeThickness = iconSize / strokeDivisor;

                Canvas canvas = new Canvas
                {
                    Width = maxX - minX,
                    Height = maxY - minY
                };

                foreach (Path path in pathList)
                {
                    path.Stroke = Brushes.Black;
                    path.StrokeThickness = strokeThickness;
                    path.StrokeStartLineCap = PenLineCap.Round;
                    path.StrokeEndLineCap = PenLineCap.Round;
                    path.StrokeLineJoin = PenLineJoin.Round;
                    path.Fill = Brushes.Transparent;
                    path.RenderTransform = new TranslateTransform(-minX, -minY);
                    canvas.Children.Add(path);
                }

                return canvas;
            }
            catch
            {
                return null;
            }
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
        // HELPER CLASSES
        // ============================
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
            public FrameworkElement FirstInvalidControl { get; set; }
        }

        public class IconInfo
        {
            public int IconID { get; set; }
            public bool Selected { get; set; }
            public Button ButtonRef { get; set; }
        }
    }
}