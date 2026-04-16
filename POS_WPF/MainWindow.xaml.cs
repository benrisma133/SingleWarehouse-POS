using POS_BLL;
using POS_WPF.Popup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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


        // ── Class-level fields ────────────────────────────────────────────────────
        private List<TextBlock> _sidebarButtonTexts;
        private CancellationTokenSource _sidebarCts;
        private FrameworkElement _activeMenu = null;
        private bool isSidebarOpen = true;
        private bool userClosedSidebar = false;
        private bool shouldRestoreDeptExpanded = false;
        private bool isDeptExpanded = false;
        private bool isProfileActive = false;

        private readonly Brush MenuNormalBg = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        private readonly Brush MenuHoverBg = new SolidColorBrush(Color.FromRgb(92, 92, 92));
        private readonly Brush MenuActiveBg = new SolidColorBrush(Color.FromRgb(92, 92, 92));
        private readonly Brush MenuActiveFg = new SolidColorBrush(Color.FromRgb(255, 140, 66));
        private readonly Brush MenuNormalFg = new SolidColorBrush(Color.FromRgb(245, 245, 245));

        private Geometry HamburgerIcon = Geometry.Parse("M4 6H20 M4 12H20 M4 18H20");
        private Geometry CloseIcon = Geometry.Parse("M18.3 5.71L12 12l6.3 6.29-1.41 1.42L10.59 13.4l-6.3 6.31-1.41-1.42L9.17 12 2.88 5.71l1.41-1.42 6.3 6.3 6.29-6.3z");
        private Geometry LeftArrowIcon = Geometry.Parse("M11.7071 4.29289C12.0976 4.68342 12.0976 5.31658 11.7071 5.70711L6.41421 11H20C20.5523 11 21 11.4477 21 12C21 12.5523 20.5523 13 20 13H6.41421L11.7071 18.2929C12.0976 18.6834 12.0976 19.3166 11.7071 19.7071C11.3166 20.0976 10.6834 20.0976 10.2929 19.7071L3.29289 12.7071C3.10536 12.5196 3 12.2652 3 12C3 11.7348 3.10536 11.4804 3.29289 11.2929L10.2929 4.29289C10.6834 3.90237 11.3166 3.90237 11.7071 4.29289Z");

        // ── Loaded ────────────────────────────────────────────────────────────────
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitNotifyIcon();
            UpdateHamburgerIcon();
            SetActiveMenu(DashboardButton);
            LoadStockNotifications();

            // Cache sidebar TextBlocks ONCE here — avoids traversal on every toggle
            _sidebarButtonTexts = Sidebar
                .ChildOfType<StackPanel>().First().Children
                .OfType<Button>()
                .SelectMany(btn => btn.ContentOfType<Grid>())
                .Where(g => g.ColumnDefinitions.Count > 1)
                .Select(g => g.Children.OfType<TextBlock>().FirstOrDefault())
                .Where(tb => tb != null)
                .ToList();

            MainTitle.Text = "Dashboard";
            PageContent.Content = new Pages.DashboardPage();
        }

        // ── Active menu ───────────────────────────────────────────────────────────
        private void SetActiveMenu(FrameworkElement clickedElement)
        {
            ResetProfileStyle();

            if (_activeMenu is Button oldBtn)
                oldBtn.ClearValue(Button.TagProperty);

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

        // ── Hamburger icon ────────────────────────────────────────────────────────
        private void UpdateHamburgerIcon()
        {
            if (isSidebarOpen)
            {
                HamburgerPath.Data = LeftArrowIcon;
                HamburgerBtn.HorizontalAlignment = HorizontalAlignment.Right;
                HamburgerBtn.Margin = new Thickness(0, 10, 6, 0);
            }
            else
            {
                HamburgerPath.Data = HamburgerIcon;
                HamburgerBtn.HorizontalAlignment = HorizontalAlignment.Center;
                HamburgerBtn.Margin = new Thickness(0, 10, 0, 0);
            }
        }

        // ── Hamburger click ───────────────────────────────────────────────────────
        private async void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isSidebarOpen)
            {
                shouldRestoreDeptExpanded = isDeptExpanded;
                if (isDeptExpanded) CollapseDepartmentsSubmenu();
            }
            else
            {
                await ToggleSidebar();
                if (shouldRestoreDeptExpanded) ExpandDepartmentsSubmenu();
                return;
            }

            await ToggleSidebar();
            userClosedSidebar = !isSidebarOpen;
        }

        // ── Window resize ─────────────────────────────────────────────────────────
        private async void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth < 1100)
            {
                HamburgerBtn.Visibility = Visibility.Collapsed;
                if (isSidebarOpen)
                    await ToggleSidebar(forceCollapse: true);
            }
            else
            {
                HamburgerBtn.Visibility = Visibility.Visible;
                if (!isSidebarOpen && !userClosedSidebar)
                    await ToggleSidebar(forceCollapse: false);
            }
        }

        // ── Submenus ──────────────────────────────────────────────────────────────
        private void CollapseDepartmentsSubmenu()
        {
            var animation = new DoubleAnimation
            {
                From = DepartmentsSubmenu.ActualHeight,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            DepartmentsSubmenuContainer.BeginAnimation(Border.HeightProperty, animation);
            CatalogArrow.Data = Geometry.Parse("M6 9L12 15L18 9");
            isDeptExpanded = false;
        }

        private void ExpandDepartmentsSubmenu()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = DepartmentsSubmenu.ActualHeight,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            animation.Completed += (s, _) => DepartmentsSubmenuContainer.Height = double.NaN;
            DepartmentsSubmenuContainer.BeginAnimation(Border.HeightProperty, animation);
            CatalogArrow.Data = Geometry.Parse("M6 15L12 9L18 15");
            isDeptExpanded = true;
        }

        private async void _ShowItems()
        {
            if (!isSidebarOpen)
                await ToggleSidebar();

            if (!isDeptExpanded) ExpandDepartmentsSubmenu();
            else CollapseDepartmentsSubmenu();
        }

        // ── Core toggle ───────────────────────────────────────────────────────────
        private async Task ToggleSidebar(bool forceCollapse = false)
        {
            _sidebarCts?.Cancel();
            _sidebarCts = new CancellationTokenSource();
            var token = _sidebarCts.Token;

            bool targetState = forceCollapse ? false : !isSidebarOpen;
            const int timing = 200;
            double fromWidth = Sidebar.ActualWidth;
            double toWidth = targetState ? 280 : 60;
            double profileSize = targetState ? 50 : 40;

            // Commit state before any await
            isSidebarOpen = targetState;
            UpdateHamburgerIcon();

            // Collapse: hide text instantly before animation starts
            if (!targetState)
            {
                ProfileTextStack.Visibility = Visibility.Collapsed;
                SetSidebarButtonTextsVisibility(false);
            }

            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            var duration = TimeSpan.FromMilliseconds(timing);

            Sidebar.BeginAnimation(WidthProperty, new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = duration,
                EasingFunction = ease
            });

            ProfileIcon.BeginAnimation(WidthProperty, new DoubleAnimation(profileSize, duration));
            ProfileIcon.BeginAnimation(HeightProperty, new DoubleAnimation(profileSize, duration));

            if (((Grid)this.Content).Children[1] is Grid mainGrid)
            {
                mainGrid.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation
                {
                    From = mainGrid.Margin,
                    To = new Thickness(toWidth, 0, 0, 0),
                    Duration = duration,
                    EasingFunction = ease
                });
            }

            // Expand: wait until sidebar is wide enough, then show text
            if (targetState)
            {
                try
                {
                    await Task.Delay((int)(timing * 0.6), token);
                    // Back on UI thread after await — no Dispatcher needed
                    ProfileTextStack.Visibility = Visibility.Visible;
                    SetSidebarButtonTextsVisibility(true);
                }
                catch (TaskCanceledException) { }
            }
        }

        // ── O(1) visibility flip — uses cached list, zero traversal ──────────────
        private void SetSidebarButtonTextsVisibility(bool visible)
        {
            if (_sidebarButtonTexts == null) return;
            var vis = visible ? Visibility.Visible : Visibility.Collapsed;
            foreach (var tb in _sidebarButtonTexts)
                tb.Visibility = vis;
        }

        // ── Cached page instances (lazy) ─────────────────────────────────────────
        private Pages.DashboardPage _dashboardPage;
        private Pages.ClientPage _clientPage;
        private Pages.SalesPage _salesPage;
        private Pages.SalesDetailsPage _salesDetailsPage;
        private Pages.BrandPage _brandPage;
        private Pages.SeriePage _seriePage;
        private Pages.ProductPage _productPage;
        private Pages.CategoryPage _categoryPage;
        private Pages.ModelPage _modelPage;
        private Pages.SettingsPage _settingsPage;
        private Pages.ProfilePage _profilePage;
        private Pages.AboutMePage _aboutMePage;
        private Pages.AboutSystemPage _aboutSystemPage;
        private Pages.HelpPage _helpPage;
        private Pages.FeedbackPage _feedbackPage;

        private void NavigateTo(ref Pages.DashboardPage page, string title, Button btn)
        {
            SetActiveMenu(btn);
            MainTitle.Text = title;
            if (page == null) page = new Pages.DashboardPage();
            PageContent.Content = page;
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Dashboard";
            if (_dashboardPage == null) _dashboardPage = new Pages.DashboardPage();
            PageContent.Content = _dashboardPage;
            _dashboardPage.Refresh(); // reload data without rebuilding UI
        }

        private void Client_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Clients";
            if (_clientPage == null) _clientPage = new Pages.ClientPage();
            PageContent.Content = _clientPage;
        }

        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "New Sale";
            if (_salesPage == null) _salesPage = new Pages.SalesPage();
            PageContent.Content = _salesPage;
        }

        private void SaleDetails_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Sales Details";
            if (_salesDetailsPage == null) _salesDetailsPage = new Pages.SalesDetailsPage();
            PageContent.Content = _salesDetailsPage;
        }

        private void Brands_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Brands";
            if (_brandPage == null) _brandPage = new Pages.BrandPage();
            PageContent.Content = _brandPage;
        }

        private void Series_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Series";
            if (_seriePage == null) _seriePage = new Pages.SeriePage();
            PageContent.Content = _seriePage;
        }

        private void Product_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Products";
            if (_productPage == null) _productPage = new Pages.ProductPage();
            PageContent.Content = _productPage;
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Categories";
            if (_categoryPage == null) _categoryPage = new Pages.CategoryPage();
            PageContent.Content = _categoryPage;
        }

        private void Model_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Models";
            if (_modelPage == null) _modelPage = new Pages.ModelPage();
            PageContent.Content = _modelPage;
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Settings";
            if (_settingsPage == null) _settingsPage = new Pages.SettingsPage();
            PageContent.Content = _settingsPage;
        }

        private void AboutMe_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "About Me";
            if (_aboutMePage == null) _aboutMePage = new Pages.AboutMePage();
            PageContent.Content = _aboutMePage;
        }

        private void AboutSystem_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "About System";
            if (_aboutSystemPage == null) _aboutSystemPage = new Pages.AboutSystemPage();
            PageContent.Content = _aboutSystemPage;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Help";
            if (_helpPage == null) _helpPage = new Pages.HelpPage();
            PageContent.Content = _helpPage;
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(sender as Button);
            MainTitle.Text = "Feedback";
            if (_feedbackPage == null) _feedbackPage = new Pages.FeedbackPage();
            PageContent.Content = _feedbackPage;
        }

        private void ProfileBorder_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveMenu(null);
            isProfileActive = true;
            ProfileBorder.Background = MenuActiveBg;
            var path = ProfileIcon.ChildOfType<Path>().FirstOrDefault();
            if (path != null) path.Fill = MenuActiveFg;
            foreach (var tb in ProfileTextStack.Children.OfType<TextBlock>())
                tb.Foreground = MenuActiveFg;

            MainTitle.Text = "Profile";
            if (_profilePage == null) _profilePage = new Pages.ProfilePage();
            PageContent.Content = _profilePage;
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

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            _ShowItems();
        }

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

        

    }
}
