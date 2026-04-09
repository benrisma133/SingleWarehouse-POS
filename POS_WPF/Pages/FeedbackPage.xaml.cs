using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MailKit.Net.Smtp;
using MimeKit;

namespace POS_WPF.Pages
{
    public partial class FeedbackPage : UserControl
    {
        // ── Put your Gmail App Password here (NOT your real password)
        private const string SenderEmail = "ibenrahhal133@gmail.com";
        private const string AppPassword = "iaox ybru jsza rowx"; // 16-char App Password
        private const string ReceiverEmail = "benrahhalismail8@gmail.com";

        public FeedbackPage()
        {
            InitializeComponent();
        }

        // Show/hide the "Other" textbox
        private void FeedbackType_Checked(object sender, RoutedEventArgs e)
        {
            if (pnlOtherType == null) return;
            pnlOtherType.Visibility = rbOther.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // ── Validation
            if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                ShowError("Please fill in both the title and message.");
                return;
            }

            if (rbOther.IsChecked == true && string.IsNullOrWhiteSpace(txtOtherType.Text))
            {
                ShowError("Please specify the feedback type.");
                return;
            }

            // ── Read all UI values BEFORE Task.Run
            string type = rbBug.IsChecked == true ? "Bug Report"
                           : rbOther.IsChecked == true ? $"Other — {txtOtherType.Text.Trim()}"
                           : "Suggestion";
            string title = txtTitle.Text.Trim();
            string message = txtMessage.Text.Trim();
            string senderName = string.IsNullOrWhiteSpace(txtSenderName.Text)
                    ? "Unknown User"
                    : txtSenderName.Text.Trim();

            // ── UI state
            btnSubmit.IsEnabled = false;
            btnClear.IsEnabled = false;
            pnlSending.Visibility = Visibility.Visible;
            pnlSuccess.Visibility = Visibility.Collapsed;
            pnlError.Visibility = Visibility.Collapsed;

            try
            {
                // ── Pass plain strings (no UI objects) into the background thread
                await Task.Run(() => SendEmail(type, title, message, senderName));

                pnlSuccess.Visibility = Visibility.Visible;
                txtTitle.Clear();
                txtMessage.Clear();
                txtOtherType.Clear();
                txtSenderName.Clear();
                rbSuggestion.IsChecked = true;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to send: {ex.Message}");
            }
            finally
            {
                pnlSending.Visibility = Visibility.Collapsed;
                btnSubmit.IsEnabled = true;
                btnClear.IsEnabled = true;
            }
        }

