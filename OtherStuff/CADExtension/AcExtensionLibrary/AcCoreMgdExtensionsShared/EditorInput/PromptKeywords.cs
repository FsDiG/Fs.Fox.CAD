using System;

namespace Autodesk.AutoCAD.EditorInput
{
    /// <summary>
    ///
    /// </summary>
    public class PromptKeywords
    {
        /// <summary>
        /// The seperator
        /// </summary>
        private static char[] seperator = new char[] { '/' };

        /// <summary>
        /// Sets the keywords.
        /// </summary>
        /// <param name="meassageAndKeywords">The meassage and keywords.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">meassageAndKeywords is empty, whitespaces, or null</exception>
        /// <exception cref="System.ArgumentException">
        /// meassageAndKeywords must contain \[\
        /// or
        /// meassageAndKeywords must contain \]\
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">\[\ must be before \]\ </exception>
        public static PromptKeywordOptions SetKeywords(string meassageAndKeywords)
        {
            if (String.IsNullOrWhiteSpace(meassageAndKeywords))
            {
                throw new ArgumentNullException("meassageAndKeywords is empty, whitespaces, or null");
            }

            int startOfKeywordsIndex = meassageAndKeywords.IndexOf("[");
            if (startOfKeywordsIndex == -1)
            {
                throw new ArgumentException("meassageAndKeywords must contain \"[\"");
            }

            int endOfKeywordsIndex = meassageAndKeywords.IndexOf("]");
            if (endOfKeywordsIndex == -1)
            {
                throw new ArgumentException("meassageAndKeywords must contain \"]\"");
            }

            if (startOfKeywordsIndex > endOfKeywordsIndex)
            {
                throw new ArgumentOutOfRangeException("\"[\" must be before \"]\" ");
            }

            string keywords = meassageAndKeywords.Substring(startOfKeywordsIndex + 1, endOfKeywordsIndex - startOfKeywordsIndex - 1);
            string[] keywordsArray = keywords.Split(seperator);
            keywords = String.Join(" ", keywordsArray);
            return new PromptKeywordOptions(meassageAndKeywords, keywords);
        }

        /// <summary>
        /// Sets the keywords.
        /// </summary>
        /// <param name="meassageAndKeywords">The meassage and keywords.</param>
        /// <param name="defaultKeyword">The default keyword.</param>
        /// <returns></returns>
        public static PromptKeywordOptions SetKeywords(string meassageAndKeywords, string defaultKeyword)
        {
            PromptKeywordOptions pko = SetKeywords(meassageAndKeywords);
            pko.Keywords.Default = defaultKeyword;
            return pko;
        }
    }
}