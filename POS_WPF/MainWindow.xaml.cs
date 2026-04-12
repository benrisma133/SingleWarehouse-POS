using POS_BLL;
using POS_WPF.Popup;
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



namespace POS_WPF
{

    public class StockNotification
    {
        public int NotificationID { get; set; }   // DB link
        public int ProductID { get; set; }        // useful
        public int Type { get; set; }             // 1 = OUT, 2 = LOW

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public string ProductName { get; set; }
        public string SubText { get; set; }
        public string TagLabel { get; set; }
        public Brush DotColor { get; set; }
        public Brush TagBackground { get; set; }
        public Brush TagForeground { get; set; }

        public string SmartDate
        {
            get
            {
                var span = DateTime.Now - CreatedAt;

                if (span.TotalSeconds < 60)
                    return "Just now";

                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes} min ago";

                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours} hr ago";

                if (span.TotalDays < 2)
                    return "Yesterday";

                if (span.TotalDays < 7)
                    return $"{(int)span.TotalDays} days ago";

                return CreatedAt.ToString("dd MMM yyyy");
            }
        }

        public static StockNotification OutOfStock(string product)
            => new StockNotification
            {
                ProductName = product,
                SubText = $"0 units remaining",
                TagLabel = "Out of stock",
                Type = 1,
                DotColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E24B4A")),
                TagBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCEBEB")),
                TagForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A32D2D")),
            };

        public static StockNotification LowStock(string product, int qty)
            => new StockNotification
            {
                ProductName = product,
                SubText = $"{qty} units remaining",
                TagLabel = "Low stock",
                Type = 2,
                DotColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF9F27")),
                TagBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAEEDA")),
                TagForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#854F0B")),
            };
    }

    /// <summary>
    /// Interaction logic for TestSidebarWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        public MainWindow()
        {
            InitializeComponent();

            AppEvents.StockChanged += OnStockChanged;
        }

        private void OnStockChanged()
        {
            LoadStockNotifications();
        }

        private void InitNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Warning;
            _notifyIcon.Visible = true;

            _notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
        }

        private void ShowNotification(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, System.Windows.Forms.ToolTipIcon.Warning);
        }

        private Geometry HamburgerIcon = Geometry.Parse("M4 6H20 M4 12H20 M4 18H20");
        private Geometry CloseIcon = Geometry.Parse("M18.3 5.71L12 12l6.3 6.29-1.41 1.42L10.59 13.4l-6.3 6.31-1.41-1.42L9.17 12 2.88 5.71l1.41-1.42 6.3 6.3 6.29-6.3z");
        private Geometry LeftArrowIcon = Geometry.Parse("M11.7071 4.29289C12.0976 4.68342 12.0976 5.31658 11.7071 5.70711L6.41421 11H20C20.5523 11 21 11.4477 21 12C21 12.5523 20.5523 13 20 13H6.41421L11.7071 18.2929C12.0976 18.6834 12.0976 19.3166 11.7071 19.7071C11.3166 20.0976 10.6834 20.0976 10.2929 19.7071L3.29289 12.7071C3.10536 12.5196 3 12.2652 3 12C3 11.7348 3.10536 11.4804 3.29289 11.2929L10.2929 4.29289C10.6834 3.90237 11.3166 3.90237 11.7071 4.29289Z");

        private FrameworkElement _activeMenu = null;


        private readonly Brush MenuNormalBg = new SolidColorBrush(Color.FromRgb(74, 74, 74));      // #4A4A4A
        private readonly Brush MenuHoverBg  = new SolidColorBrush(Color.FromRgb(92, 92, 92));      // #5C5C5C
        private readonly Brush MenuActiveBg = new SolidColorBrush(Color.FromRgb(92, 92, 92));      // SAME AS HOVER
        private readonly Brush MenuActiveFg = new SolidColorBrush(Color.FromRgb(255, 140, 66));    // #FF8C42
        private readonly Brush MenuNormalFg = new SolidColorBrush(Color.FromRgb(245, 245, 245));   // #F5F5F5

        //Pages.CategoryPage categoryPage = new CategoryPage();
        //Pages.ModelPage modelPage = new ModelPage();

        private void SetActiveMenu(FrameworkElement clickedElement)
        {
            // ── Reset profile style whenever a menu button is clicked ──
            ResetProfileStyle();

            // Reset previous active
            if (_activeMenu is Button oldBtn)
            {
                oldBtn.ClearValue(Button.TagProperty);
            }

            // Set new active
            if (clickedElement is Button newBtn)
            {
                newBtn.Tag = "Active";
                _activeMenu = newBtn;
            }
            else
            {
                _activeMenu = null;
            }
        }

        private void ResetProfileStyle()
        {
            isProfileActive = false;
            ProfileBorder.Background = MenuNormalBg;

            var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
            if (path != null) path.Fill = MenuNormalFg;

            foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                tb.Foreground = MenuNormalFg;
        }

        public Geometry HamburgerPathData
        {
            get { return isSidebarOpen ? CloseIcon : HamburgerIcon; }
        }

        private void UpdateHamburgerIcon()
        {
            if (isSidebarOpen)
            {
                // Sidebar open → show left arrow (close)
                HamburgerPath.Data = LeftArrowIcon;
                HamburgerBtn.HorizontalAlignment = HorizontalAlignment.Right;
                HamburgerBtn.Margin = new Thickness(0, 10, 6, 0);
            }
            else
            {
                // Sidebar closed → show hamburger icon
                HamburgerPath.Data = HamburgerIcon;
                HamburgerBtn.HorizontalAlignment = HorizontalAlignment.Center;
                HamburgerBtn.Margin = new Thickness(0, 10, 0, 0);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitNotifyIcon();
            UpdateHamburgerIcon();
            SetActiveMenu(DashboardButton); // give Dashboard x:Name="DashboardButton"
            LoadStockNotifications();

            MainTitle.Text = "Dashboard";
            PageContent.Content = new Pages.DashboardPage();
        }

        private bool isSidebarOpen = true;

        private bool userClosedSidebar = false; // track if user manually closed

        private bool shouldRestoreDeptExpanded = false;

        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            // Sidebar is OPEN and user is closing it
            if (isSidebarOpen)
            {
                // Remember submenu state
                shouldRestoreDeptExpanded = isDeptExpanded;

                // Collapse submenu visually if open
                if (isDeptExpanded)
                {
                    CollapseDepartmentsSubmenu();
                }
            }
            else
            {
                // Sidebar is CLOSED and user is opening it
                ToggleSidebar();

                // Restore submenu if needed
                if (shouldRestoreDeptExpanded)
                {
                    ExpandDepartmentsSubmenu();
                }

                return;
            }

            ToggleSidebar();
            userClosedSidebar = !isSidebarOpen;
        }

        private void CollapseDepartmentsSubmenu()
        {
            double height = DepartmentsSubmenu.ActualHeight;

            var animation = new DoubleAnimation
            {
                From = height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            DepartmentsSubmenuContainer.BeginAnimation(Border.HeightProperty, animation);

            CatalogArrow.Data = Geometry.Parse("M6 9L12 15L18 9"); // Down
            isDeptExpanded = false;
        }

        private void ExpandDepartmentsSubmenu()
        {
            double height = DepartmentsSubmenu.ActualHeight;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = height,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, _) =>
            {
                DepartmentsSubmenuContainer.Height = double.NaN;
            };

            DepartmentsSubmenuContainer.BeginAnimation(Border.HeightProperty, animation);

            CatalogArrow.Data = Geometry.Parse("M6 15L12 9L18 15"); // Up
            isDeptExpanded = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 1100)
            {
                HamburgerBtn.Visibility = Visibility.Collapsed;

                if (isSidebarOpen)
                    ToggleSidebar(forceCollapse: true); // force collapse
            }
            else
            {
                HamburgerBtn.Visibility = Visibility.Visible;

                // Only expand if user didn't manually close before
                if (!isSidebarOpen && !userClosedSidebar)
                    ToggleSidebar(forceCollapse: false);
            }
        }

        private void ToggleSidebar(bool forceCollapse = false)
        {
            // Tell pages: sidebar is moving
            //categoryPage.IsSidebarToggling = true;
            //modelPage.IsSidebarToggling = true;

            bool targetState = forceCollapse ? false : !isSidebarOpen;

            long Timing = 190;

            // -------- Profile --------
            ProfileTextStack.Visibility = targetState ? Visibility.Visible : Visibility.Collapsed;

            // Animate profile icon size
            ProfileIcon.BeginAnimation(WidthProperty,
                new DoubleAnimation(targetState ? 50 : 40, TimeSpan.FromMilliseconds(Timing)));
            ProfileIcon.BeginAnimation(HeightProperty,
                new DoubleAnimation(targetState ? 50 : 40, TimeSpan.FromMilliseconds(Timing)));

            // -------- Sidebar width --------
            double fromWidth = isSidebarOpen ? 280 : 60;
            double toWidth = targetState ? 280 : 60;

            var sidebarAnim = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(Timing),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Sidebar.BeginAnimation(WidthProperty, sidebarAnim);

            // -------- Main content margin --------
            var mainGrid = ((Grid)this.Content).Children[1] as Grid;
            if (mainGrid != null)
            {
                var marginAnim = new ThicknessAnimation
                {
                    From = mainGrid.Margin,
                    To = new Thickness(toWidth, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(Timing),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                mainGrid.BeginAnimation(Grid.MarginProperty, marginAnim);
            }

            // -------- Show/hide button texts --------
            foreach (var child in Sidebar.ChildOfType<StackPanel>().First().Children)
            {
                if (child is Button btn)
                {
                    foreach (var gridChild in btn.ContentOfType<Grid>())
                    {
                        if (gridChild.ColumnDefinitions.Count > 1)
                        {
                            var textBlock = gridChild.Children.OfType<TextBlock>().FirstOrDefault();
                            if (textBlock != null)
                                textBlock.Visibility = targetState ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }

            // -------- End of animation --------
            sidebarAnim.Completed += (s, e) =>
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new Action(() =>
                    {
                        //categoryPage.IsSidebarToggling = false;
                        //modelPage.IsSidebarToggling = false;
                    })
                );
            };


            // Update state
            isSidebarOpen = targetState;

            // Update hamburger icon
            UpdateHamburgerIcon();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Dashboard";
            PageContent.Content = new Pages.DashboardPage();
        }

        private void Client_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Clients";
            PageContent.Content = new Pages.ClientPage();
        }

        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "New Sale";
            PageContent.Content = new Pages.SalesPage();
        }

        private void SaleDetails_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Sales Details";
            PageContent.Content = new Pages.SalesDetailsPage();
        }

        private void Brands_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Brands";
            PageContent.Content = new Pages.BrandPage();
        }

        private void Series_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Series";
            PageContent.Content = new Pages.SeriePage();
        }

        private void Product_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Products";
            PageContent.Content = new Pages.ProductPage();
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Categories";
            PageContent.Content = new Pages.CategoryPage();
        }

        private bool isDeptExpanded = false;

        private void _ShowItems()
        {
            if (!isSidebarOpen)
                ToggleSidebar();

            if (!isDeptExpanded)
                ExpandDepartmentsSubmenu();
            else
                CollapseDepartmentsSubmenu();
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            _ShowItems();
        }

        private void Model_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Models";
            PageContent.Content = new Pages.ModelPage();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            // Comming soon - for demo, just change title
            MainTitle.Text = "Settings";
            PageContent.Content = new Pages.SettingsPage();
        }

        private bool isProfileActive = false; // track if profile is selected

        private void ProfileBorder_Click(object sender, MouseButtonEventArgs e)
        {
            // ── Reset other menu buttons first ───────────────────────────────
            SetActiveMenu(null); // pass null so no button gets Tag=Active

            // ── NOW mark profile as active (after SetActiveMenu reset it) ───
            isProfileActive = true;

            // ── Apply active colors ──────────────────────────────────────────
            ProfileBorder.Background = MenuActiveBg;

            var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
            if (path != null) path.Fill = MenuActiveFg;

            foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                tb.Foreground = MenuActiveFg;

            MainTitle.Text = "Profile";
            PageContent.Content = new Pages.ProfilePage();
        }

        private void ProfileBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;

            // Hover color
            border.Background = MenuHoverBg;

            var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
            if (path != null) path.Fill = MenuActiveFg;

            foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                tb.Foreground = MenuActiveFg;
        }

        private void ProfileBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            // Only reset if not active
            if (!isProfileActive)
            {
                ProfileBorder.Background = MenuNormalBg;

                var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
                if (path != null) path.Fill = MenuNormalFg;

                foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                    tb.Foreground = MenuNormalFg;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = (ScrollViewer)sender;

            // VERY SMALL STEP (try 10, 15, 20)
            double step = 15;

            if (e.Delta > 0)
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset - step);
            else
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + step);

            e.Handled = true;
        }


        // Tune this threshold to whatever "low" means for you
        private const int LowStockThreshold = 10;

        // ── Called from MainWindow_Loaded or wherever you refresh data ──
        private void LoadStockNotifications()
        {
            var notifications = new List<StockNotification>();

            DataTable dt = clsNotification.GetAll();

            foreach (DataRow row in dt.Rows)
            {
                int notificationID = Convert.ToInt32(row["NotificationID"]);
                int productID = Convert.ToInt32(row["ProductID"]);
                int type = Convert.ToInt32(row["Type"]);
                bool isRead = Convert.ToInt32(row["IsRead"]) == 1;
                DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);

                clsProduct product = clsProduct.FindByID(productID); // adjust if needed


                StockNotification notif;

                if (type == 1)
                    notif = StockNotification.OutOfStock(product.ProductName);
                else
                    notif = StockNotification.LowStock(product.ProductName, product.Quantity);

                notif.NotificationID = notificationID;
                notif.ProductID = productID;
                notif.Type = type;
                notif.IsRead = isRead;
                notif.CreatedAt = createdAt;

                notifications.Add(notif);
            }

            NotifList.ItemsSource = notifications;
            int unreadCount = notifications.Count(n => !n.IsRead);
            UpdateBadgeCount(unreadCount);

            SendUnsentNotifications();
        }

        private void NotifBell_Click(object sender, MouseButtonEventArgs e)
        {
            NotifPopup.IsOpen = !NotifPopup.IsOpen;
        }

        private void MarkAllRead_Click(object sender, MouseButtonEventArgs e)
        {
            clsNotification.MarkAllAsRead();

            NotifBadge.Visibility = Visibility.Collapsed;
            NotifPopup.IsOpen = false;
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // هادي هي نفس الدالة ديال النقر على زر NotifBell
            NotifPopup.IsOpen = !NotifPopup.IsOpen;
        }

        private void DeleteNotification_Click(object sender, RoutedEventArgs e)
        {
            //Button btn = sender as Button;
            //var data = btn.DataContext as StockNotification;

            //if (data != null)
            //{
            //    bool isDeleted = clsNotification.Delete(data.NotificationID);
            //    if (isDeleted)
            //    {
            //        MessageBox.Show("Notification deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }

            //    var list = NotifList.ItemsSource as List<StockNotification>;
            //    list.Remove(data);

            //    NotifList.Items.Refresh();
            //    UpdateBadgeCount(list.Count);
            //}
        }

        // Helper method to update the badge (you can keep this in your code)
        private void UpdateBadgeCount(int count)
        {
            if (count > 0)
            {
                NotifBadge.Visibility = Visibility.Visible;
                NotifBadgeText.Text = count > 99 ? "99+" : count.ToString();
            }
            else
            {
                NotifBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void SendUnsentNotifications()
        {
            var dt = clsNotification.GetUnsent();

            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["NotificationID"]);
                int type = Convert.ToInt32(row["Type"]);

                string title = type == 1 ? "Out of Stock" : "Low Stock";

                // you can join with product table if needed
                ShowNotification(title, "Check your stock!");

                clsNotification.MarkAsSent(id);
            }
        }

        private void Notification_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var notif = border?.DataContext as StockNotification;

            if (notif == null) return;

            // ── Load product details ─────────────────────
            clsProduct product = clsProduct.FindByID(notif.ProductID);

            // ── Show modern popup ────────────────────────
            var popup = new ProductNotificationPopup(notif, product)
            {
                Owner = Window.GetWindow(this)
            };
            popup.ShowDialog();

            // ── Mark as read (only if not already) ──────
            if (!notif.IsRead)
            {
                clsNotification.MarkAsRead(notif.NotificationID);
                notif.IsRead = true;
                NotifList.Items.Refresh();

                var list = NotifList.ItemsSource as List<StockNotification>;
                UpdateBadgeCount(list?.Count(n => !n.IsRead) ?? 0);
            }
        }

        private void Delete_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // ✅ Stop the event from bubbling up to the parent Border
            e.Handled = true;

            // ✅ Perform the delete logic right here
            Button btn = sender as Button;
            var data = btn.DataContext as StockNotification;

            if (data != null)
            {
                bool isDeleted = clsNotification.Delete(data.NotificationID);

                if (isDeleted)
                {
                    var list = NotifList.ItemsSource as List<StockNotification>;
                    if (list != null)
                    {
                        list.Remove(data);
                        NotifList.Items.Refresh();
                        UpdateBadgeCount(list.Count(n => !n.IsRead)); // Use unread count
                    }
                }
                else
                {
                    MessageBox.Show("Failed to delete notification from the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AboutMe_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "About Me";
            PageContent.Content = new Pages.AboutMePage();
        }

        private void AboutSystem_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "About System";
            PageContent.Content = new Pages.AboutSystemPage();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Help";
            PageContent.Content = new Pages.HelpPage();
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Feedback";
            PageContent.Content = new Pages.FeedbackPage();
        }

    }
}