        private void SendEmail(string type, string title, string message, string senderName)
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("POS Feedback", SenderEmail));
            mail.To.Add(new MailboxAddress("Developer", ReceiverEmail));
            mail.Subject = $"[POS Feedback] [{type}] {title}";

            string typeColor = type.StartsWith("Bug") ? "#E53935"
                             : type.StartsWith("Suggestion") ? "#43A047"
                             : "#1E88E5";

            string html = $@"
                            <!DOCTYPE html>
                            <html>
                            <body style=""margin:0;padding:0;background:#F4F6FA;font-family:'Segoe UI',Arial,sans-serif;"">

                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F4F6FA;padding:40px 0;"">
                                    <tr>
                                        <td align=""center"">
                                            <table width=""580"" cellpadding=""0"" cellspacing=""0"">

                                                <!-- Header -->
                                                <tr>
                                                    <td style=""background:#1A1D23;border-radius:12px 12px 0 0;padding:28px 32px;"">
                                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                                            <tr>
                                                                <td>
                                                                    <p style=""margin:0;font-size:11px;font-weight:600;
                                                                               color:#FF8C42;letter-spacing:2px;"">
                                                                        POINT OF SALE SYSTEM
                                                                    </p>
                                                                    <p style=""margin:6px 0 0;font-size:22px;font-weight:700;color:#FFFFFF;"">
                                                                        New Feedback Received
                                                                    </p>
                                                                </td>
                                                                <td align=""right"">
                                                                    <span style=""background:{typeColor};color:#fff;
                                                                                  font-size:11px;font-weight:700;
                                                                                  padding:6px 14px;border-radius:20px;
                                                                                  letter-spacing:1px;"">
                                                                        {type.ToUpper()}
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>

                                                <!-- Body Card -->
                                                <tr>
                                                    <td style=""background:#FFFFFF;padding:32px;border-radius:0 0 12px 12px;
                                                                box-shadow:0 4px 20px rgba(0,0,0,0.08);"">

                                                        <!-- Title row -->
                                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                                                               style=""background:#F4F6FA;border-radius:8px;
                                                                      padding:16px 20px;margin-bottom:24px;"">
                                                            <tr>
                                                                <td>
                                                                    <p style=""margin:0 0 4px;font-size:11px;font-weight:600;
                                                                               color:#9099A8;letter-spacing:1px;"">TITLE</p>
                                                                    <p style=""margin:0;font-size:17px;font-weight:700;color:#1A1D23;"">
                                                                        {title}
                                                                    </p>
                                                                </td>
                                                            </tr>
                                                        </table>

                                                        <!-- Message -->
                                                        <p style=""margin:0 0 8px;font-size:11px;font-weight:600;
                                                                   color:#9099A8;letter-spacing:1px;"">MESSAGE</p>
                                                        <p style=""margin:0 0 28px;font-size:14px;color:#2D3142;
                                                                   line-height:1.7;white-space:pre-line;"">
                                                            {message}
                                                        </p>

                                                        <!-- Divider -->
                                                        <hr style=""border:none;border-top:1px solid #F0F0F5;margin:0 0 24px;""/>

                                                        <!-- Meta info -->
                                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                                            <tr>
                                                                <td width=""50%"">
                                                                    <p style=""margin:0 0 4px;font-size:11px;font-weight:600;
                                                                               color:#9099A8;letter-spacing:1px;"">DATE &amp; TIME</p>
                                                                    <p style=""margin:0;font-size:13px;font-weight:600;color:#2D3142;"">
                                                                        {DateTime.Now:dddd, MMM dd yyyy — HH:mm}
                                                                    </p>
                                                                </td>
                                                                <td width=""50%"" align=""right"">
                                                                    <p style=""margin:0 0 4px;font-size:11px;font-weight:600;
                                                                               color:#9099A8;letter-spacing:1px;"">SENT FROM</p>
                                                                    <p style=""margin:0;font-size:13px;font-weight:600;color:#2D3142;"">
                                                                        {senderName}
                                                                    </p>
                                                                    <p style=""margin:2px 0 0;font-size:11px;color:#9099A8;"">
                                                                        POS Desktop Application
                                                                    </p>
                                                                </td>
                                                            </tr>
                                                        </table>

                                                    </td>
                                                </tr>

                                                <!-- Footer -->
                                                <tr>
                                                    <td align=""center"" style=""padding:20px 0 0;"">
                                                        <p style=""margin:0;font-size:11px;color:#9099A8;"">
                                                            This email was sent automatically from your POS System.
                                                        </p>
                                                    </td>
                                                </tr>

                                            </table>
                                        </td>
                                    </tr>
                                </table>

                            </body>
                            </html>";

            var bodyBuilder = new BodyBuilder { HtmlBody = html };
            mail.Body = bodyBuilder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(SenderEmail, AppPassword);
                smtp.Send(mail);
                smtp.Disconnect(true);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtTitle.Clear();
            txtMessage.Clear();
            txtOtherType.Clear();
            txtSenderName.Clear();
            rbSuggestion.IsChecked = true;
            pnlSuccess.Visibility = Visibility.Collapsed;
            pnlError.Visibility = Visibility.Collapsed;
            pnlOtherType.Visibility = Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            txtError.Text = "✕  " + message;
            pnlError.Visibility = Visibility.Visible;
            pnlSuccess.Visibility = Visibility.Collapsed;
        }
    }
}