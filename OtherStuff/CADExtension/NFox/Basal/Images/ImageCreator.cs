using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace NFox.Images
{
    public abstract class ImageCreator
    {
        protected string m_FileName;
        protected int m_Count;

        /// <summary>
        ///
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        ///
        /// </summary>
        public string FileName
        {
            set { m_FileName = value; }
            get { return m_FileName; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bitmap"></param>
        public virtual void AddFrame(Bitmap bitmap)
        {
            m_Count++;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bitmaps"></param>
        public void AddFrame(List<Bitmap> bitmaps)
        {
            for (int i = 0; i < bitmaps.Count; i++)
            {
                AddFrame(bitmaps[i]);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void Finish()
        {
            m_Count = 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public ImageCodecInfo GetCodecInfo(string mimeType)
        {
            foreach (ImageCodecInfo ici in ImageCodecInfo.GetImageDecoders())
            {
                if (ici.MimeType == mimeType)
                {
                    return ici;
                }
            }
            return null;
        }
    }
}