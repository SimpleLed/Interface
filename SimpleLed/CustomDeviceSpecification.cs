using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleLed
{
    public enum RGBOrder
    {
        RGB=0,
        RBG=1,
        GBR=2,
        GRB=3,
        BGR=4,
        BRG=5
    }
    public class CustomDeviceSpecification : BaseViewModel
    {
        private RGBOrder rgbOrder = RGBOrder.RGB;

        public RGBOrder RGBOrder
        {
            get => rgbOrder;
            set => SetProperty(ref rgbOrder, value);
        }
        private string madeByName;
        public string MadeByName
        {
            get => madeByName;
            set => SetProperty(ref madeByName, value);
        }


        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private int ledCount;

        public int LedCount
        {
            get => ledCount;
            set => SetProperty(ref ledCount, value);
        }

        private byte[] pngData;

        public byte[] PngData
        {
            get => pngData;
            set
            {
                SetProperty(ref pngData, value);
                if (value != null)
                {
                    try
                    {
                        Bitmap = new Bitmap(new MemoryStream(value));
                    }
                    catch
                    {
                    }
                }
            }
        }

        private Bitmap bitmap;

        [JsonIgnore]
        public Bitmap Bitmap
        {
            get => bitmap;
            set => bitmap = value;
        }

        private string mapperName;

        public string MapperName
        {
            get => mapperName;
            set => SetProperty(ref mapperName, value);
        }

        private int gridWidth = 0;
        public int GridWidth
        {
            get => gridWidth;
            set => SetProperty(ref gridWidth, value);
        }


        private int gridHeight = 0;
        public int GridHeight
        {
            get => gridHeight;
            set => SetProperty(ref gridHeight, value);
        }
    }

    public class EightByEightMatrix : CustomDeviceSpecification
    {
        public EightByEightMatrix()
        {
            LedCount = 64;
            GridWidth = 8;
            GridHeight = 8;
            MapperName = "SMatrix";
            Name = "8x8 Matrix";
            PngData = ImageHelper.ReadImageStream("8x8grid.png");
            RGBOrder = RGBOrder.BRG;
        }

    }

    public class GenericLEDStrip : CustomDeviceSpecification
    {
        public GenericLEDStrip(int leds = 30)
        {
            LedCount = leds;
            Name = "LED Strip";
            PngData = ImageHelper.ReadImageStream("LedStrip.png");

        }
    }


    public class GenericFan : CustomDeviceSpecification
    {
        public GenericFan(int leds = 21)
        {
            LedCount = leds;
            Name = "Fan";
            PngData = ImageHelper.ReadImageStream("Fan.png");
        }
    }

    public class GenericAIO : CustomDeviceSpecification
    {
        public GenericAIO(int leds = 1)
        {
            LedCount = leds;
            Name = "AIO";
            PngData = ImageHelper.ReadImageStream("AIO.png");
        }
    }

    public class GenericBulb : CustomDeviceSpecification
    {
        public GenericBulb(int leds = 1)
        {
            LedCount = leds;
            Name = "Bulb";
            PngData = ImageHelper.ReadImageStream("Bulb.png");
        }
    }


    public class GenericOther : CustomDeviceSpecification
    {
        public GenericOther(int leds = 1)
        {
            LedCount = leds;
            Name = "Other";
            PngData = ImageHelper.ReadImageStream("Other.png");
        }
    }

    public class GenericCooler : CustomDeviceSpecification
    {
        public GenericCooler(int leds = 1)
        {
            LedCount = leds;
            Name = "Cooler";
            PngData = ImageHelper.ReadImageStream("Cooler.png");
        }
    }
    public class GenericGPU : CustomDeviceSpecification
    {
        public GenericGPU(int leds = 1)
        {
            LedCount = leds;
            Name = "GPU";
            PngData = ImageHelper.ReadImageStream("gpu.png");
        }
    }


    public class GenericHeadSet : CustomDeviceSpecification
    {
        public GenericHeadSet(int leds = 2)
        {
            LedCount = leds;
            Name = "Headset";
            PngData = ImageHelper.ReadImageStream("Headset.png");
        }
    }

    public class GenericKeyboard : CustomDeviceSpecification
    {
        public GenericKeyboard(int leds = 106)
        {
            LedCount = leds;
            Name = "Keyboard";
            PngData = ImageHelper.ReadImageStream("Keyboard.png");
        }
    }

    public class GenericKeypad : CustomDeviceSpecification
    {
        public GenericKeypad(int leds = 25)
        {
            LedCount = leds;
            Name = "Keypad";
            PngData = ImageHelper.ReadImageStream("Keypad.png");
        }
    }


    public class GenericMemory : CustomDeviceSpecification
    {
        public GenericMemory(int leds = 8)
        {
            LedCount = leds;
            Name = "Memory";
            PngData = ImageHelper.ReadImageStream("Memory.png");
        }
    }


    public class GenericMotherboard : CustomDeviceSpecification
    {
        public GenericMotherboard(int leds = 4)
        {
            LedCount = leds;
            Name = "Motherboard";
            PngData = ImageHelper.ReadImageStream("MotherBoard.png");
        }
    }


    public class GenericMouse : CustomDeviceSpecification
    {
        public GenericMouse(int leds = 3)
        {
            LedCount = leds;
            Name = "Mouse";
            PngData = ImageHelper.ReadImageStream("Mouse.png");
        }
    }


    public class GenericMousePad : CustomDeviceSpecification
    {
        public GenericMousePad(int leds = 2)
        {
            LedCount = leds;
            Name = "MousePad";
            PngData = ImageHelper.ReadImageStream("MousePad.png");
        }
    }

    public class GenericSpeakers : CustomDeviceSpecification
    {
        public GenericSpeakers(int leds = 2)
        {
            LedCount = leds;
            Name = "Speakers";
            PngData = ImageHelper.ReadImageStream("Speakers.png");
        }
    }

    public class GenericWaterBlock : CustomDeviceSpecification
    {
        public GenericWaterBlock(int leds = 1)
        {
            LedCount = leds;
            Name = "Waterblock";
            PngData = ImageHelper.ReadImageStream("WaterBlock.png");
        }
    }


    public class GenericPSU : CustomDeviceSpecification
    {
        public GenericPSU(int leds = 1)
        {
            LedCount = leds;
            Name = "PSU";
            PngData = ImageHelper.ReadImageStream("PSU.png");
        }
    }


    internal static class ImageHelper
    {
        internal static byte[] ReadImageStream(string name)
        {
            Stream imgStream = System.Reflection.Assembly.GetAssembly(typeof(ISimpleLed))
                .GetManifestResourceStream("SimpleLed.Images." + name);
            var temp = new byte[imgStream.Length];
            imgStream.Read(temp, 0, (int)imgStream.Length);

            return temp;
        }
    }



    public interface Mapper
    {
        string GetName();

        int GetLed(int x, int y, CustomDeviceSpecification spec);

        void SetParams(string prms);
    }

    public class SMatrix : Mapper
    {
        public string GetName() => "SMatrix";


        public int GetLed(int x, int y, CustomDeviceSpecification spec)
        {
            if (y % 2 == 0) x = (spec.GridWidth-1 - x);
            return (y * spec.GridHeight) + x;
        }

        public void SetParams(string prms)
        {

        }
    }


    public class LMatrix : Mapper
    {
        public string GetName() => "LMatrix";


        public int GetLed(int x, int y, CustomDeviceSpecification spec) => (y * spec.GridHeight) + x;

        public void SetParams(string prms)
        {

        }
    }
}