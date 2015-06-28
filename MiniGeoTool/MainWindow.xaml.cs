using GeoTool;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiniGeoTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region clipboard hook

        IntPtr viewerHandle = IntPtr.Zero;
        IntPtr installedHandle = IntPtr.Zero;

        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;

        [DllImport("user32.dll")]
        private extern static IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("user32.dll")]
        private extern static int ChangeClipboardChain(IntPtr hWnd, IntPtr hWndNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                installedHandle = hwndSource.Handle;
                viewerHandle = SetClipboardViewer(installedHandle);
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ChangeClipboardChain(this.installedHandle, this.viewerHandle);
            int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            e.Cancel = error != 0;

            base.OnClosing(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            this.viewerHandle = IntPtr.Zero;
            this.installedHandle = IntPtr.Zero;
            base.OnClosed(e);
        }

        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_CHANGECBCHAIN:
                    this.viewerHandle = lParam;
                    if (this.viewerHandle != IntPtr.Zero)
                    {
                        SendMessage(this.viewerHandle, msg, wParam, lParam);
                    }

                    break;

                case WM_DRAWCLIPBOARD:
                    OnClipboardChanged();

                    if (this.viewerHandle != IntPtr.Zero)
                    {
                        SendMessage(this.viewerHandle, msg, wParam, lParam);
                    }

                    break;

            }
            return IntPtr.Zero;
        }

        private event EventHandler ClipboardChanged;
        private void OnClipboardChanged()
        {
            EventHandler handler = ClipboardChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public void RegisterViewer()
        {
            viewerHandle = SetClipboardViewer(installedHandle);
        }

        public void UnregisterViewer()
        {
            ChangeClipboardChain(this.installedHandle, this.viewerHandle);
        }

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.ClipboardChanged += ClipboardHook_ClipboardChanged;
        }

        string previousClipboardText;

        private async void ClipboardHook_ClipboardChanged(object sender, EventArgs e)
        {
            string[] clipboardLines = getClipBoardLines();
            if (clipboardLines == null)
            {
                return;
            }

            if (clipboardLines.Length == 1)
            {
                string ip = IpUtilities.ExtractFirstIpFromLine(clipboardLines[0]);
                if (ip != null)
                {
                    IPAddress iPAddress = IPAddress.Parse(ip);
                    if (iPAddress != null)
                    {
                        GeoData geoData = await getAsync(iPAddress);

                        if (geoData != null)
                        {
                            if (this.WindowState == System.Windows.WindowState.Minimized)
                            {
                                string text = string.Format("{0}, {1}\n{2}\n{3}\n{4}\n{5}",
                                geoData.Country, geoData.City, geoData.Carrier, geoData.Organisation, geoData.State, geoData.Sld);

                                ntfIcn.ShowBalloonTip(geoData.IpAddress.ToString(), text, BalloonIcon.Info);
                            }

                            fillForm(geoData);
                        }
                    }
                }
            }
        }

        public async Task<GeoData> getAsync(IPAddress ip)
        {
            return await Task<GeoData>.Factory.StartNew(() =>
            {
                GeoInfo info = new GeoInfo(ip);
                return info.Get();
            });
        }

        private void fillForm(GeoData data)
        {
            label_IP.Content = data.IpAddress.ToString();
            label_Geo.Content = string.Format("{0}, {1}", data.Country, data.City);

            label_Carrier.Content = data.Carrier;
            label_Org.Content = data.Organisation;
            label_State.Content = data.State;
            label_Sld.Content = data.Sld;

            Uri uri = new Uri(string.Format("/Resources/flag-icons/{0}.png", data.CountryCode), UriKind.Relative);
            BitmapImage bitmap = new BitmapImage(uri);
            image_CountryFlag.Source = bitmap;
        }

        private string[] getClipBoardLines()
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText;
                clipboardText = Clipboard.GetText();
                if (clipboardText != previousClipboardText)
                {
                    previousClipboardText = clipboardText;

                    return clipboardText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                }
            }
            return null;
        }

        #region interface events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowLeft = this.Left;
            Properties.Settings.Default.MainWindowTop = this.Top;
            Properties.Settings.Default.MainWindowActualWidth = this.ActualWidth;
            Properties.Settings.Default.MainWindowActualHeight = this.ActualHeight;
            Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = Properties.Settings.Default.MainWindowLeft;
            this.Top = Properties.Settings.Default.MainWindowTop;
            this.Width = Properties.Settings.Default.MainWindowActualWidth;
            this.Height = Properties.Settings.Default.MainWindowActualHeight;
            this.Topmost = Properties.Settings.Default.MainWindowTopMost;
        }

        private void ntfIcn_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
        }
        #endregion


        private void btn_fromClipboard_Click(object sender, RoutedEventArgs e)
        {
            BatchWindow b = new BatchWindow();
            b.Show();
        }
    }
}
