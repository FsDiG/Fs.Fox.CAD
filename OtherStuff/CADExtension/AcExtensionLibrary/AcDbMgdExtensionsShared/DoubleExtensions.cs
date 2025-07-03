using System;
using System.Collections.Generic;
using System.Text;

namespace Autodesk.AutoCAD.ApplicationServices
{
   /// <summary>
   /// 
   /// </summary>
   public static class DoubleExtensions
    {
       /// <summary>
       /// 
       /// </summary>
       /// <param name="angle"></param>
       /// <returns></returns>
       public static double ConvertToRadians(this double angle)
       {
           return angle * Math.PI / 180.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double ConvertToAngle(this double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
