using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_WPF.Pages
{
    /// <summary>
    /// Interaction logic for AboutMePage.xaml
    /// </summary>
    public partial class AboutMePage : UserControl
    {
        public AboutMePage()
        {
            InitializeComponent();
        }

        private void Email_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start("mailto:ibenrahhal133@gmail.com");
        }

        private void GitHub_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/benrisma133");
        }

        private void LinkedIn_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.linkedin.com/in/benrisma133");
        }

        private void WhatsApp_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Opens WhatsApp chat directly in browser or WhatsApp app
                string number = "212600144024"; // No + sign, no spaces
                string url = $"https://wa.me/{number}";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open WhatsApp:\n{ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }
    }
}
