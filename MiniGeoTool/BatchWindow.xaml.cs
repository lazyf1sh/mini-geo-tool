using GeoTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
using NLog;

namespace MiniGeoTool
{
    /// <summary>
    /// Interaction logic for BatchWindow.xaml
    /// </summary>
    public partial class BatchWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        List<BatchWindowListViewView> listViewView = new List<BatchWindowListViewView>();
        List<BatchWindowPercentView> percentView = new List<BatchWindowPercentView>();

        Dictionary<string, int> colorizeBy = new Dictionary<string, int>();

        public BatchWindow()
        {
            InitializeComponent();
            lstw.Visibility = System.Windows.Visibility.Collapsed;
            lstwPercents.Visibility = System.Windows.Visibility.Collapsed;
            if (Clipboard.ContainsText())
            {


                prepareList();



            }
            else
            {
                this.Content = "Clipboard didn't contain any text";
            }
        }

        private async void prepareList()
        {
            string[] lines = Clipboard.GetText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            prgrssbr.Maximum = lines.Length -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                prgrssbr.Value = i;
                string stringIP = IpUtilities.ExtractFirstIpFromLine(line);
                if (stringIP != null)
                {
                    GeoData g = await getAsync(IPAddress.Parse(stringIP));
                    if (g != null)
                    {
                        BatchWindowListViewView lvv = new BatchWindowListViewView(g, line);
                        listViewView.Add(lvv);
                    }
                }

            }

            lbl_LinesWithIP.Content = string.Format("Lines contain IPs: {0}", listViewView.Count.ToString());
            colorizeByOrganisation();

            lstw.ItemsSource = listViewView;
            lstwPercents.ItemsSource = percentView;

