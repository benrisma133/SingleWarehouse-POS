using POS_BLL;
using POS_WPF.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace POS_WPF.Pages
{
    public partial class SalesPage : UserControl
    {
        // ── Cart state ────────────────────────────────────────────────────────────
        private readonly List<clsCartItem> _cart = new List<clsCartItem>();

        // ── Constructor ───────────────────────────────────────────────────────────
        public SalesPage()
        {
            InitializeComponent();
        }

        // ── Loaded ────────────────────────────────────────────────────────────────
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClients();
            await LoadProductsAsync();
        }

        // ======================= CLIENTS =======================

        private void LoadClients()
        {
            DataTable dt = clsSale.GetClients();
            cmbClients.ItemsSource = dt.DefaultView;
            cmbClients.DisplayMemberPath = "FullName";
            cmbClients.SelectedValuePath = "ClientID";
        }

        private void cmbClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbClients.SelectedItem is DataRowView row)
            {
                string phone = row["Phone"]?.ToString();
                txtClientInfo.Text = string.IsNullOrWhiteSpace(phone)
                    ? row["FullName"].ToString()
                    : $"{row["FullName"]}  ·  {phone}";
                ClientInfoPill.Visibility = Visibility.Visible;
            }
            else
            {
                ClientInfoPill.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnNewClient_Click(object sender, RoutedEventArgs e)
        {
            var win = new frmAddEditClient();
            win.Owner = Application.Current.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();

            if (win.IsSaved)
            {
                LoadClients();
                // Auto-select the last client (just added)
                if (cmbClients.Items.Count > 0)
                    cmbClients.SelectedIndex = cmbClients.Items.Count - 1;
            }
        }

        // ======================= PRODUCTS =======================

        private async Task LoadProductsAsync(string filter = "")
        {
            ProductListContainer.Children.Clear();

            DataTable dt = await Task.Run(() => clsSale.GetAvailableProducts(filter));

            if (dt.Rows.Count == 0)
            {
                NoProductsMessage.Visibility = Visibility.Visible;
                return;
            }

            NoProductsMessage.Visibility = Visibility.Collapsed;

            foreach (DataRow row in dt.Rows)
            {
                int pid = Convert.ToInt32(row["ProductID"]);
                string name = row["ProductName"].ToString();
                decimal price = Convert.ToDecimal(row["Price"]);
                int stock = Convert.ToInt32(row["Quantity"]);

                ProductListContainer.Children.Add(
                    CreateProductCard(pid, name, price, stock));
            }
        }

        private Border CreateProductCard(int productID, string name, decimal price, int stock)
        {
            // Check if already in cart
            bool inCart = _cart.Any(c => c.ProductID == productID);

            Border card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 251, 252)),
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 12, 14, 12),
                Margin = new Thickness(0, 0, 0, 10),
                Tag = productID
            };

            card.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(148, 163, 184),
                BlurRadius = 6,
                ShadowDepth = 1,
                Opacity = 0.10
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left: name + price + stock badge
            StackPanel left = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            left.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            });

            StackPanel priceRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 0)
            };

            priceRow.Children.Add(new TextBlock
            {
                Text = $"${price:F2}",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 66))
            });

            // Stock badge
            Border stockBadge = new Border
            {
                Background = stock > 10
                    ? new SolidColorBrush(Color.FromRgb(209, 250, 229))
                    : new SolidColorBrush(Color.FromRgb(254, 243, 199)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(7, 2, 7, 2),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            stockBadge.Child = new TextBlock
            {
                Text = $"In stock: {stock}",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = stock > 10
                    ? new SolidColorBrush(Color.FromRgb(6, 95, 70))
                    : new SolidColorBrush(Color.FromRgb(146, 64, 14))
            };
            priceRow.Children.Add(stockBadge);
            left.Children.Add(priceRow);
            grid.Children.Add(left);

            // Right: Add button
            Button addBtn = new Button
            {
                Content = inCart ? "✓ In Cart" : "+ Add",
                Style = (Style)FindResource("AddToCartBtn"),
                Width = 90,
                IsEnabled = !inCart,
                Background = inCart
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                    : new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                Tag = productID,
                VerticalAlignment = VerticalAlignment.Center
            };
            addBtn.Click += AddToCart_Click;
            Grid.SetColumn(addBtn, 1);
            grid.Children.Add(addBtn);

            card.Child = grid;

            // Hover
            card.MouseEnter += (s, e) =>
            {
                if (card.Effect is DropShadowEffect sh)
                    sh.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                        new DoubleAnimation(12, TimeSpan.FromMilliseconds(200)));
            };
            card.MouseLeave += (s, e) =>
            {
                if (card.Effect is DropShadowEffect sh)
                    sh.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                        new DoubleAnimation(6, TimeSpan.FromMilliseconds(200)));
            };

            return card;
        }

        private async void txtProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtProductPlaceholder.Visibility = string.IsNullOrEmpty(txtProductSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            await LoadProductsAsync(txtProductSearch.Text.Trim());
        }

        // ======================= CART =======================

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            int productID = (int)btn.Tag;

            // Get product info from card
            var card = FindParentOfType<Border>(btn);
            if (card == null) return;

            // Find from the product list by Tag
            var cardBorder = ProductListContainer.Children
                .OfType<Border>()
                .FirstOrDefault(b => (int)b.Tag == productID);
            if (cardBorder == null) return;

            // Read name and price from card visual tree
            var nameBlock = FindVisualChild<TextBlock>(cardBorder, 0);
            var priceBlock = FindVisualChild<TextBlock>(cardBorder, 1);

            if (nameBlock == null || priceBlock == null) return;

            string name = nameBlock.Text;
            decimal price = decimal.Parse(priceBlock.Text.TrimStart('$'));
            int stock = clsSale.GetProductStock(productID);

            // Already in cart? Just bump quantity
            var existing = _cart.FirstOrDefault(c => c.ProductID == productID);
            if (existing != null)
            {
                if (existing.Quantity < existing.MaxStock)
                {
                    existing.Quantity++;
                    RefreshCart();
                }
                return;
            }

            _cart.Add(new clsCartItem
            {
                ProductID = productID,
                ProductName = name,
                UnitPrice = price,
                Quantity = 1,
                MaxStock = stock
            });

            RefreshCart();
            RefreshProductList();
        }

        private void RefreshCart()
        {
            CartItemsContainer.Children.Clear();

            if (_cart.Count == 0)
            {
                EmptyCartMessage.Visibility = Visibility.Visible;
                txtCartCount.Text = "0";
                txtTotal.Text = "$0.00";
                return;
            }

            EmptyCartMessage.Visibility = Visibility.Collapsed;
            txtCartCount.Text = _cart.Count.ToString();

            foreach (var item in _cart)
                CartItemsContainer.Children.Add(CreateCartRow(item));

            decimal total = _cart.Sum(c => c.Total);
            txtTotal.Text = $"${total:F2}";
        }

        private Border CreateCartRow(clsCartItem item)
        {
            Border row = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(238, 240, 244)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(4, 10, 4, 10)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Name + unit price
            StackPanel info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock
            {
                Text = item.ProductName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                TextTrimming = TextTrimming.CharacterEllipsis
            });
            info.Children.Add(new TextBlock
            {
                Text = $"${item.UnitPrice:F2} each",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            grid.Children.Add(info);

            // Qty controls + line total
            StackPanel qtyArea = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 0)
            };
            Grid.SetColumn(qtyArea, 1);

            Button minusBtn = new Button { Content = "−", Style = (Style)FindResource("QtyBtn"), Tag = item };
            minusBtn.Click += QtyMinus_Click;

            TextBlock qtyText = new TextBlock
            {
                Text = item.Quantity.ToString(),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 0),
                MinWidth = 20,
                TextAlignment = TextAlignment.Center
            };

            Button plusBtn = new Button { Content = "+", Style = (Style)FindResource("QtyBtn"), Tag = item };
            plusBtn.IsEnabled = item.Quantity < item.MaxStock;
            plusBtn.Click += QtyPlus_Click;

            qtyArea.Children.Add(minusBtn);
            qtyArea.Children.Add(qtyText);
            qtyArea.Children.Add(plusBtn);

            // Line total
            TextBlock lineTotal = new TextBlock
            {
                Text = $"${item.Total:F2}",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                MinWidth = 58,
                TextAlignment = TextAlignment.Right
            };
            qtyArea.Children.Add(lineTotal);
            grid.Children.Add(qtyArea);

            // Remove button
            Button removeBtn = new Button
            {
                Style = (Style)FindResource("RemoveBtn"),
                Content = "✕",
                Tag = item,
                VerticalAlignment = VerticalAlignment.Center
            };
            removeBtn.Click += RemoveCartItem_Click;
            Grid.SetColumn(removeBtn, 2);
            grid.Children.Add(removeBtn);

            row.Child = grid;
            return row;
        }

        private void QtyMinus_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag is clsCartItem item)) return;

            if (item.Quantity > 1)
                item.Quantity--;
            else
                _cart.Remove(item);

            RefreshCart();
            RefreshProductList();
        }

        private void QtyPlus_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag is clsCartItem item)) return;

            if (item.Quantity < item.MaxStock)
                item.Quantity++;

            RefreshCart();
        }

        private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag is clsCartItem item)) return;
            _cart.Remove(item);
            RefreshCart();
            RefreshProductList();
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) return;

            var confirm = MessageBox.Show(
                "Clear all items from the cart?",
                "Clear Cart",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                _cart.Clear();
                RefreshCart();
                RefreshProductList();
            }
        }

        // Refresh add buttons (in/out of cart state)
        private async void RefreshProductList()
        {
            await LoadProductsAsync(txtProductSearch.Text.Trim());
        }

        // ======================= CONFIRM SALE =======================

        private async void BtnConfirmSale_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (cmbClients.SelectedValue == null)
            {
                MessageBox.Show("Please select a client before confirming the sale.",
                    "No Client", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_cart.Count == 0)
            {
                MessageBox.Show("The cart is empty. Add at least one product.",
                    "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal total = _cart.Sum(c => c.Total);
            var confirm = MessageBox.Show(
                $"Confirm sale of {_cart.Count} item(s) for ${total:F2}?",
                "Confirm Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var sale = new clsSale
            {
                ClientID = Convert.ToInt32(cmbClients.SelectedValue),
                Items = new List<clsCartItem>(_cart)
            };

            bool saved = false;
            string errorMsg = null;

            try
            {
                saved = await Task.Run(() => sale.Save());
            }
            catch (InvalidOperationException ex)
            {
                errorMsg = ex.Message;
            }
            catch (Exception ex)
            {
                errorMsg = "Unexpected error: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg, "Stock Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (saved)
            {
                MessageBox.Show(
                    $"Sale #{sale.SaleID} confirmed successfully!\nTotal: ${total:F2}",
                    "Sale Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reset page
                _cart.Clear();
                cmbClients.SelectedIndex = -1;
                ClientInfoPill.Visibility = Visibility.Collapsed;
                RefreshCart();
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show("Failed to save the sale. Please try again.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelSale_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0 && cmbClients.SelectedIndex == -1) return;

            var confirm = MessageBox.Show(
                "Cancel this sale? All cart items will be cleared.",
                "Cancel Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                _cart.Clear();
                cmbClients.SelectedIndex = -1;
                ClientInfoPill.Visibility = Visibility.Collapsed;
                RefreshCart();
                _ = LoadProductsAsync();
            }
        }

        // ======================= VISUAL HELPERS =======================

        private static T FindParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T t) return t;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        // Finds the Nth TextBlock in visual tree (used to read card data)
        private static TextBlock FindVisualChild<TextBlock>(
            DependencyObject parent, int index) where TextBlock : DependencyObject
        {
            int found = 0;
            return FindVisualChildByIndex<TextBlock>(parent, ref found, index);
        }

        private static T FindVisualChildByIndex<T>(
            DependencyObject parent, ref int found, int target) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    if (found == target) return t;
                    found++;
                }
                var result = FindVisualChildByIndex<T>(child, ref found, target);
                if (result != null) return result;
            }
            return null;
        }
    }
}