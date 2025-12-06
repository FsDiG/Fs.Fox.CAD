using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;

namespace DotNetARX
{
    /// <summary>
    /// 进度条管理类，可以解决常规进度条显示闪烁的问题
    /// </summary>
    public class ProgressManager : IDisposable
    {
        //更新进度条的次数，由于一些特殊原因，你必须把它设置为600
        const int progressMeterIncrements = 600;
        private ProgressMeter pm;//进度条对象
        private long updateIncrement;//进度条需要更新的增量，即更新一次需要多少进度
        private long currentInc;//当前进度

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">进度条的显示信息</param>
        public ProgressManager(string message)
        {
            pm = new ProgressMeter();//新建一个进度条
            pm.Start(message);//进度条开始
            //设置进度条需要更新的次数
            pm.SetLimit(progressMeterIncrements);
            //设置当前进度为0
            currentInc = 0;
        }

        /// <summary>
        /// 程序结束时调用的函数，用以释放资源
        /// </summary>
        public void Dispose()
        {
            pm.Stop();//停止进度条的更新
            pm.Dispose();//销毁进度条对象
        }

        /// <summary>
        /// 设置进度条需要更新，一般为循环次数
        /// </summary>
        /// <param name="totalOps">循环次数</param>
        public void SetTotalOperations(long totalOps)
        {
            // 设置进度条需要更新的增量
            updateIncrement =
              (totalOps > progressMeterIncrements ?
                totalOps / progressMeterIncrements :
                totalOps
              );
        }

        /// <summary>
        /// 当进度增量达到需要更新的量时，则更新进度条
        /// </summary>
        public void Tick()
        {
            //当进度增量达到需要更新的量时，则更新进度条
            if (++currentInc == updateIncrement)
            {
                pm.MeterProgress();//更新进度条
                currentInc = 0;//更新完毕，重置进度增量为0
                //让AutoCAD在长时间任务处理时仍然能接收消息
                System.Windows.Forms.Application.DoEvents();
            }
        }

    }
}
