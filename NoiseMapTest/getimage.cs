using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace NoiseMapTest
{
    class getimage
    {

        public void getmap()
        {
            String path = "E:\\test\\workspace\\temp\\pngtest.tiff";
            Bitmap image = new Bitmap(2000, 2000);
            Graphics g = Graphics.FromImage(image);
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            //  对于C#的随机数，没什么好说的
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);
            //填充透明色
            g.Clear(Color.Transparent);
            SolidBrush mysbrush;
            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < 2000; j++)
                {
                    int int_Red = RandomNum_First.Next(256);
                    int int_Green = RandomNum_Sencond.Next(256);
                    int int_Blue = RandomNum_Sencond.Next(256);
                    int a = RandomNum_Sencond.Next(256);
                    //  int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
                    //  int_Blue = (int_Blue > 255) ? 255 : int_Blue;

                    Color color = Color.FromArgb(a, int_Red, int_Green, int_Blue);
                    mysbrush = new SolidBrush(color);
                    g.FillRectangle(mysbrush, i, j, 1, 1);

                }
            }
            g.Dispose();
            image.Save(path, System.Drawing.Imaging.ImageFormat.Tiff);
            image.Dispose();
        }
    }
}
