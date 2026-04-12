using POS_BLL;
using POS_WPF.Utils;
using SQLitePCL;
using System;
using System.Windows;

namespace POS_WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Batteries.Init();

            string savedLanguage = POS_WPF.Properties.Settings.Default.AppLanguage;

            if (!string.IsNullOrEmpty(savedLanguage))
            {
                LanguageManager.Instance.ChangeLanguage(savedLanguage);
            }

            // 1. Ensure admin exists
            //clsUser.EnsureDefaultAdmin();

            // 2. 🔥 FIX: if password is NOT hashed
            //FixAdminPassword();
        }

        private void FixAdminPassword()
        {
            try
            {
                // ❗ Get user using PLAIN password (old data)
                clsUser user = clsUser.FindByUsernameAndPassword("admin", "admin");

                if (user != null)
                {
                    // 🔥 Hash it
                    user.PasswordHash = clsUtil.HashPassword("admin");

                    // 🔥 Update using your BLL (calls Update)
                    user.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fixing admin password: " + ex.Message);
            }
        }
    }
}