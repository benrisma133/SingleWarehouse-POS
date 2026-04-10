using POS_BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS_WPF.Popup
{
    /// <summary>
    /// Interaction logic for ProductNotificationPopup.xaml
    /// </summary>
    public partial class ProductNotificationPopup : Window
    {
        public ProductNotificationPopup(StockNotification notif, clsProduct product)
        {
            InitializeComponent();
            Populate(notif, product);
        }

        private void Populate(StockNotification notif, clsProduct product)
        {
            // ── Header ──────────────────────────────────
            TxtProductName.Text = notif.ProductName;
            TxtTagLabel.Text = notif.TagLabel;
            HeaderIcon.Text = notif.Type == 1 ? "🚫" : "⚠️";

            // ── Status badge ────────────────────────────
            StatusBadge.Background = notif.TagBackground;
            TxtSubText.Foreground = notif.TagForeground;
            TxtSubText.Text = notif.SubText;

            var dotColor = notif.DotColor as SolidColorBrush;
            BadgeDot.Fill = dotColor ?? new SolidColorBrush(Colors.Gray);

            // ── Info rows ───────────────────────────────
            if (product != null)
            {
                TxtCategory.Text = product.CategoryID.ToString();   // swap with name if you have it
                TxtPrice.Text = product.Price.ToString("C2");
                TxtQuantity.Text = product.Quantity == 0
                                        ? "Out of stock"
                                        : $"{product.Quantity} units";
                TxtDescription.Text = string.IsNullOrWhiteSpace(product.Description)
                                        ? "No description available."
                                        : product.Description;
            }
            else
            {
                TxtCategory.Text = "—";
                TxtPrice.Text = "—";
                TxtQuantity.Text = "—";
                TxtDescription.Text = "Product details could not be loaded.";
            }

            TxtDate.Text = notif.SmartDate;

            // ── Footer note ─────────────────────────────
            TxtFooterNote.Text = notif.IsRead
                ? "Already marked as read."
                : "Will be marked as read.";
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        private void Overlay_Click(object sender, MouseButtonEventArgs e) => Close();

    }
}
