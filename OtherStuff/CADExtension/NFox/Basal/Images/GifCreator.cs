using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NFox.Images
{
    /// <summary>
    ///
    /// </summary>
    public enum Disposal
    {
        None = 0,
        NotDisposal = 4,
        ByBackColor = 8,
        ByPrevious = 12,
    }

    /// <summary>
    ///
    /// </summary>
    public class GifCreator : ImageCreator
    {
        private BinaryWriter m_Writer;

        private byte[] m_GraphicControlExtensionBlock =
            new byte[8]
            {
                (byte)0x21, // 扩展块开始
                (byte)0xF9, // 图像控制扩展块
                (byte)0x04, // 字节数(4)
                (byte)0x00, // 标志位:0-Transparent,1-UserInput,2~4-DisposalMethod,5~7-保留
                (byte)0x00, // 延迟时间低字节
                (byte)0x00, // 延迟时间高字节
                (byte)0x00, // 透明色索引
                (byte)0x00  // 结束
            };

        private byte[] m_ApplicationExtensionBlock =
            new byte[19]
            {
                (byte)0x21, // 扩展块开始
                (byte)0xFF, // 应用程序扩展块
                (byte)0x0B, // 字节数(8+3)
                (byte)'N',
                (byte)'E',
                (byte)'T',
                (byte)'S',
                (byte)'C',
                (byte)'A',
                (byte)'P',
                (byte)'E',
                (byte)'2',
                (byte)'.',
                (byte)'0',
                (byte)0x03, // 循环块字节数(3)
                (byte)0x01, //
                (byte)0x00, // 循环次数低字节
                (byte)0x00, // 循环次数高字节
                (byte)0x00  // 结束
            };

        public GifCreator(string fileName)
        {
            m_FileName = fileName;
        }

        /// <summary>
        /// 循环次数
        /// </summary>
        public short Repeat
        {
            set
            {
                byte[] bits = BitConverter.GetBytes(value);
                m_ApplicationExtensionBlock[16] = bits[0];
                m_ApplicationExtensionBlock[17] = bits[1];
            }
            get
            {
                return BitConverter.ToInt16(m_ApplicationExtensionBlock, 16);
            }
        }

        /// <summary>
        /// 延迟时间
        /// </summary>
        public short Delay
        {
            set
            {
                byte[] bits = BitConverter.GetBytes(value);
                m_GraphicControlExtensionBlock[4] = bits[0];
                m_GraphicControlExtensionBlock[5] = bits[1];
            }
            get
            {
                return BitConverter.ToInt16(m_GraphicControlExtensionBlock, 4);
            }
        }

        /// <summary>
        /// 透明色索引
        /// </summary>
        public byte TransparentColorIndex
        {
            set { m_GraphicControlExtensionBlock[6] = value; }
            get { return m_GraphicControlExtensionBlock[6]; }
        }

        /// <summary>
        /// 透明色标志位
        /// </summary>
        public bool Transparent
        {
            set
            {
                if (value)
                    //00000001
                    m_GraphicControlExtensionBlock[3] |= 1;
                else
                    //11111110
                    m_GraphicControlExtensionBlock[3] &= (byte)0xFE;
            }
            get
            {
                //00000001
                return (m_GraphicControlExtensionBlock[3] & 1) == 1;
            }
        }

        /// <summary>
        /// 用户输入标志位
        /// </summary>
        public bool UserInput
        {
            set
            {
                if (value)
                    //00000010
                    m_GraphicControlExtensionBlock[3] |= 2;
                else
                    //11111101
                    m_GraphicControlExtensionBlock[3] &= (byte)0xFD;
            }
            get
            {
                //00000010
                return (m_GraphicControlExtensionBlock[3] & 2) == 2;
            }
        }

        /// <summary>
        /// 处置方式
        /// </summary>
        public Disposal DisposalMethod
        {
            set
            {
                //11100011
                m_GraphicControlExtensionBlock[3] &= (byte)0xE3;
                m_GraphicControlExtensionBlock[3] |= (byte)value;
            }
            get
            {
                //00011100
                return (Disposal)(m_GraphicControlExtensionBlock[3] & (byte)0x1C);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bitmap"></param>
        public override void AddFrame(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Gif);
            byte[] datas = ms.ToArray();
            if (m_Count == 0)
            {
                m_Writer = new BinaryWriter(File.Open(m_FileName, FileMode.Create));
                m_Writer.Write(datas, 0, 781); //Header & global color table
                m_Writer.Write(m_ApplicationExtensionBlock, 0, 19); //Application extension
            }
            m_Writer.Write(m_GraphicControlExtensionBlock, 0, 8); //Graphic extension
            m_Writer.Write(datas, 789, datas.Length - 790); //Image data
            base.AddFrame(bitmap);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Finish()
        {
            if (m_Writer != null)
            {
                m_Writer.Write((byte)0x3B);  //Image terminator
                m_Writer.Close();
                m_Writer = null;
            }
            base.Finish();
        }
    }
}