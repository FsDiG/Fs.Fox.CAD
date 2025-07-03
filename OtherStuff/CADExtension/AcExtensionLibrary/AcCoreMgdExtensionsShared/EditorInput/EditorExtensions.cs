using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Autodesk.AutoCAD.EditorInput
{
    /// <summary>
    ///
    /// </summary>
    public static class EditorExtensions
    {
        /// <summary>
        /// Private field holding the Environment's newline ("/r/n")
        /// </summary>
        private static string newLine = Environment.NewLine;

        /// <summary>
        /// Private field holding IFormatProvider
        /// </summary>
        private static IFormatProvider _formatProvider;

        /// <summary>
        /// Property to get current IFormatProvider
        /// </summary>
        /// <value>
        /// The format provider.
        /// </value>
        private static IFormatProvider formatProvider
        {
            get
            {
                if (_formatProvider == null)
                {
                    _formatProvider = Thread.CurrentThread.CurrentCulture;
                }
                return _formatProvider;
            }
        }

        /// <summary>
        /// Writes the specified subarray of Unicode characters to the CommandLine
        /// </summary>
        /// <param name="ed">Extension method of Editor</param>
        /// <param name="buffer">An array of Unicode characters.</param>
        /// <param name="index">The starting position in buffer.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if buffer is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if index is less than zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if index plus count specify a position that is not within buffer.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if index is less than zero.</exception>
        public static void Write(this Editor ed, char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException("CreateBuffer Invalid Length");
            }
            for (int i = 0; i < count; i++)
            {
                ed.Write(buffer[index + i]);
            }
        }

        /// <summary>
        /// Writes the specified subarray of Unicode characters to the CommandLine
        /// </summary>
        /// <param name="ed">Extension method of Editor</param>
        /// <param name="buffer">A Unicode character array.</param>
        public static void Write(this Editor ed, char[] buffer)
        {
            ed.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes the text representation of the specified Boolean val to the CommandLine
        /// </summary>
        /// <param name="ed">Extension method of Editor</param>
        /// <param name="value">The val to write.</param>
        public static void Write(this Editor ed, bool value)
        {
            ed.WriteMessage(value ? "True" : "False");
        }

        /// <summary>
        /// Writes the specified Unicode character val to the CommandLine
        /// </summary>
        /// <param name="ed">Extension method of Editor</param>
        /// <param name="value">The val to write.</param>
        public static void Write(this Editor ed, char value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, decimal value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, double value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, int value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, Int64 value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, Point2d value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, Point3d value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, object value)
        {
            if (value != null)
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    ed.WriteMessage(formattable.ToString(null, formatProvider));
                }
                else
                {
                    ed.Write(value.ToString());
                }
            }
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, Single value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                ed.WriteMessage(value);
            }
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, IEnumerable<string> value)
        {
            if (value != null)
            {
                foreach (string s in value)
                {
                    ed.WriteMessage(s);
                }
            }
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, IEnumerable<object> value)
        {
            if (value != null)
            {
                foreach (object obj in value)
                {
                    ed.Write(obj);
                }
            }
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void Write(this Editor ed, ObjectId value)
        {
            ed.WriteMessage(value.ToString(formatProvider));
        }

        /// <summary>
        /// Writes the specified format.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void Write(this Editor ed, string format, object arg0)
        {
            ed.WriteMessage(string.Format(formatProvider, format, new object[] { arg0 }));
        }

        /// <summary>
        /// Writes the specified format.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg">The argument.</param>
        public static void Write(this Editor ed, string format, params object[] arg)
        {
            ed.WriteMessage(string.Format(formatProvider, format, arg));
        }

        /// <summary>
        /// Writes the specified format.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void Write(this Editor ed, string format, object arg0, object arg1)
        {
            ed.WriteMessage(string.Format(formatProvider, format, new object[] { arg0, arg1 }));
        }

        /// <summary>
        /// Writes the specified format.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void Write(this Editor ed, string format, object arg0, object arg1, object arg2)
        {
            ed.WriteMessage(string.Format(formatProvider, format, new object[] { arg0, arg1, arg2 }));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        public static void WriteLine(this Editor ed)
        {
            ed.WriteMessage(newLine);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public static void WriteLine(this Editor ed, char[] buffer, int index, int count)
        {
            ed.Write(buffer, index, count);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="buffer">The buffer.</param>
        public static void WriteLine(this Editor ed, char[] buffer)
        {
            ed.Write(buffer);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void WriteLine(this Editor ed, bool value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, char value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, decimal value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, double value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, int value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, Int64 value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, Point2d value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, Point3d value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, object value)
        {
            if (value == null)
            {
                ed.WriteLine();
            }
            else
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    ed.Write(formattable.ToString(null, formatProvider));
                }
                else
                {
                    ed.WriteMessage(value.ToString());
                }
                ed.WriteLine();
            }
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, Single value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, string value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, ObjectId value)
        {
            ed.Write(value);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void WriteLine(this Editor ed, string format, object arg0)
        {
            ed.Write(format, arg0);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg">The argument.</param>
        public static void WriteLine(this Editor ed, string format, params object[] arg)
        {
            ed.Write(format, arg);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void WriteLine(this Editor ed, string format, object arg0, object arg1)
        {
            ed.Write(format, arg0, arg1);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void WriteLine(this Editor ed, string format, object arg0, object arg1, object arg2)
        {
            ed.Write(format, arg0, arg1, arg2);
            ed.WriteLine();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, IEnumerable<string> value)
        {
            if (value != null)
            {
                foreach (string s in value)
                {
                    ed.WriteLine(s);
                }
            }
            else
            {
                ed.WriteLine();
            }
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="value">The value.</param>
        public static void WriteLine(this Editor ed, IEnumerable<object> value)
        {
            if (value != null)
            {
                foreach (object obj in value)
                {
                    ed.WriteLine(obj);
                }
            }
            else
            {
                ed.WriteLine();
            }
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ed">The ed.</param>
        /// <param name="items">The items.</param>
        public static void WriteLine<T>(this Editor ed, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ed.WriteLine(item);
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="options">The options.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <returns></returns>
        public static PromptSelectionResult GetSelection(this Editor ed, PromptSelectionOptions options, AllowedClassFilter classFilter)
        {
            return ed.GetSelection((PromptSelectionOptions)null, classFilter);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <returns></returns>
        public static PromptSelectionResult GetSelection(this Editor ed, SelectionFilter filter, AllowedClassFilter classFilter)
        {
            return ed.GetSelection((SelectionFilter)null, classFilter);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <returns></returns>
        public static PromptSelectionResult GetSelection(this Editor ed, AllowedClassFilter classFilter)
        {
            return ed.GetSelection((PromptSelectionOptions)null, (SelectionFilter)null, classFilter);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="ed">The ed.</param>
        /// <param name="options">The options.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <returns></returns>
        public static PromptSelectionResult GetSelection(this Editor ed, PromptSelectionOptions options, SelectionFilter filter, AllowedClassFilter classFilter)
        {
            AllowedClassPtrs = classFilter.AllowedClassPtrs;
            ed.SelectionAdded += new SelectionAddedEventHandler(ed_ClassFilterSelectionAdded);

            try
            {
                if (options != null)
                {
                    if (filter != null)
                    {
                        return ed.GetSelection(options, filter);
                    }
                    else
                    {
                        return ed.GetSelection(options);
                    }
                }
                else if (filter != null)
                {
                    return ed.GetSelection(filter);
                }
                else
                {
                    return ed.GetSelection();
                }
            }
            finally
            {
                ed.SelectionAdded -= new SelectionAddedEventHandler(ed_ClassFilterSelectionAdded);
            }
        }

        /// <summary>
        /// The allowed class PTRS
        /// </summary>
        private static ICollection<IntPtr> AllowedClassPtrs;

        /// <summary>
        /// Handles the ClassFilterSelectionAdded event of the ed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionAddedEventArgs"/> instance containing the event data.</param>
        private static void ed_ClassFilterSelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            ObjectId[] ids = e.AddedObjects.GetObjectIds();
            for (int i = 0; i < ids.Length; i++)
            {
                if (!AllowedClassPtrs.Contains(ids[i].ObjectClass.UnmanagedObject))
                    e.Remove(i);
            }
        }
    }
}