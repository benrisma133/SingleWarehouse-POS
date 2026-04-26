using Org.BouncyCastle.Asn1.Cmp;
using POS_BLL;
using POS_WPF.Client;
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
            cmbStatus.SelectedIndex = 1; // Default to "Active"
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

                // ADD THIS
                string statusFilter = "";
                Dispatcher.Invoke(() =>
                {
                    statusFilter = (cmbStatus.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "all";
                });

                var clientData = await Task.Run(() =>
                {
                    DataTable dt = clsClient.GetAll();

                    return dt.AsEnumerable()
                        .Where(row =>
                        {
                            bool matchesSearch =
                                row["FirstName"].ToString().ToLower().Contains(filter) ||
                                row["LastName"].ToString().ToLower().Contains(filter) ||
                                row["Phone"].ToString().ToLower().Contains(filter) ||
                                row["Email"].ToString().ToLower().Contains(filter);

                            // ADD THIS
                            int isActive = row["IsActive"] != DBNull.Value
                                ? Convert.ToInt32(row["IsActive"]) : 0;

                            bool matchesStatus = statusFilter == "all" ||
                                (statusFilter == "active" && isActive == 1) ||
                                (statusFilter == "inactive" && isActive == 0);

                            return matchesSearch && matchesStatus; // ADD matchesStatus
                        })
                        .Select(row => new
                        {
                            ClientID = Convert.ToInt32(row["ClientID"]),
                            FirstName = row["FirstName"].ToString(),
                            LastName = row["LastName"].ToString(),
                            Phone = row["Phone"].ToString(),
                            Email = row["Email"].ToString(),
                            IsActive = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1 // ADD THIS
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
                            item.Phone, item.Email, item.IsActive, cardWidth)); // ADD item.IsActive
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
                                string phone, string email, bool isActive, double width)
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

            Button toggleBtn = CardButtonsFactory.CreateToggleButton(BtnToggle_Click, id, isActive); // ADD
            Button editBtn = CardButtonsFactory.CreateEditButton(BtnEdit_Click, id);
            Button deleteBtn = CardButtonsFactory.CreateDeleteButton(BtnDelete_Click, id);

            toggleBtn.Margin = new Thickness(0, 0, 8, 0); // ADD
            editBtn.Margin = new Thickness(0, 0, 8, 0);   // CHANGE
            deleteBtn.Margin = new Thickness(0, 0, 0, 0);

            buttonStack.Children.Add(toggleBtn); // ADD
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
                // ✔ Check dependencies
                bool canDelete = clsClient.CanDelete(clientId, out int salesCount);

                // ❌ Cannot delete → show warning dialog
                if (!canDelete)
                {
                    var dialog = new frmConfirmDelete(
                        title: "Warning",
                        message:
                            $"Cannot delete this client.\n\n" +
                            $"It is linked with:\n" +
                            $"- {salesCount} Sales\n\n" +
                            $"Do you want to delete anyway?");

                    dialog.Owner = Window.GetWindow(this);

                    if (dialog.ShowDialog() != true)
                        return;

                    if (clsClient.Delete(clientId))
                    {
                        await LoadClientsAsync();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Unexpected error while deleting client.\nPlease contact support.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    return;
                }

                // ✔ No dependencies → simple confirm
                var simpleDialog = new frmConfirmDelete(
                    title: "Confirm Delete",
                    message: "This will permanently delete this client.\n\nAre you sure you want to continue?");

                simpleDialog.Owner = Window.GetWindow(this);

                if (simpleDialog.ShowDialog() != true)
                    return;

                if (clsClient.Delete(clientId))
                {
                    await LoadClientsAsync();
                }
                else
                {
                    MessageBox.Show(
                        "Unexpected error while deleting client.\nPlease contact support.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int clientId)
            {
                bool isCurrentlyActive = clsClient.GetActiveStatus(clientId);
                bool newState = !isCurrentlyActive;

                bool success = clsClient.SetActiveStatus(clientId, newState);

                if (success)
                {
                    CardButtonsFactory.SetToggleState(btn, newState);

                    string message = newState
                        ? "Client activated successfully."
                        : "Client deactivated successfully.";
                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadClientsAsync();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to update client status.",
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
                await LoadClientsAsync();
            }
        }

    }
}