using POS_WPF.UI;
using System.Windows;
using System.Windows.Controls;

namespace POS_WPF
{
    public partial class WindowTest : Window
    {
        private bool _isActive = false;
        private Button _toggleBtn;

        public WindowTest()
        {
            InitializeComponent();
            LoadToggleButton();
        }

        private void LoadToggleButton()
        {
            _toggleBtn = CardButtonsFactory.CreateToggleButton(ToggleButton_Click, 8, _isActive);
            ToggleContainer.Children.Add(_toggleBtn);
            UpdateStatusText();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Flip the state
            _isActive = !_isActive;

            // Update button appearance
            CardButtonsFactory.SetToggleState(_toggleBtn, _isActive);

            // Update status label
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            StatusText.Text = _isActive ? "Status: Active" : "Status: Inactive";
            StatusText.Foreground = _isActive
                ? new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1ABC9C"))
                : new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#95A5A6"));
        }
    }
}