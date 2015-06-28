using GeoTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MiniGeoTool
{
    class BatchWindowListViewView
    {
        private GeoData _ipdata;
        private string _clipdata;
        public BatchWindowListViewView(GeoData ipdata, string clipdata)
        {
            _ipdata = (GeoData)ipdata.Clone();
            _clipdata = clipdata;
        }

        public string IPAddress
        {
            get
            {
                return _ipdata.IpAddress.ToString();
            }
            private set
            {
                ;
            }
        }

        public string City
        {
            get
            {
                return _ipdata.City;
            }
            private set
            {
                ;
            }
        }

        public string Country
        {
            get
            {
                return _ipdata.Country;
            }
            private set
            {
                ;
            }
        }

        public string Organisation
        {
            get
            {
                return _ipdata.Organisation;
            }
            private set
            {
                ;
            }
        }

        public string Carrier
        {
            get
            {
                return _ipdata.Carrier;
            }
            private set
            {
                ;
            }
        }

        public string State
        {
            get
            {
                return _ipdata.State;
            }
            private set
            {
                ;
            }
        }

        public string Sld
        {
            get
            {
                return _ipdata.Sld;
            }
            private set
            {
                ;
            }
        }

        public string FlagPath
        {
            get
            {
                string path = string.Format("/Resources/flag-icons/{0}.png", _ipdata.CountryCode.ToLower());
                return path;
            }
            set
            {
                ;
            }
        }

        public string ClipboardOriginal
        {
            get
            {
                return _clipdata;
            }
            private set
            {
                ;
            }
        }

        public SolidColorBrush BackgroundColor { get; set; }
    }
}
