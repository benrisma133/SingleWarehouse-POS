using POS_BLL;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace POS_WPF.Pages
{
    public partial class SalesDetailsPage : UserControl
    {
        public SalesDetailsPage()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
            => await LoadSalesAsync();

        // ======================= LOAD =======================

        private async Task LoadSalesAsync(string filter = "")
        {
            SaleCardsContainer.Children.Clear();

            DataTable dt = await Task.Run(() => clsSaleDetails.GetAllSales(filter));

            if (dt.Rows.Count == 0)
            {
                NoSalesMessage.Visibility = Visibility.Visible;
                txtTotalSales.Text = "Total Sales : 0";
                return;
            }

            NoSalesMessage.Visibility = Visibility.Collapsed;
            txtTotalSales.Text = $"Total Sales : {dt.Rows.Count}";

            foreach (DataRow row in dt.Rows)
            {
                int saleID = Convert.ToInt32(row["SaleID"]);
                string date = Convert.ToDateTime(row["SaleDate"]).ToString("dd MMM yyyy  HH:mm");
                string client = row["ClientName"].ToString();
                string phone = row["ClientPhone"]?.ToString() ?? "";
                string email = row["ClientEmail"]?.ToString() ?? "";
                decimal total = Convert.ToDecimal(row["TotalPrice"]);
                int items = Convert.ToInt32(row["ItemCount"]);

                SaleCardsContainer.Children.Add(
                    CreateSaleCard(saleID, date, client, phone, email, total, items));
            }
        }

        // ======================= CARD FACTORY =======================

        private Border CreateSaleCard(int saleID, string date, string client,
                                      string phone, string email,
                                      decimal total, int itemCount)
        {
            // ── Outer card ──
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 251, 252)),
                CornerRadius = new CornerRadius(14),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                Tag = saleID,
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 18)
            };

            card.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(148, 163, 184),
                BlurRadius = 8,
                ShadowDepth = 1,
                Opacity = 0.10
            };

            // ── Root stack (header row + collapsible detail) ──
            var root = new StackPanel();

            // ── HEADER ROW ──
            var header = new Grid { Margin = new Thickness(16, 14, 16, 14) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Left: Sale# badge + client info
            var leftStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Sale ID badge
            var saleBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 14, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            saleBadge.Child = new TextBlock
            {
                Text = $"# {saleID}",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            leftStack.Children.Add(saleBadge);

            // Client + date stack
            var metaStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            metaStack.Children.Add(new TextBlock
            {
                Text = client,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            });

            var subLine = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 3, 0, 0)
            };

            subLine.Children.Add(new TextBlock
            {
                Text = date,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
            });

            if (!string.IsNullOrWhiteSpace(phone))
            {
                subLine.Children.Add(new TextBlock
                {
                    Text = $"  ·  📞 {phone}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
                });
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                subLine.Children.Add(new TextBlock
                {
                    Text = $"  ·  ✉ {email}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
                });
            }

            metaStack.Children.Add(subLine);
            leftStack.Children.Add(metaStack);
            header.Children.Add(leftStack);

            // Right: total + item count + expand + delete
            var rightStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(rightStack, 1);

            // Item count pill
            var itemPill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            itemPill.Child = new TextBlock
            {
                Text = $"{itemCount} item{(itemCount != 1 ? "s" : "")}",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105))
            };
            rightStack.Children.Add(itemPill);

            // Total
            rightStack.Children.Add(new TextBlock
            {
                Text = $"${total:F2}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0)
            });

            // Collapsible detail panel (built lazily)
            Border detailPanel = BuildDetailPanelPlaceholder(saleID);
            bool expanded = false;

            // Expand chevron button
            var chevronBtn = new Button
            {
                Style = (Style)FindResource("ChevronBtn"),
                Content = "▼",
                Margin = new Thickness(0, 0, 8, 0)
            };
            chevronBtn.Click += async (s, e) =>
            {
                expanded = !expanded;
                chevronBtn.Content = expanded ? "▲" : "▼";

                if (expanded)
                {
                    // Lazy-load items
                    var items = await Task.Run(() => clsSaleDetails.GetSaleItems(saleID));
                    FillDetailPanel(detailPanel, items);
                    detailPanel.Visibility = Visibility.Visible;
                    detailPanel.BeginAnimation(OpacityProperty,
                        new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
                }
                else
                {
                    var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
                    anim.Completed += (_, __) => detailPanel.Visibility = Visibility.Collapsed;
                    detailPanel.BeginAnimation(OpacityProperty, anim);
                }
            };
            rightStack.Children.Add(chevronBtn);

            // Delete button
            var deleteBtn = new Button
            {
                Style = (Style)FindResource("DeleteBtn"),
                Content = "🗑",
                Tag = saleID
            };
            deleteBtn.Click += async (s, e) =>
            {
                var confirm = MessageBox.Show(
                    $"Permanently delete Sale #{saleID}?\nThis cannot be undone.",
                    "Delete Sale", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes) return;

                bool ok = await Task.Run(() => clsSaleDetails.DeleteSale(saleID));
                if (ok)
                    await LoadSalesAsync(txtSearch.Text.Trim());
                else
                    MessageBox.Show("Could not delete the sale.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            rightStack.Children.Add(deleteBtn);

            header.Children.Add(rightStack);
            root.Children.Add(header);
            root.Children.Add(detailPanel);

            card.Child = root;

            // ── Hover ──
            card.MouseEnter += (s, e) =>
            {
                if (card.Effect is DropShadowEffect sh)
                    sh.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                        new DoubleAnimation(16, TimeSpan.FromMilliseconds(200)));
            };
            card.MouseLeave += (s, e) =>
            {
                if (card.Effect is DropShadowEffect sh)
                    sh.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                        new DoubleAnimation(8, TimeSpan.FromMilliseconds(200)));
            };

            // ── Entrance animation ──
            card.Loaded += (s, e) =>
            {
                card.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(1, TimeSpan.FromMilliseconds(350)));
                if (card.RenderTransform is TranslateTransform t)
                    t.BeginAnimation(TranslateTransform.YProperty,
                        new DoubleAnimation(0, TimeSpan.FromMilliseconds(350))
                        { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
            };

            return card;
        }

        // ── Detail panel placeholder (collapsed until user expands) ──
        private Border BuildDetailPanelPlaceholder(int saleID)
        {
            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(238, 240, 244)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                Padding = new Thickness(16, 10, 16, 14),
                Visibility = Visibility.Collapsed,
                Tag = saleID
            };
        }

        // ── Fill detail panel with product rows ──
        private void FillDetailPanel(Border panel, DataTable items)
        {
            var stack = new StackPanel();

            // Column header
            stack.Children.Add(BuildDetailHeader());

            foreach (DataRow row in items.Rows)
            {
                string product = row["ProductName"].ToString();
                string brand = row["BrandName"].ToString();
                string series = row["SeriesName"].ToString();
                string model = row["ModelName"].ToString();
                string category = row["CategoryName"].ToString();
                int qty = Convert.ToInt32(row["Quantity"]);
                decimal unit = Convert.ToDecimal(row["UnitPrice"]);
                decimal line = Convert.ToDecimal(row["LineTotal"]);
                string desc = row["ProductDescription"]?.ToString() ?? "";

                stack.Children.Add(BuildDetailRow(product, brand, series, model,
                                                   category, qty, unit, line, desc));
            }

            panel.Child = stack;
        }

        private Grid BuildDetailHeader()
        {
            var g = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            AddHeaderCell(g, "Product", 0, TextAlignment.Left);
            AddHeaderCell(g, "Brand / Series / Model", 1, TextAlignment.Left);
            AddHeaderCell(g, "Category", 2, TextAlignment.Left);
            AddHeaderCell(g, "Qty", 3, TextAlignment.Center, 50);
            AddHeaderCell(g, "Unit", 4, TextAlignment.Right, 80);
            AddHeaderCell(g, "Total", 5, TextAlignment.Right, 90);

            return g;
        }

        private void AddHeaderCell(Grid g, string text, int col,
                                   TextAlignment align, double minW = 0)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextAlignment = align,
                Margin = new Thickness(col == 0 ? 0 : 8, 0, 0, 0)
            };
            if (minW > 0) tb.MinWidth = minW;
            Grid.SetColumn(tb, col);
            g.Children.Add(tb);
        }

        private Border BuildDetailRow(string product, string brand, string series,
                                      string model, string category,
                                      int qty, decimal unit, decimal line, string desc)
        {
            var row = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(238, 240, 244)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 8, 0, 8)
            };

            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Product name + optional description
            var nameStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            nameStack.Children.Add(new TextBlock
            {
                Text = product,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                TextTrimming = TextTrimming.CharacterEllipsis
            });
            if (!string.IsNullOrWhiteSpace(desc))
                nameStack.Children.Add(new TextBlock
                {
                    Text = desc,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            Grid.SetColumn(nameStack, 0);
            g.Children.Add(nameStack);

            // Brand / Series / Model
            var bsmStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            bsmStack.Children.Add(new TextBlock
            {
                Text = brand,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            });
            bsmStack.Children.Add(new TextBlock
            {
                Text = $"{series}  ›  {model}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(bsmStack, 1);
            g.Children.Add(bsmStack);

            // Category badge
            var catBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            catBadge.Child = new TextBlock
            {
                Text = category,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161))
            };
            Grid.SetColumn(catBadge, 2);
            g.Children.Add(catBadge);

            // Qty
            var qtyTb = new TextBlock
            {
                Text = qty.ToString(),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                TextAlignment = TextAlignment.Center,
                MinWidth = 50,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(qtyTb, 3);
            g.Children.Add(qtyTb);

            // Unit price
            var unitTb = new TextBlock
            {
                Text = $"${unit:F2}",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextAlignment = TextAlignment.Right,
                MinWidth = 80,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(unitTb, 4);
            g.Children.Add(unitTb);

            // Line total
            var lineTb = new TextBlock
            {
                Text = $"${line:F2}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 66)),
                TextAlignment = TextAlignment.Right,
                MinWidth = 90,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lineTb, 5);
            g.Children.Add(lineTb);

            row.Child = g;
            return row;
        }

        // ======================= EVENTS =======================

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            await LoadSalesAsync(txtSearch.Text.Trim());
        }
    }
}