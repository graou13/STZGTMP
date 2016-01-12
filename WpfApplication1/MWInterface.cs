using                                   System;
using                                   System.Collections.Generic;
using                                   System.Drawing;
using                                   System.Linq;
using                                   System.Text;
using                                   System.Threading.Tasks;
using                                   System.Windows;
using                                   System.Windows.Interop;
using                                   System.Windows.Media.Imaging;

namespace                               WindowsMediaPlayer
{
    class                               MWInterface
    {
        private System.Drawing.Image    IconMap;

        public                          MWInterface()
        {
            System.Reflection.Assembly  thisExe = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream            file = thisExe.GetManifestResourceStream("WindowsMediaPlayer.Icons.png");
            
            if ((this.IconMap = System.Drawing.Image.FromStream(file)) == null)
                throw new MWException("System.Drawing.Image.FromStream failed with parameter (" + file + ")");
        }

        public BitmapSource             getIcon(int Index)
        {
            if (this.IconMap == null)
                throw new MWException("The icon map has not been initialized");

            int x = Convert.ToInt32(this.IconMap.Width) / 30;
            int y = Convert.ToInt32(this.IconMap.Height) / 30;
            int iconCount = x * y;
            if (Index < 0 || Index >= iconCount) return null;
            int offsetX = (Index % x) * 30;
            int offsetY = (Index / x) * 30;
            Bitmap bitmap = new Bitmap(30, 30);
            Graphics.FromImage(bitmap).DrawImage(this.IconMap,
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                new System.Drawing.Rectangle(offsetX, offsetY, bitmap.Width, bitmap.Height),
                GraphicsUnit.Pixel);
            IntPtr hbitmap = bitmap.GetHbitmap();
            BitmapSource bimage = Imaging.CreateBitmapSourceFromHBitmap(
                hbitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
            return (bimage);
        }
    }
}
