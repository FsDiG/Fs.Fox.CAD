using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ThCADExtension
{
    public static class InvokeTool
    {
        public struct ads_name
        {
            IntPtr a;
            IntPtr b;
        };

#if ACAD2012
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        public static extern int acdbGetAdsName(ref ads_name name, ObjectId objId);
#elif ACAD2014
        [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        public static extern int acdbGetAdsName(ref ads_name name, ObjectId objId);
#elif ACAD2016
        [DllImport("acdb20.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        public static extern int acdbGetAdsName(ref ads_name name, ObjectId objId);
#elif ACAD2018
        [DllImport("acdb22.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        public static extern int acdbGetAdsName(ref ads_name name, ObjectId objId);
#endif

#if ACAD2012
        [DllImport("acad.exe", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acdbEntGet")]
        public static extern System.IntPtr acdbEntGet(ref ads_name ename);
#else
        [DllImport("accore.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acdbEntGet")]
        public static extern System.IntPtr acdbEntGet(ref ads_name ename);
#endif

#if ACAD2012
        [DllImport("acad.exe", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTextBox")]
        public static extern System.IntPtr acedTextBox(IntPtr rb, double[] point1, double[] point2);
#else
        [DllImport("accore.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTextBox")]
        public static extern System.IntPtr acedTextBox(IntPtr rb, double[] point1, double[] point2);
#endif

        public static System.Collections.Generic.List<object> acdbEntGetObjects(this
            ObjectId id, short dxfcode)
        {
            System.Collections.Generic.List<object> result = new System.Collections.Generic.List<object>();

            ads_name name = new ads_name();
            int res = acdbGetAdsName(ref name, id);

            ResultBuffer rb = new ResultBuffer();
            Autodesk.AutoCAD.Runtime.Interop.AttachUnmanagedObject(
                rb, acdbEntGet(ref name), true);

            ResultBufferEnumerator iter = rb.GetEnumerator();

            while (iter.MoveNext())
            {
                TypedValue typedValue = (TypedValue)iter.Current;
                if (typedValue.TypeCode == dxfcode)
                {
                    result.Add(typedValue.Value);
                }

            }
            return result;

        }

        public static System.Collections.Generic.List<TypedValue>
            acdbEntGetTypedVals(this ObjectId id)
        {
            System.Collections.Generic.List<TypedValue> result = new System.Collections.Generic.List<TypedValue>();

            ads_name name = new ads_name();

            int res = acdbGetAdsName(ref name, id);

            ResultBuffer rb = new ResultBuffer();

            Autodesk.AutoCAD.Runtime.Interop.AttachUnmanagedObject(rb, acdbEntGet(ref name), true);

            ResultBufferEnumerator iter = rb.GetEnumerator();

            while (iter.MoveNext())
            {
                result.Add((TypedValue)iter.Current);
            }

            return result;

        }
    }
}
