using POS_BLL;
using POS_WPF.Client;
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
    public partial class ClientPage : UserControl
    {
        public bool IsSidebarToggling { get; set; } = false;

        public ClientPage()
        {
            InitializeComponent();
        }

        // ======================= LIFECYCLE =======================

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClientsAsync();
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

        // ======================= LOAD CLIENTS =======================

        public async Task LoadClientsAsync()
        {
            try
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Visible;

                if (txtTitle != null)
                    txtTitle.Text = "Loading...";

                await Task.Delay(50);

                string filter = GetSearchText();

                var clientData = await Task.Run(() =>
                {
                    DataTable dt = clsClient.GetAll();

                    return dt.AsEnumerable()
                        .Where(row =>
                            row["FirstName"].ToString().ToLower().Contains(filter) ||
                            row["LastName"].ToString().ToLower().Contains(filter) ||
                            row["Phone"].ToString().ToLower().Contains(filter) ||
                            row["Email"].ToString().ToLower().Contains(filter))
                        .Select(row => new
                        {
                            ClientID = Convert.ToInt32(row["ClientID"]),
                            FirstName = row["FirstName"].ToString(),
                            LastName = row["LastName"].ToString(),
                            Phone = row["Phone"].ToString(),
                            Email = row["Email"].ToString()
                        })
                        .ToList();
                });

                DynamicCardContainer.Items.Clear();
                double cardWidth = GetCardWidth();

                foreach (var item in clientData)
                {
                    DynamicCardContainer.Items.Add(
                        CreateClientCard(
                            item.ClientID, item.FirstName, item.LastName,
                            item.Phone, item.Email, cardWidth));
                }

                var wrapPanel = FindVisualChild<WrapPanel>(DynamicCardContainer);
                if (wrapPanel != null)
                {
                    wrapPanel.Margin = new Thickness(14);
                    wrapPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    wrapPanel.VerticalAlignment = VerticalAlignment.Top;
                }

                UpdateCardWidths();

                if (txtTitle != null)
                    txtTitle.Text = "Client Management";

                if (txtTotalClients != null)
                    txtTotalClients.Text = $"Total Clients : {clientData.Count}";

                if (NoClientsMessage != null)
                {
                    NoClientsMessage.Text = "No clients found";
                    NoClientsMessage.Visibility = clientData.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                if (txtTitle != null)
                    txtTitle.Text = "Client Management - Error";

                MessageBox.Show("Error loading clients: " + ex.Message,
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

        private Border CreateClientCard(int id, string firstName, string lastName,
                                        string phone, string email, double width)
        {
            string fullName = $"{firstName} {lastName}".Trim();
            string initials = GetInitials(firstName, lastName);

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

            // ── Layout ──
            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ── Avatar badge + name row ──
            StackPanel textStack = new StackPanel();

            StackPanel topRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 8)
            };

            Border badge = new Border
            {
                Width = 54,
                Height = 54,
                Margin = new Thickness(0, 0, 16, 0),
                Background = new SolidColorBrush(Color.FromRgb(47, 136, 255)),
                CornerRadius = new CornerRadius(27)   // circle for people
            };
            badge.Child = new TextBlock
            {
                Text = initials,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            topRow.Children.Add(badge);

            // Name + email stacked next to avatar
            StackPanel nameEmailStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            nameEmailStack.Children.Add(new TextBlock
            {
                Text = fullName,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                FontFamily = new FontFamily("Segoe UI")
            });
            nameEmailStack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(email) ? "No email" : email,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 2, 0, 0)
            });
            topRow.Children.Add(nameEmailStack);
            textStack.Children.Add(topRow);

            // ── Phone row ──
            StackPanel phoneRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 0)
            };

            // Phone icon (SVG path)
            Viewbox phoneIcon = new Viewbox { Width = 14, Height = 14, Margin = new Thickness(0, 0, 6, 0) };
            phoneIcon.Child = new Path
            {
                Fill = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Stretch = Stretch.Uniform,
                Data = Geometry.Parse("M6.6 10.8c1.4 1.4 2.9 2.5 4.4 3.3l1.5-1.5c.2-.2.4-.2.6-.1 1.1.4 2.3.7 3.5.7.3 0 .5.2.5.5v3.5c0 .3-.2.5-.5.5C8.6 17.7 2.3 11.4 2.3 3.7c0-.3.2-.5.5-.5H6.3c.3 0 .5.2.5.5 0 1.2.2 2.4.7 3.5.1.2 0 .4-.1.6L6 9.3c.2.5.4 1 .6 1.5z")
            };
            phoneRow.Children.Add(phoneIcon);
            phoneRow.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(phone) ? "No phone" : phone,
                FontSize = 13,
                Foreground = new SolidColorBrush(
                    string.IsNullOrWhiteSpace(phone)
                        ? Color.FromRgb(148, 163, 184)
                        : Color.FromRgb(100, 116, 139)),
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = string.IsNullOrWhiteSpace(phone) ? FontStyles.Italic : FontStyles.Normal
            });
            textStack.Children.Add(phoneRow);

            cardGrid.Children.Add(textStack);

            // ── Action buttons ──
            StackPanel buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(buttonStack, 1);

            Button editBtn = CardButtonsFactory.CreateEditButton(BtnEdit_Click, id);
            Button deleteBtn = CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id);
            deleteBtn.Margin = new Thickness(8, 0, 0, 0);

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

        // ======================= HOVER ANIMATIONS =======================

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
            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(1.01, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });

            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(155, 155, 155), TimeSpan.FromMilliseconds(350)));

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(18, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0.28, TimeSpan.FromMilliseconds(260)));
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
                scale?.BeginAnimation(ScaleTransform.ScaleXProperty,
                    new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
                scale?.BeginAnimation(ScaleTransform.ScaleYProperty,
                    new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250)));
            }

            ((SolidColorBrush)cardBorder.BorderBrush).BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(Color.FromRgb(223, 230, 233), TimeSpan.FromMilliseconds(350)));

            if (cardBorder.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(12, TimeSpan.FromMilliseconds(260)));
                shadow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0.15, TimeSpan.FromMilliseconds(260)));
            }

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fadeOut.Completed += (s, e) => buttonStack.Visibility = Visibility.Collapsed;
            buttonStack.BeginAnimation(StackPanel.OpacityProperty, fadeOut);
        }

        // ======================= HELPERS =======================

        private static string GetInitials(string firstName, string lastName)
        {
            string f = string.IsNullOrWhiteSpace(firstName) ? "" : firstName[0].ToString().ToUpper();
            string l = string.IsNullOrWhiteSpace(lastName) ? "" : lastName[0].ToString().ToUpper();
            return $"{f}{l}";
        }

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
            double avail = CardScrollViewer.ActualWidth
                           - wp.Margin.Left - wp.Margin.Right - 16;

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

        // ======================= EVENTS =======================

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
            => UpdateCardWidths();

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            await LoadClientsAsync();
            UpdateCardWidths();
        }

        private async void BtnAddClient_Click(object sender, RoutedEventArgs e)
        {
            var win = new frmAddEditClient();
            win.Owner = Application.Current.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.ShowDialog();

            if (win.IsSaved)
                await LoadClientsAsync();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int clientId)
            {
                var win = new frmAddEditClient(clientId);
                win.Owner = Application.Current.MainWindow;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                win.ShowDialog();

                // Reload whether saved or just edited
                await LoadClientsAsync();
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int clientId)
            {
                var result = MessageBox.Show(
                    "This will permanently delete this client.\n\nDo you want to proceed?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (clsClient.Delete(clientId))
                    {
                        MessageBox.Show("Client deleted successfully.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadClientsAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Error deleting client. They may be linked to existing orders.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}