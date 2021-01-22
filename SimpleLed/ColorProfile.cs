using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleLed
{
    public class ColorProfile : BaseViewModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string profileName;

        public string ProfileName
        {
            get => profileName;
            set => SetProperty(ref profileName, value);
        }

        private ObservableCollection<ColorBank> colorBanks = new ObservableCollection<ColorBank>();
        public ObservableCollection<ColorBank> ColorBanks
        {
            get => colorBanks;
            set => SetProperty(ref colorBanks, value);
        }
    }

    public class ColorBank : BaseViewModel
    {
        private string bankName;

        public string BankName
        {
            get => bankName;
            set => SetProperty(ref bankName, value);
        }

        private ObservableCollection<ColorObject> colors = new ObservableCollection<ColorObject>();
        public ObservableCollection<ColorObject> Colors
        {
            get => colors;
            set => SetProperty(ref colors, value);
        }
    }

    public class ColorObject : BaseViewModel
    {
        private ColorModel color;

        public ColorModel Color
        {
            get => color;
            set
            {
                SetProperty(ref color, value);
                OnPropertyChanged("ColorString");
            }
        }

        private string colorString;

        public string ColorString
        {
            get => Color.ToString();
            set
            {
                if (value.Replace("#", "").Length == 6)
                {
                    try
                    {
                        var c = (Color)ColorConverter.ConvertFromString(value);
                        Color = new ColorModel(c.R, c.G, c.B);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
    public class ColorModel : BaseViewModel
    {
        public override string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";

        private int red = 0;
        private int green = 0;
        private int blue = 0;

        public int Red
        {
            get => red;
            set => SetProperty(ref red, value);
        }

        public int Green
        {
            get => green;
            set => SetProperty(ref green, value);
        }

        public int Blue
        {
            get => blue;
            set => SetProperty(ref blue, value);
        }

        public ColorModel()
        {

        }


        public ColorModel(int r, int g, int b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
    }
}
