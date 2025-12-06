using System.Windows.Forms;

namespace DotNetARX
{
    /// <summary>
    /// Windows消息过滤类
    /// </summary>
    public class MessageFilter : IMessageFilter
    {
        /// <summary>
        /// 按键消息
        /// </summary>
        public const int WM_KEYDOWN = 0x0100;
        /// <summary>
        /// 按下的键名
        /// </summary>
        public Keys KeyName { get; set; }

        /// <summary>
        /// 在调度消息之前将其筛选出来
        /// </summary>
        /// <param name="m">消息名</param>
        /// <returns>如果调试的是按键消息，则返回true,否则返回false</returns>
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)//如果调度的消息是按键
            {
                //设置键名
                KeyName = (Keys)(int)m.WParam & Keys.KeyCode;
                //返回true表示调度的是按键消息
                return true;
            }
            return false;//返回false，表示非按键消息
        }
    }
}
