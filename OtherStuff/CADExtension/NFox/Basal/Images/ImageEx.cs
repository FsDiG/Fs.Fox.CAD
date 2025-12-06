using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NFox.Cad
{
    /// <summary>
    /// 图像扩展类
    /// </summary>
    public static class ImageEx
    {
        /// <summary>
        /// WPF 颜色转换为 winform 颜色
        /// </summary>
        /// <param name="color">WPF 颜色</param>
        /// <returns>winform 颜色</returns>
        public static System.Drawing.Color ToDrawColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// winform 颜色转换为 WPF 颜色
        /// </summary>
        /// <param name="color">winform 颜色</param>
        /// <returns>WPF 颜色</returns>
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// winform 图像转换为 wpf 图像
        /// </summary>
        /// <param name="bitmap">winform 图像</param>
        /// <returns>wpf 图像</returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        // BitmapImage --> Bitmap
        /// <summary>
        /// wpf 图像转换为 winform 图像
        /// </summary>
        /// <param name="bitmapImage">wpf 图像</param>
        /// <returns>winform 图像</returns>
        public static Bitmap ToBitmap(this BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// RenderTargetBitmap 转换为 BitmapImage
        /// </summary>
        /// <param name="wbm">RenderTargetBitmap</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage ToBitmapImage(this RenderTargetBitmap wbm)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.StreamSource = new MemoryStream(stream.ToArray()); //stream;
                bmp.EndInit();
                bmp.Freeze();
            }
            return bmp;
        }

        /// <summary>
        /// RenderTargetBitmap 转换为 BitmapImage 的另一种方法
        /// </summary>
        /// <param name="rtb">RenderTargetBitmap</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage ToBitmapImage1(this RenderTargetBitmap rtb)
        {
            var renderTargetBitmap = rtb;
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        // ImageSource --> Bitmap
        /// <summary>
        /// wpf 图像资源转换为 winform 图像
        /// </summary>
        /// <param name="imageSource">wpf 图像资源</param>
        /// <returns>winform 图像</returns>
        public static System.Drawing.Bitmap ToBitmap(this ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            Bitmap bmp = new Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }

        /// <summary>
        /// wpf 图像转换为二进制数组
        /// </summary>
        /// <param name="bmp">wpf 图像</param>
        /// <returns>二进制数组</returns>
        public static byte[] ToByteArray(this BitmapImage bmp)
        {
            byte[] bytearray = null;
            try
            {
                Stream smarket = bmp.StreamSource; ;
                if (smarket != null && smarket.Length > 0)
                {
                    //设置当前位置
                    smarket.Position = 0;
                    using (BinaryReader br = new BinaryReader(smarket))
                    {
                        bytearray = br.ReadBytes((int)smarket.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return bytearray;
        }

        // byte[] --> BitmapImage
        /// <summary>
        /// 二进制数组转换为 wpf 图像
        /// </summary>
        /// <param name="array">二进制数组</param>
        /// <returns>wpf 图像</returns>
        public static BitmapImage ToBitmapImage(this byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        /// <summary>
        /// 二进制数组转换为 winform 图像
        /// </summary>
        /// <param name="bytes">二进制数组</param>
        /// <returns>winform 图像</returns>
        public static Bitmap ToBitmap(this byte[] bytes)
        {
            System.Drawing.Bitmap img = null;
            try
            {
                if (bytes != null && bytes.Length != 0)
                {
                    MemoryStream ms = new MemoryStream(bytes);
                    img = new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return img;
        }
    }
}