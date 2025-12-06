using System;
using ThMEPEngineCore.Engine;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThMEPEngineCore
{
    public class ThMEPEngineCoreService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMEPEngineCoreService instance = new ThMEPEngineCoreService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMEPEngineCoreService() { }
        internal ThMEPEngineCoreService() { }
        public static ThMEPEngineCoreService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public ThBuildingElementRecognitionEngine CreateBeamEngine()
        {
            if (Convert.ToInt16(Application.GetSystemVariable("USERR1")) == 0)
            {
                return new ThBeamRecognitionEngine();
            }
            else
            {
                return new ThRawBeamRecognitionEngine();
            }
        }
    }
}
