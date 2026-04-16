using POS_BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            Dispatcher.BeginInvoke(new Action(() =>
                UpdateGridLayout(ActualWidth)),
                System.Windows.Threading.DispatcherPriority.Loaded);

            _ = LoadAllAsync();
        }

        // Called by MainWindow when navigating back to dashboard
        public void Refresh()
        {
            _ = LoadAllAsync();
        }

        private async Task LoadAllAsync()
        {
            // ── Fetch all data on a background thread ─────────────────────────
            int products = 0, sales = 0, low = 0;
            decimal rev = 0;
            DataTable chartDt = null, lowStockDt = null, recentDt = null;

            await Task.Run(() =>
            {
                products = clsDashboard.GetTotalProducts();
                sales = clsDashboard.GetTotalSales();
                rev = clsDashboard.GetRevenue();
                low = clsDashboard.GetLowStock();
                chartDt = clsDashboard.GetSalesChart();
                lowStockDt = clsDashboard.GetLowStockItems(10);
                recentDt = clsDashboard.GetRecentSales(10);
            });

            // ── Back on UI thread — just update controls ───────────────────────
            ApplyStats(products, sales, rev, low);
            ApplyChart(chartDt);
            ApplyLowStock(lowStockDt);
            ApplyRecentSales(recentDt);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateGridLayout(e.NewSize.Width);

        private void UpdateGridLayout(double width)
        {
            int columns = width < 500 ? 1 : width < 850 ? 2 : width < 1150 ? 3 : 4;
            if (StatsGrid.Columns != columns)
                StatsGrid.Columns = columns;
            UpdateCardMargins(columns);
        }

        private void UpdateCardMargins(int columns)
        {
            var cards = StatsGrid.Children.OfType<Border>().ToList();
            int total = cards.Count;
            int gap = 8;

            for (int i = 0; i < total; i++)
            {
                int col = i % columns;
                int row = i / columns;
                int totalRows = (int)Math.Ceiling((double)total / columns);

                cards[i].Margin = new Thickness(
                    col == 0 ? 0 : gap / 2.0,
                    row == 0 ? 0 : gap / 2.0,
                    col == columns - 1 ? 0 : gap / 2.0,
                    row == totalRows - 1 ? 0 : gap / 2.0
                );
            }
        }

        // ── Apply methods (all run on UI thread, no DB calls) ─────────────────

        private void ApplyStats(int products, int sales, decimal rev, int low)
        {
            txtProducts.Text = products.ToString("N0");
            txtProductsSub.Text = "In inventory";
            txtSales.Text = sales.ToString("N0");
            txtSalesSub.Text = "Transactions";
            txtRevenue.Text = "$" + rev.ToString("N2");
            txtRevenueSub.Text = "Lifetime earnings";
            txtLowStock.Text = low.ToString("N0");
        }

        private void ApplyChart(DataTable dt)
        {
            ChartCanvas.Children.Clear();
            if (dt == null) return;

            var rows = new List<DataRow>();
            foreach (DataRow r in dt.Rows) rows.Insert(0, r);
            if (rows.Count == 0) return;

            double canvasH = 180, leftPad = 48, topPad = 10;
            double chartH = canvasH - topPad - 20;
            double maxValue = 1;

            foreach (var r in rows)
            {
                double v = r["Total"] == DBNull.Value ? 0 : Convert.ToDouble(r["Total"]);
                if (v > maxValue) maxValue = v;
            }

            // Gridlines
            for (int g = 0; g <= 3; g++)
            {
                double y = topPad + chartH - (chartH * g / 3.0);
                ChartCanvas.Children.Add(new Line
                {
                    X1 = leftPad - 4,
                    X2 = 2000,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(235, 237, 243)),
                    StrokeThickness = 1
                });

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

            double barWidth = 32, barGap = 14, startX = leftPad, grandTotal = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                double value = rows[i]["Total"] == DBNull.Value ? 0 : Convert.ToDouble(rows[i]["Total"]);
                grandTotal += value;
                double barH = chartH * (value / maxValue);
                double x = startX + i * (barWidth + barGap);
                double y = topPad + chartH - barH;

                ChartCanvas.Children.Add(new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(barH, 2),
                    RadiusX = 4,
                    RadiusY = 4,
                    Fill = value == maxValue
                        ? new SolidColorBrush(Color.FromRgb(255, 140, 66))
                        : new SolidColorBrush(Color.FromRgb(210, 224, 255))
                }.WithCanvas(x, y));

                if (barH > 18)
                {
                    ChartCanvas.Children.Add(new TextBlock
                    {
                        Text = value >= 1000 ? "$" + (value / 1000).ToString("F1") + "k" : "$" + value.ToString("F0"),
                        FontSize = 9,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = value == maxValue
                            ? new SolidColorBrush(Color.FromRgb(255, 140, 66))
                            : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                        FontFamily = new FontFamily("Segoe UI")
                    }.WithCanvas(x, y - 16));
                }

                string day = "—";
                if (rows[i]["Day"] != DBNull.Value &&
                    DateTime.TryParse(rows[i]["Day"].ToString(), out DateTime d))
                    day = d.ToString("ddd");

                ChartCanvas.Children.Add(new TextBlock
                {
                    Text = day,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(144, 153, 168)),
                    FontFamily = new FontFamily("Segoe UI"),
                    Width = barWidth,
                    TextAlignment = TextAlignment.Center
                }.WithCanvas(x, topPad + chartH + 4));
            }

            txtChartTotal.Text = "Total: $" + grandTotal.ToString("N2");
        }

        private void ApplyLowStock(DataTable dt)
        {
            LowStockPanel.Children.Clear();

            if (dt == null || dt.Rows.Count == 0)
            {
                txtNoLowStock.Visibility = Visibility.Visible;
                return;
            }
            txtNoLowStock.Visibility = Visibility.Collapsed;

            foreach (DataRow row in dt.Rows)
            {
                string name = row["ProductName"]?.ToString() ?? "—";
                int qty = row["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(row["Quantity"]);
                Color barColor = qty == 0
                    ? Color.FromRgb(229, 57, 53)
                    : Color.FromRgb(255, 152, 0);

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
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = qty == 0 ? "Out" : qty + " left",
                        FontSize = 11,
                        FontWeight = FontWeights.SemiBold,
                        FontFamily = new FontFamily("Segoe UI"),
                        Foreground = new SolidColorBrush(barColor)
                    }
                };
                Grid.SetColumn(badge, 1);

                grid.Children.Add(nameBlock);
                grid.Children.Add(badge);

                LowStockPanel.Children.Add(new Border
                {
                    Padding = new Thickness(0, 8, 0, 8),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(240, 240, 245)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = grid
                });
            }
        }

        private void ApplyRecentSales(DataTable dt)
        {
            var items = new List<RecentSaleRow>();
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string dateFormatted = row["SaleDate"]?.ToString() ?? "";
                    if (DateTime.TryParse(dateFormatted, out DateTime d))
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