using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WHampson.Cascara.Extensions;

namespace WHampson.Cascara.Interpreter.Xml
{
    /// <summary>
    /// Encapsulates an <see cref="XObject"/> as an <see cref="ISourceEntity"/>.
    /// </summary>
    internal sealed class XmlSourceEntity : ISourceEntity
    {
        public XmlSourceEntity(XObject sourceObject)
        {
            if (sourceObject == null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            SourceObject = sourceObject;
        }

        public XObject SourceObject
        {
            get;
        }

        int ISourceEntity.LineNumber
        {
            get { return SourceObject.GetLineInfo().Item1; }
        }

        int ISourceEntity.LinePosition
        {
            get { return SourceObject.GetLineInfo().Item2; }
        }

        public override string ToString()
        {
            return string.Format("{0}: [ {1} = {2} ]",
                GetType().Name,
                nameof(SourceObject), SourceObject);
        }
    }
}
