using POS_BLL;
using POS_WPF.Brand; // adjust namespace to where frmAddEditBrand lives
using POS_WPF.Dialogs;
using POS_WPF.UI;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace POS_WPF.Pages
{
    public partial class BrandPage : UserControl
    {
        private bool _isLoadingWarehouseFilter = false;
        public bool IsSidebarToggling { get; set; } = false;

        public BrandPage()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            this.SizeChanged += UserControl_SizeChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cmbStatus.SelectedIndex = 1; // Default to "Active"
            await LoadBrandsAsync();
            DynamicCardContainer.PreviewMouseWheel += DynamicCardContainer_PreviewMouseWheel;
        }

        private void DynamicCardContainer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CardScrollViewer != null)
            {
                CardScrollViewer.ScrollToVerticalOffset(CardScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private string GetSearchText()
        {
            return txtSearch.Dispatcher.Invoke(() =>
                txtSearch?.Text?.ToLower() ?? "");
        }


        // ======================= LOAD BRANDS =======================
        public async Task LoadBrandsAsync(int? warehouseID = null)
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                await Task.Delay(50);

                string filter = GetSearchText();
                string warehouseName = "All Warehouses";

                // Get filter status from ComboBox
                string statusFilter = (cmbStatus.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "all";

                // Fetch + filter off UI thread
                var brandData = await Task.Run(() =>
                {
                    DataTable dt = clsBrand.GetAll();

                    return dt.AsEnumerable()
                        .Where(row =>
                        {
                            string name = row["Name"].ToString().ToLower();
                            string description = row["Description"].ToString().ToLower();
                            int isActive = Convert.ToInt32(row["IsActive"]);

                            // Search filter
                            bool matchesSearch = name.Contains(filter) || description.Contains(filter);

                            // Status filter
                            bool matchesStatus = statusFilter == "all" ||
                                                (statusFilter == "active" && isActive == 1) ||
                                                (statusFilter == "inactive" && isActive == 0);

                            return matchesSearch && matchesStatus;
                        })
                        .Select(row => new
                        {
                            BrandID = Convert.ToInt32(row["BrandID"]),
                            Name = row["Name"].ToString(),
                            Description = row["Description"].ToString(),
                            IsActive = Convert.ToInt32(row["IsActive"]) == 1
                        })
                        .ToList();
                });

                // Build cards
                DynamicCardContainer.Items.Clear();
                double cardWidth = GetCardWidth();

                foreach (var item in brandData)
                {
                    DynamicCardContainer.Items.Add(
                        CreateBrandCard(item.BrandID, item.Name, item.Description, item.IsActive, cardWidth));
                }

                // Fix WrapPanel layout
                var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                if (wrapPanel != null)
                {
                    wrapPanel.Margin = new Thickness(14);
                    wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                }

                UpdateCardWidths();

                if (txtTitle != null)
                    txtTitle.Text = $"Brand Management - {warehouseName}";

                if (txtTotalBrands != null)
                    txtTotalBrands.Text = $"Total Brands : {brandData.Count}";

                if (NoBrandsMessage != null)
                {
                    NoBrandsMessage.Text = $"No brands for {warehouseName}";
                    NoBrandsMessage.Visibility = brandData.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Brand Management - Error";

                MessageBox.Show("Error loading brands: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingOverlay != null)
                {
                    await Task.Delay(100);
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        // ======================= CARD FACTORY =======================
        private Border CreateBrandCard(int id, string name, string description, bool isActive, double width)
        {
            Border cardBorder = new Border
            {
                Width = Math.Max(0, width),
                Background = new SolidColorBrush(Color.FromRgb(250, 251, 252)),
                CornerRadius = new CornerRadius(16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 12, 12),
                SnapsToDevicePixels = true,
                Cursor = Cursors.Hand,
                Tag = id,
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 20),
                CacheMode = new BitmapCache()
            };

            cardBorder.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(148, 163, 184),
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.15
            };

            // Layout grid
            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ── Left : brand initial badge + text ──
            StackPanel textStack = new StackPanel();

            StackPanel badgeAndNameStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 4)
            };

            // Coloured initial badge
            Border badge = new Border
            {
                Width = 54,
                Height = 54,
                Margin = new Thickness(8, 0, 18, 0),
                Background = new SolidColorBrush(Color.FromRgb(47, 136, 255)),
                CornerRadius = new CornerRadius(10)
            };
            badge.Child = new TextBlock
            {
                Text = name.Length > 0 ? name[0].ToString().ToUpper() : "B",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeAndNameStack.Children.Add(badge);

            // Name
            badgeAndNameStack.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                FontFamily = new FontFamily("Segoe UI"),
                VerticalAlignment = VerticalAlignment.Center
            });

            textStack.Children.Add(badgeAndNameStack);

            // Description
            bool hasDesc = !string.IsNullOrWhiteSpace(description);
            textStack.Children.Add(new TextBlock
            {
                Text = hasDesc ? description : "No description available",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(
                    hasDesc ? Color.FromRgb(100, 116, 139) : Color.FromRgb(148, 163, 184)),
                FontFamily = new FontFamily("Segoe UI"),
                LineHeight = 20,
                FontStyle = hasDesc ? FontStyles.Normal : FontStyles.Italic
            });

            cardGrid.Children.Add(textStack);

            // ── Right : action buttons (Toggle, Edit, Delete) ──
            StackPanel buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(buttonStack, 1);

            Button toggleBtn = CardButtonsFactory.CreateToggleButton(BtnToggle_Click, id, isActive);
            Button editBtn = CardButtonsFactory.CreateEditButton(BtnEdit_Click, id);
            Button deleteBtn = CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id);

            toggleBtn.Margin = new Thickness(0, 0, 8, 0);
            editBtn.Margin = new Thickness(0, 0, 8, 0);
            deleteBtn.Margin = new Thickness(0, 0, 0, 0);

            buttonStack.Children.Add(toggleBtn);
            buttonStack.Children.Add(editBtn);
            buttonStack.Children.Add(deleteBtn);
            cardGrid.Children.Add(buttonStack);

            cardBorder.Child = cardGrid;

            // ── Entrance animation ──
            cardBorder.Loaded += (s, e) =>
            {
                if (IsSidebarToggling)
                {
                    cardBorder.Opacity = 1;
                    ((TranslateTransform)cardBorder.RenderTransform).Y = 0;
                    return;
                }

                TranslateTransform translate;
                ScaleTransform scale;
                TransformGroup tg;

                if (cardBorder.RenderTransform is TransformGroup group)
                {
                    tg = group;
                    translate = group.Children.OfType<TranslateTransform>().FirstOrDefault()
                                ?? new TranslateTransform(0, 20);
                    scale = group.Children.OfType<ScaleTransform>().FirstOrDefault()
                                ?? new ScaleTransform(1, 1);
                    if (!tg.Children.Contains(translate)) tg.Children.Add(translate);
                    if (!tg.Children.Contains(scale)) tg.Children.Add(scale);
                }
                else
                {
                    translate = new TranslateTransform(0, 20);
                    scale = new ScaleTransform(1, 1);
                    tg = new TransformGroup();
                    tg.Children.Add(scale);
                    tg.Children.Add(translate);
                    cardBorder.RenderTransform = tg;
                }

                var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

                translate.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(0, TimeSpan.FromMilliseconds(400)) { EasingFunction = ease });

                cardBorder.BeginAnimation(Border.OpacityProperty,
                    new DoubleAnimation(1, TimeSpan.FromMilliseconds(400)) { EasingFunction = ease });
            };

            // ── Hover ──
            cardBorder.MouseEnter += (s, e) => Card_MouseEnterAnimations(cardBorder, buttonStack);
            cardBorder.MouseLeave += (s, e) => Card_MouseLeaveAnimations(cardBorder, buttonStack);

            // ── Scroll passthrough ──
            cardBorder.PreviewMouseWheel += (s, e) =>
            {
                CardScrollViewer?.ScrollToVerticalOffset(
                    CardScrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            };

            return cardBorder;
        }

        // ======================= Hover Animations =======================
        private void Card_MouseEnterAnimations(Border cardBorder, StackPanel buttonStack)
        {
            ScaleTransform scale;
            TransformGroup tg;

            if (cardBorder.RenderTransform is TransformGroup group)
            {
                tg = group;
                scale = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (scale == null) { scale = new ScaleTransform(1, 1); tg.Children.Add(scale); }
            }
            else
            {
                scale = new ScaleTransform(1, 1);
                var tr = cardBorder.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
                tg = new TransformGroup();
                tg.Children.Add(scale);
                tg.Children.Add(tr);
                cardBorder.RenderTransform = tg;
            }

            cardBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });

            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(155, 155, 155), TimeSpan.FromMilliseconds(350)));

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(18, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.28, TimeSpan.FromMilliseconds(260)));
            }

            buttonStack.Visibility = Visibility.Visible;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
        }

        private void Card_MouseLeaveAnimations(Border cardBorder, StackPanel buttonStack)
        {
            if (cardBorder.RenderTransform is TransformGroup tg)
            {
                var scale = tg.Children.OfType<ScaleTransform>().FirstOrDefault();
                scale?.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
                scale?.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
            }

            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(223, 230, 233), TimeSpan.FromMilliseconds(350)));

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(12, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.15, TimeSpan.FromMilliseconds(260)));
            }

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (s, e) => buttonStack.Visibility = Visibility.Collapsed;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }

        // ======================= Helpers =======================
        private double GetCardWidth()
        {
            var wp = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wp == null) return 400;
            double avail = wp.ActualWidth - wp.Margin.Left - wp.Margin.Right;
            return avail > 900 ? 420 : avail - 20;
        }

        private void UpdateCardWidths()
        {
            var wp = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wp == null) return;

            int count = wp.Children.Count;
            double avail = CardScrollViewer.ActualWidth - wp.Margin.Left - wp.Margin.Right - 16;

            foreach (var child in wp.Children)
            {
                if (child is Border card)
                {
                    card.Width = count == 1 ? avail
                               : count == 2 ? (avail / 2) - 16
                               : avail > 760 ? (avail / 2) - 16 : avail - 16;
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        // ======================= Events =======================
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadBrandsAsync();
            UpdateCardWidths();
        }

        private async void cmbWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingWarehouseFilter) return;
            await LoadBrandsAsync();
        }

        private async void BtnAddBrand_Click(object sender, RoutedEventArgs e)
        {
            var win = new frmAddEditBrand();
            win.Owner = Application.Current.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();
            if (win.IsSaved)
                await LoadBrandsAsync();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int brandId)
            {
                var win = new frmAddEditBrand(brandId);
                win.Owner = Application.Current.MainWindow;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                win.ShowDialog();
                if (win.IsSaved)
                    await LoadBrandsAsync();
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int brandId)
            {
                // ✔ Check dependencies
                var canDelete = clsBrand.CanDelete(brandId, out var deps);

                // ❌ Cannot delete → show warning dialog
                if (!canDelete)
                {
                    var dialog = new frmConfirmDelete(
                        title: "Warning",
                        message:
                            $"Cannot delete this brand.\n\n" +
                            $"It is linked with:\n" +
                            $"- {deps.Series} Series\n" +
                            $"- {deps.Models} Models\n" +
                            $"- {deps.Products} Products\n\n" +
                            $"Do you want to delete anyway?");

                    dialog.Owner = Window.GetWindow(this);

                    if (dialog.ShowDialog() != true)
                        return;

                    // user chose "Delete Anyway"
                    if (clsBrand.Delete(brandId))
                    {
                        await LoadBrandsAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Unexpected error while deleting brand.\nPlease contact support.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    return;
                }

                // ✔ No dependencies → simple confirm
                var simpleDialog = new frmConfirmDelete(
                    title: "Confirm Delete",
                    message: "This will permanently delete this brand.\n\nAre you sure you want to continue?");

                simpleDialog.Owner = Window.GetWindow(this);

                if (simpleDialog.ShowDialog() != true)
                    return;

                if (clsBrand.Delete(brandId))
                {
                    await LoadBrandsAsync();
                }
                else
                {
                    MessageBox.Show(
                        "Unexpected error while deleting brand.\nPlease contact support.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int brandId)
            {
                // Get current active status from database
                bool isCurrentlyActive = clsBrand.GetActiveStatus(brandId);
                bool newState = !isCurrentlyActive;

                // Update in database
                bool success = clsBrand.SetActiveStatus(brandId, newState);

                if (success)
                {
                    // Update button UI
                    CardButtonsFactory.SetToggleState(btn, newState);

                    // Show notification
                    string message = newState ? "Brand activated successfully." : "Brand deactivated successfully.";
                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reload to reflect changes
                    await LoadBrandsAsync();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to update brand status.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStatus.SelectedItem != null)
            {
                await LoadBrandsAsync();
            }
        }

    }
}