            lstw.Visibility = System.Windows.Visibility.Visible;
            lstwPercents.Visibility = System.Windows.Visibility.Visible;
            prgrssbr.Visibility = System.Windows.Visibility.Collapsed;
        }

        public async Task<GeoData> getAsync(IPAddress ip)
        {
            return await Task<GeoData>.Factory.StartNew(() =>
            {
                GeoInfo info = new GeoInfo(ip);
                return info.Get();
            });
        }



        #region interface events
        //http://stackoverflow.com/questions/607827/what-is-the-difference-between-width-and-actualwidth-in-wpf
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.BatchWindowLeft = this.Left;
            Properties.Settings.Default.BatchWindowTop = this.Top;
            Properties.Settings.Default.BatchWindowWidth = this.Width;
            Properties.Settings.Default.BatchWindowHeight = this.Height;
            Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = Properties.Settings.Default.BatchWindowLeft;
            this.Top = Properties.Settings.Default.BatchWindowTop;
            this.Width = Properties.Settings.Default.BatchWindowWidth;
            this.Height = Properties.Settings.Default.BatchWindowHeight;
            this.Topmost = Properties.Settings.Default.BatchWindowTopMost;
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            string header = null;
            try
            {
                header = headerClicked.Column.Header as string;
            }
            catch
            {
                return;
            }

            switch (header)
            {
                case "Clipboard Line":
                case "IP":
                    clearColors();
                    break;
                case "Country":
                    colorizeByCountry();
                    break;
                case "City":
                    colorizeByCity();
                    break;
                case "Carrier":
                    colorizeByCarrier();
                    break;
                case "Organisation":
                    colorizeByOrganisation();
                    break;
                case "Sld":
                    colorizeBySld();
                    break;
                case "State":
                    colorizeByState();
                    break;
                default:
                    break;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(lstw.ItemsSource);
            view.Refresh();

            ICollectionView view2 = CollectionViewSource.GetDefaultView(lstwPercents.ItemsSource);
            view2.Refresh();
        }
        #endregion

        #region colorize funcs

        private void clearColors()
        {
            foreach (BatchWindowListViewView lv in listViewView)
            {
                lv.BackgroundColor = null;
            }

            foreach (BatchWindowPercentView p in percentView)
            {
                p.BackgroundColor = null;
            }
        }

        private void colorizeByCountry()
        {

            percentView.Clear();

            var query = listViewView.GroupBy(x => x.Country)
                                    .Select(g => new { Value = g.Key, Count = g.Count() })
                                    .OrderByDescending(x => x.Count);


            //generate BatchWindowPercentView and colors
            Random random = new Random();
            foreach (var field in query)
            {
                BatchWindowPercentView prcnts = new BatchWindowPercentView();
                prcnts.Field = field.Value;
                prcnts.Percent = (int)((double)field.Count / (double)listViewView.Count * 100);

                int colorR = random.Next(130, 255);
                int colorG = random.Next(130, 255);
                int colorB = random.Next(130, 255);

                prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                percentView.Add(prcnts);
            }

            //assign colors based on field.Value
            foreach (BatchWindowListViewView lv in listViewView)
            {
                foreach (BatchWindowPercentView p in percentView)
                {
                    if (lv.Country == p.Field)
                    {
                        lv.BackgroundColor = p.BackgroundColor;
                    }
                }
            }
        }

        private void colorizeByCity()
        {

            percentView.Clear();


            var query = listViewView.GroupBy(x => x.City)
.Select(g => new { Value = g.Key, Count = g.Count() })
.OrderByDescending(x => x.Count);


            //generate BatchWindowPercentView and colors
            Random random = new Random();
            foreach (var field in query)
            {
                BatchWindowPercentView prcnts = new BatchWindowPercentView();
                prcnts.Field = field.Value;
                prcnts.Percent = (int)((double)field.Count / (double)listViewView.Count * 100);

                int colorR = random.Next(130, 255);
                int colorG = random.Next(130, 255);
                int colorB = random.Next(130, 255);

                prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                percentView.Add(prcnts);
            }

            //assign colors based on field.Value
            foreach (BatchWindowListViewView lv in listViewView)
            {
                foreach (BatchWindowPercentView p in percentView)
                {
                    if (lv.City == p.Field)
                    {
                        lv.BackgroundColor = p.BackgroundColor;
                    }
                }
            }
        }

        private void colorizeByCarrier()
        {

            percentView.Clear();

            var query = listViewView.GroupBy(x => x.Carrier)
.Select(g => new { Value = g.Key, Count = g.Count() })
.OrderByDescending(x => x.Count);


            //generate BatchWindowPercentView and colors
            Random random = new Random();
            foreach (var field in query)
            {
                BatchWindowPercentView prcnts = new BatchWindowPercentView();
                prcnts.Field = field.Value;
                prcnts.Percent = (int)((double)field.Count / (double)listViewView.Count * 100);

                int colorR = random.Next(130, 255);
                int colorG = random.Next(130, 255);
                int colorB = random.Next(130, 255);

                prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                percentView.Add(prcnts);
            }

            //assign colors based on field.Value
            foreach (BatchWindowListViewView lv in listViewView)
            {
                foreach (BatchWindowPercentView p in percentView)
                {
                    if (lv.Carrier == p.Field)
                    {
                        lv.BackgroundColor = p.BackgroundColor;
                    }
                }
            }
        }

        private void colorizeByOrganisation()
        {

            percentView.Clear();


            var query = listViewView.GroupBy(x => x.Organisation)
.Select(g => new { Value = g.Key, Count = g.Count() })
.OrderByDescending(x => x.Count);


            //generate BatchWindowPercentView and colors
            Random random = new Random();
            foreach (var field in query)
            {
                BatchWindowPercentView prcnts = new BatchWindowPercentView();
                prcnts.Field = field.Value;
                prcnts.Percent = (int)((double)field.Count / (double)listViewView.Count * 100);

                int colorR = random.Next(130, 255);
                int colorG = random.Next(130, 255);
                int colorB = random.Next(130, 255);

                prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                percentView.Add(prcnts);
            }

            //assign colors based on field.Value
            foreach (BatchWindowListViewView lv in listViewView)
            {
                foreach (BatchWindowPercentView p in percentView)
                {
                    if (lv.Organisation == p.Field)
                    {
                        lv.BackgroundColor = p.BackgroundColor;
                    }
                }
            }
        }

        private void colorizeBySld()
        {

            percentView.Clear();


            var query = listViewView.GroupBy(x => x.Sld)
.Select(g => new { Value = g.Key, Count = g.Count() })
.OrderByDescending(x => x.Count);


            //generate BatchWindowPercentView and colors
            Random random = new Random();
            foreach (var field in query)
            {
                BatchWindowPercentView prcnts = new BatchWindowPercentView();
                prcnts.Field = field.Value;
                prcnts.Percent = (int)((double)field.Count / (double)listViewView.Count * 100);

                int colorR = random.Next(130, 255);
                int colorG = random.Next(130, 255);
                int colorB = random.Next(130, 255);

                prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                percentView.Add(prcnts);
            }

            //assign colors based on field.Value
            foreach (BatchWindowListViewView lv in listViewView)
            {
                foreach (BatchWindowPercentView p in percentView)
                {
                    if (lv.Sld == p.Field)
                    {
                        lv.BackgroundColor = p.BackgroundColor;
                    }
                }
            }
        }

        private void colorizeByState()
        {

            percentView.Clear();

            var query = listViewView.GroupBy(x => x.State)
.Select(g => new { Value = g.Key, Count = g.Count() })
.OrderByDescending(x => x.Count);


            //generate BatchWindowPercentView and colors
            Random random = new Random();
            foreach (var field in query)
            {
                BatchWindowPercentView prcnts = new BatchWindowPercentView();
                prcnts.Field = field.Value;
                prcnts.Percent = (int)((double)field.Count / (double)listViewView.Count * 100);

                int colorR = random.Next(130, 255);
                int colorG = random.Next(130, 255);
                int colorB = random.Next(130, 255);

                prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                percentView.Add(prcnts);
            }

            //assign colors based on field.Value
            foreach (BatchWindowListViewView lv in listViewView)
            {
                foreach (BatchWindowPercentView p in percentView)
                {
                    if (lv.State == p.Field)
                    {
                        lv.BackgroundColor = p.BackgroundColor;
                    }
                }
            }
        }
        #endregion
    }
}
