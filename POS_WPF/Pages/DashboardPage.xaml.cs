using POS_BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace POS_WPF.Pages
{
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtDate.Text = DateTime.Now.ToString("dddd, MMMM dd yyyy");
            LoadStats();
            LoadChart();
            LoadLowStock();
            LoadRecentSales();

            // ✅ Initial layout pass after render
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateGridLayout(ActualWidth);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGridLayout(e.NewSize.Width);
        }

        private void UpdateGridLayout(double width)
        {
            int columns;

            if (width < 500)
                columns = 1;
            else if (width < 850)
                columns = 2;   // ✅ 2×2 grid
            else if (width < 1150)
                columns = 3;
            else
                columns = 4;

            if (StatsGrid.Columns != columns)
                StatsGrid.Columns = columns;

            // ✅ Fix card margins based on column count
            UpdateCardMargins(columns);
        }

        private void UpdateCardMargins(int columns)
        {
            // Get all direct Border children of StatsGrid
            var cards = StatsGrid.Children.OfType<Border>().ToList();
            int total = cards.Count; // 4 cards
            int gap = 8;

            for (int i = 0; i < total; i++)
            {
                int col = i % columns;
                int row = i / columns;
                int totalRows = (int)Math.Ceiling((double)total / columns);

                double left = col == 0 ? 0 : gap / 2.0;
                double right = col == columns - 1 ? 0 : gap / 2.0;
                double top = row == 0 ? 0 : gap / 2.0;
                double bottom = row == totalRows - 1 ? 0 : gap / 2.0;

                cards[i].Margin = new Thickness(left, top, right, bottom);
            }
        }

        // ──────────────────────────────────────────
        //  KPI CARDS
        // ──────────────────────────────────────────
        private void LoadStats()
        {
            int products = clsDashboard.GetTotalProducts();
            int sales = clsDashboard.GetTotalSales();
            decimal rev = clsDashboard.GetRevenue();
            int low = clsDashboard.GetLowStock();   // threshold = 10

            txtProducts.Text = products.ToString("N0");
            txtProductsSub.Text = "In inventory";

            txtSales.Text = sales.ToString("N0");
            txtSalesSub.Text = "Transactions";

            txtRevenue.Text = "$" + rev.ToString("N2");
            txtRevenueSub.Text = "Lifetime earnings";

            txtLowStock.Text = low.ToString("N0");
        }

        // ──────────────────────────────────────────
        //  BAR CHART  (Canvas-based, properly scaled)
        // ──────────────────────────────────────────
        private void LoadChart()
        {
            DataTable dt = clsDashboard.GetSalesChart();
            ChartCanvas.Children.Clear();

            // Reverse so oldest → newest (left → right)
            var rows = new List<DataRow>();
            foreach (DataRow r in dt.Rows) rows.Insert(0, r);

            if (rows.Count == 0) return;

            double canvasH = 180;
            double maxValue = 1;
            foreach (var r in rows)
            {
                double v = r["Total"] == DBNull.Value ? 0 : Convert.ToDouble(r["Total"]);
                if (v > maxValue) maxValue = v;
            }

            // Gridlines (3 horizontal)
            double leftPad = 48;
            double rightPad = 10;
            double topPad = 10;
            double chartH = canvasH - topPad - 20; // 20 for bottom labels

            for (int g = 0; g <= 3; g++)
            {
                double y = topPad + chartH - (chartH * g / 3.0);
                var line = new Line
                {
                    X1 = leftPad - 4,
                    X2 = 2000,     // will be clipped by canvas
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(235, 237, 243)),
                    StrokeThickness = 1
                };
                ChartCanvas.Children.Add(line);

                // Y label
                double labelVal = maxValue * g / 3.0;
                var yLabel = new TextBlock
                {
                    Text = labelVal >= 1000
                                     ? (labelVal / 1000).ToString("F1") + "k"
                                     : labelVal.ToString("F0"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(144, 153, 168)),
                    FontFamily = new FontFamily("Segoe UI")
                };
                Canvas.SetLeft(yLabel, 0);
                Canvas.SetTop(yLabel, y - 8);
                ChartCanvas.Children.Add(yLabel);
            }

            // Width calculation (deferred — use ActualWidth, or use a fixed bar width)
            double barWidth = 32;
            double barGap = 14;
            double totalW = rows.Count * (barWidth + barGap);
            double startX = leftPad;

            double grandTotal = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                double value = rows[i]["Total"] == DBNull.Value ? 0 : Convert.ToDouble(rows[i]["Total"]);
                grandTotal += value;
                double barH = chartH * (value / maxValue);
                double x = startX + i * (barWidth + barGap);
                double y = topPad + chartH - barH;

                // Bar (rounded top via rectangle + clip via geometry — use simple rect for WPF compat)
                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(barH, 2),
                    RadiusX = 4,
                    RadiusY = 4,
                    Fill = value == maxValue
                             ? new SolidColorBrush(Color.FromRgb(255, 140, 66))   // orange accent for peak
                             : new SolidColorBrush(Color.FromRgb(210, 224, 255))  // soft blue others
                };
                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, y);
                ChartCanvas.Children.Add(bar);

                // Value label above bar
                if (barH > 18)
                {
                    var valLabel = new TextBlock
                    {
                        Text = value >= 1000
                                         ? "$" + (value / 1000).ToString("F1") + "k"
                                         : "$" + value.ToString("F0"),
                        FontSize = 9,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = value == maxValue
                                         ? new SolidColorBrush(Color.FromRgb(255, 140, 66))
                                         : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                        FontFamily = new FontFamily("Segoe UI")
                    };
                    Canvas.SetLeft(valLabel, x);
                    Canvas.SetTop(valLabel, y - 16);
                    ChartCanvas.Children.Add(valLabel);
                }

                // Day label below bar  (Mon, Tue …)
                string day = "—";
                if (rows[i]["Day"] != DBNull.Value)
                {
                    if (DateTime.TryParse(rows[i]["Day"].ToString(), out DateTime d))
                        day = d.ToString("ddd");
                }
                var dayLabel = new TextBlock
                {
                    Text = day,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(144, 153, 168)),
                    FontFamily = new FontFamily("Segoe UI"),
                    Width = barWidth,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(dayLabel, x);
                Canvas.SetTop(dayLabel, topPad + chartH + 4);
                ChartCanvas.Children.Add(dayLabel);
            }

            txtChartTotal.Text = "Total: $" + grandTotal.ToString("N2");
        }

        // ──────────────────────────────────────────
        //  LOW STOCK PANEL
        // ──────────────────────────────────────────
        private void LoadLowStock()
        {
            DataTable dt = clsDashboard.GetLowStockItems(10);  // see BLL addition below
            LowStockPanel.Children.Clear();

            if (dt == null || dt.Rows.Count == 0)
            {
                txtNoLowStock.Visibility = Visibility.Visible;
                return;
            }

            foreach (DataRow row in dt.Rows)
            {
                string name = row["ProductName"]?.ToString() ?? "—";
                int qty = row["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(row["Quantity"]);

                // Severity colour
                Color barColor = qty == 0
                    ? Color.FromRgb(229, 57, 53)   // out-of-stock  red
                    : Color.FromRgb(255, 152, 0);  // low           orange

                var item = new Border
                {
                    Padding = new Thickness(0, 8, 0, 8),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(240, 240, 245)),
                    BorderThickness = new Thickness(0, 0, 0, 1)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var nameBlock = new TextBlock
                {
                    Text = name,
                    FontSize = 12,
                    FontFamily = new FontFamily("Segoe UI"),
                    Foreground = new SolidColorBrush(Color.FromRgb(45, 49, 66)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(nameBlock, 0);

                var badge = new Border
                {
                    Background = new SolidColorBrush(barColor) { Opacity = 0.15 },
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 2, 8, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var qtyBlock = new TextBlock
                {
                    Text = qty == 0 ? "Out" : qty.ToString() + " left",
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    FontFamily = new FontFamily("Segoe UI"),
                    Foreground = new SolidColorBrush(barColor)
                };
                badge.Child = qtyBlock;
                Grid.SetColumn(badge, 1);

                grid.Children.Add(nameBlock);
                grid.Children.Add(badge);
                item.Child = grid;
                LowStockPanel.Children.Add(item);
            }
        }

        // ──────────────────────────────────────────
        //  RECENT SALES TABLE
        // ──────────────────────────────────────────
        private void LoadRecentSales()
        {
            DataTable dt = clsDashboard.GetRecentSales(10);  // see BLL addition below
            var items = new List<RecentSaleRow>();

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string dateRaw = row["SaleDate"]?.ToString() ?? "";
                    string dateFormatted = dateRaw;
                    if (DateTime.TryParse(dateRaw, out DateTime d))
                        dateFormatted = d.ToString("MMM dd, yyyy");

                    items.Add(new RecentSaleRow
                    {
                        SaleID = "#" + row["SaleID"],
                        Client = row["Client"]?.ToString() ?? "—",
                        SaleDate = dateFormatted,
                        Total = "$" + Convert.ToDecimal(row["TotalPrice"]).ToString("N2")
                    });
                }
            }

            RecentSalesList.ItemsSource = items;
            txtNoSales.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private class RecentSaleRow
        {
            public string SaleID { get; set; }
            public string Client { get; set; }
            public string SaleDate { get; set; }
            public string Total { get; set; }
        }
    }
}