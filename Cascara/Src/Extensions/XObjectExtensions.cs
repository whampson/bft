using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WHampson.Cascara.Extensions
{
    internal static class XObjectExtensions
    {
        /// <summary>
        /// Extracts source code line information from the specified <see cref="XElement"/>.
        /// </summary>
        /// <param name="node">The node to extract line info from.</param>
        /// <returns>
        /// An integer pair representing the text coordinates of the statement in the source code.
        /// 'Item1' is the line number; 'Item2' is the column number.
        /// </returns>
        public static Tuple<int, int> GetLineInfo(this XObject node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            IXmlLineInfo lineInfo = (IXmlLineInfo) node;

            return new Tuple<int, int>(lineInfo.LineNumber, lineInfo.LinePosition);
        }
    }
}
