using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

namespace Autodesk.AutoCAD.EditorInput
{
    /// <summary>
    ///
    /// </summary>
    public class AllowedClassFilter
    {
        /// <summary>
        /// The PTRS
        /// </summary>
        private HashSet<IntPtr> ptrs = new HashSet<IntPtr>();

        /// <summary>
        /// Gets the allowed class PTRS.
        /// </summary>
        /// <value>
        /// The allowed class PTRS.
        /// </value>
        internal ICollection<IntPtr> AllowedClassPtrs { get { return ptrs; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedClassFilter"/> class.
        /// </summary>
        /// <param name="types">The types.</param>
        public AllowedClassFilter(params Type[] types)
        {
            foreach (Type typ in types)
            {
                AddAllowedClass(typ);
            }
        }

        /// <summary>
        /// Adds the allowed class.
        /// </summary>
        /// <param name="type">The type.</param>
        public void AddAllowedClass(Type type)
        {
            ptrs.Add(RXClass.GetClass(type).UnmanagedObject);
        }
    }
}