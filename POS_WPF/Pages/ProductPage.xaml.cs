using POS_BLL;
using POS_WPF.Category;
using POS_WPF.Product;
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

namespace POS_WPF.Pages
{
    public partial class ProductPage : UserControl
    {
        private bool _isLoaded = false;

        public ProductPage()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            await LoadProductsAsync();
        }

        private string GetSearchText()
        {
            return txtSearch.Dispatcher.Invoke(() =>
                txtSearch?.Text?.ToLower() ?? "");
        }

        // ================= LOAD PRODUCTS =================
        public async Task LoadProductsAsync()
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                string filter = await Task.Run(() => GetSearchText());
                DataTable dt = await Task.Run(() => clsProduct.GetAll());

                var filteredRows = dt.AsEnumerable()
                    .Where(r =>
                        r["ProductName"].ToString().ToLower().Contains(filter)
                        || r["CategoryName"].ToString().ToLower().Contains(filter)
                        || r["ModelName"].ToString().ToLower().Contains(filter))
                    .ToList();

                DynamicCardContainer.Dispatcher.Invoke(() =>
                {
                    DynamicCardContainer.Items.Clear();
                    double cardWidth = GetCardWidth();

                    if (filteredRows.Count == 0)
                    {
                        NoProductsMessage.Visibility = Visibility.Visible;
                        NoProductsMessage.Text = "No products found";
                    }
                    else
                    {
                        NoProductsMessage.Visibility = Visibility.Collapsed;

                        foreach (var row in filteredRows)
                        {
                            int id = Convert.ToInt32(row["ProductID"]);

                            var card = CreateProductCard(
                                id,
                                row["ProductName"].ToString(),
                                row["CategoryName"].ToString(),
                                row["ModelName"].ToString(),
                                Convert.ToDecimal(row["Price"]),
                                Convert.ToInt32(row["Quantity"]),
                                cardWidth
                            );

                            DynamicCardContainer.Items.Add(card);
                        }

                        var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                        if (wrapPanel != null)
                        {
                            wrapPanel.Margin = new Thickness(14);
                            wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                            wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                        }

                        UpdateCardWidths();
                    }

                    if (txtTitle != null)
                        txtTitle.Text = "Products Management";

                    txtTotalProducts.Text = $"Total Products: {filteredRows.Count}";
                });
            }
            finally
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // ================= CARD WIDTH =================
        private double GetCardWidth()
        {
            var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wrapPanel == null) return 400;

            double availableWidth =
                wrapPanel.ActualWidth - wrapPanel.Margin.Left - wrapPanel.Margin.Right;

            return availableWidth > 900 ? 420 : availableWidth - 20;
        }

        private StackPanel CreateButtonWithHoverText(Button btn, string hoverText)
        {
            StackPanel container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock text = new TextBlock
            {
                Text = hoverText,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4),
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            container.Children.Add(text);
            container.Children.Add(btn);

            btn.MouseEnter += (_, __) =>
            {
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250));
                text.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            btn.MouseLeave += (_, __) =>
            {
                var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));
                text.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };

            return container;
        }

        private Border CreateProductCard(
            int id,
            string name,
            string category,
            string model,
            decimal price,
            int quantity,
            double width)
        {
            // ---------- CARD BORDER ----------
            Border cardBorder = new Border
            {
                Width = Math.Max(0, width),
                Background = new SolidColorBrush(Color.FromRgb(250, 251, 252)),
                CornerRadius = new CornerRadius(16),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 12, 12),
                Cursor = Cursors.Hand,
                Tag = id,
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        new ScaleTransform(1, 1),
                        new TranslateTransform(0, 20)
                    }
                },
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(148, 163, 184),
                    BlurRadius = 12,
                    ShadowDepth = 2,
                    Opacity = 0.15
                }
            };

            // ---------- ROOT GRID ----------
            Grid rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // ---------- CONTENT GRID ----------
            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

            // ---------- TEXT STACK ----------
            StackPanel textStack = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };

            textStack.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 8)
            });

            textStack.Children.Add(new TextBlock
            {
                Text = $"Category: {category}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 4)
            });

            textStack.Children.Add(new TextBlock
            {
                Text = $"Model: {model}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 8)
            });

            textStack.Children.Add(new TextBlock
            {
                Text = $"{price:C}",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Stock Status
            if (quantity == 0 || quantity < 10)
            {
                Color bgColor = quantity == 0 ? Color.FromRgb(239, 68, 68) : Color.FromRgb(251, 146, 60);
                string warningText = quantity == 0 ? "Out of stock" : "Low stock";

                Border alertBorder = new Border
                {
                    Background = new SolidColorBrush(bgColor),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 6, 10, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 4),
                    MaxWidth = 220
                };

                alertBorder.Child = new TextBlock
                {
                    Text = $"{quantity} units · {warningText}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 13,
                    FontFamily = new FontFamily("Segoe UI"),
                    TextAlignment = TextAlignment.Center
                };

                textStack.Children.Add(alertBorder);
            }
            else
            {
                Border stockBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(236, 253, 245)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 6, 10, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 4)
                };

                stockBorder.Child = new TextBlock
                {
                    Text = $"{quantity} units in stock",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105)),
                    FontWeight = FontWeights.SemiBold,
                    FontFamily = new FontFamily("Segoe UI")
                };

                textStack.Children.Add(stockBorder);
            }

            Grid.SetColumn(textStack, 0);
            contentGrid.Children.Add(textStack);

            // ---------- BUTTONS STACK ----------
            StackPanel buttonsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };

            buttonsStack.Children.Add(CreateButtonWithHoverText(
                CardButtonsFactory.CreateEditButton(BtnEdit_Click, id), "Edit"));
            buttonsStack.Children.Add(CreateButtonWithHoverText(
                CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id), "Delete"));

            Grid.SetColumn(buttonsStack, 1);
            contentGrid.Children.Add(buttonsStack);

            Grid.SetRow(contentGrid, 0);
            rootGrid.Children.Add(contentGrid);

            cardBorder.Child = rootGrid;

            // ---------- HOVER ANIMATION ----------
            cardBorder.MouseEnter += (s, e) =>
            {
                var tg = (TransformGroup)cardBorder.RenderTransform;
                var scale = (ScaleTransform)tg.Children.OfType<ScaleTransform>().First();

                var scaleAnim = new DoubleAnimation(1.0002, TimeSpan.FromMilliseconds(350))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

                var bgAnim = new ColorAnimation(Color.FromRgb(255, 255, 255), TimeSpan.FromMilliseconds(300))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
                cardBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

                var borderAnim = new ColorAnimation(Color.FromRgb(155, 155, 155), TimeSpan.FromMilliseconds(300))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
                cardBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

                buttonsStack.Visibility = Visibility.Visible;

                if (cardBorder.Effect is DropShadowEffect shadow)
                {
                    shadow.BlurRadius = 20;
                    shadow.ShadowDepth = 4;
                    shadow.Opacity = 0.25;
                }
            };

            cardBorder.MouseLeave += (s, e) =>
            {
                var tg = (TransformGroup)cardBorder.RenderTransform;
                var scale = (ScaleTransform)tg.Children.OfType<ScaleTransform>().First();

                var scaleAnim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(350))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

                var bgAnim = new ColorAnimation(Color.FromRgb(250, 251, 252), TimeSpan.FromMilliseconds(300))
                { EasingFunction = new QuadraticEase() };
                cardBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

                var borderAnim = new ColorAnimation(Color.FromRgb(226, 232, 240), TimeSpan.FromMilliseconds(300))
                { EasingFunction = new QuadraticEase() };
                cardBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);

                buttonsStack.Visibility = Visibility.Collapsed;

                if (cardBorder.Effect is DropShadowEffect shadow)
                {
                    shadow.BlurRadius = 12;
                    shadow.ShadowDepth = 2;
                    shadow.Opacity = 0.15;
                }
            };

            // ---------- LOAD ANIMATION ----------
            cardBorder.Loaded += (s, e) =>
            {
                var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } };
                cardBorder.BeginAnimation(OpacityProperty, opacityAnim);

                var tg = (TransformGroup)cardBorder.RenderTransform;
                var tt = (TranslateTransform)tg.Children.OfType<TranslateTransform>().First();
                var slideAnim = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(100))
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } };
                tt.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            };

            return cardBorder;
        }

        // ================= HELPERS =================
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

        private void UpdateCardWidths()
        {
            var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
            if (wrapPanel == null) return;

            double scrollbarWidth =
                CardScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible
                ? SystemParameters.VerticalScrollBarWidth : 0;

            double availableWidth =
                CardScrollViewer.ActualWidth
                - wrapPanel.Margin.Left
                - wrapPanel.Margin.Right
                - scrollbarWidth;

            int count = wrapPanel.Children.Count;

            foreach (Border card in wrapPanel.Children)
            {
                if (count == 1)
                    card.Width = availableWidth;
                else if (count == 2)
                    card.Width = (availableWidth / 2) - 16;
                else
                    card.Width = availableWidth > 820
                        ? (availableWidth / 2) - 16
                        : availableWidth - 16;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility =
                string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadProductsAsync();
            UpdateCardWidths();
        }

        private async void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmAddEditProduct();
            frm.Owner = Application.Current.MainWindow;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
            if (frm.IsSaved)
                await LoadProductsAsync();
        }

        // ================= BUTTON EVENTS =================
        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            int productId = (int)((Button)sender).Tag;
            var frm = new frmAddEditProduct(productId);
            frm.Owner = Application.Current.MainWindow;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
            if (frm.IsSaved)
                await LoadProductsAsync();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int productId = (int)((Button)sender).Tag;

            if (MessageBox.Show("Delete this product?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                clsProduct.Delete(productId);
                await LoadProductsAsync();
            }
        }
    }
}
