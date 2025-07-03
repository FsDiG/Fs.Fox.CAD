using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NFox.Images
{
    public class TiffCreator : ImageCreator
    {
        private Bitmap m_Tiff;
        private EncoderParameters m_Params = new EncoderParameters(2);

        public TiffCreator(string fileName)
        {
            m_Params.Param[0] =
                new EncoderParameter(
                   System.Drawing.Imaging.Encoder.Compression,
                   Convert.ToInt32(EncoderValue.CompressionLZW));
            m_FileName = fileName;
        }

        public override void AddFrame(Bitmap bitmap)
        {
            if (m_Count == 0)
            {
                m_Params.Param[1] =
                    new EncoderParameter(
                        System.Drawing.Imaging.Encoder.SaveFlag,
                        Convert.ToInt32(EncoderValue.MultiFrame));
                m_Tiff = (Bitmap)bitmap.Clone();
                m_Tiff.Save(m_FileName, GetCodecInfo("image/tiff"), m_Params);
            }
            else
            {
                m_Params.Param[1] =
                    new EncoderParameter(
                        System.Drawing.Imaging.Encoder.SaveFlag,
                        Convert.ToInt32(EncoderValue.FrameDimensionPage));
                m_Tiff.SaveAdd(bitmap, m_Params);
            }
            base.AddFrame(bitmap);
        }

        public override void Finish()
        {
            if (m_Tiff != null)
            {
                m_Params.Param[1] =
                    new EncoderParameter(
                        System.Drawing.Imaging.Encoder.SaveFlag,
                        Convert.ToInt32(EncoderValue.Flush));
                m_Tiff.SaveAdd(m_Params);
                m_Tiff.Dispose();
                m_Tiff = null;
            }
            base.Finish();
        }
    }
}