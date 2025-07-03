namespace Autodesk.AutoCAD.DatabaseServices
{
    /// <summary>
    /// Extension Method class for AnnotationScales
    /// </summary>
    public static class AnnotationScaleExtensions
    {
        /// <summary>
        /// Returns Scale Factor(1.0 / AnnotationScale.Scale)
        /// </summary>
        /// <param name="annoScale"></param>
        /// <returns>doubl that equals (1.0 / AnnotationScale.Scale)</returns>
        public static double GetScaleFactor(this AnnotationScale annoScale)
        {
            return (1.0 / annoScale.Scale);
        }
    }
}