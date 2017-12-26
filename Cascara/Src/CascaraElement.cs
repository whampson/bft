using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WHampson.Cascara
{
    internal abstract class CascaraElement
    {
        public static CascaraElement CreateDirective(string name, params CascaraModifier[] modifiers)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new DirectiveElement(name, modifiers);
        }

        public static CascaraElement CreateDataType(string name, CascaraType type, params CascaraModifier[] modifiers)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new DataTypeElement(name, type, modifiers);
        }

        protected CascaraElement(string name, params CascaraModifier[] modifiers)
        {
            Name = name;
            Modifiers = new List<CascaraModifier>(modifiers);
        }

        public string Name
        {
            get;
        }

        public IEnumerable<CascaraModifier> Modifiers
        {
            get;
        }
    }

    internal sealed class DataTypeElement : CascaraElement
    {
        public DataTypeElement(string name, CascaraType type, params CascaraModifier[] modifiers)
            : base(name, modifiers)
        {
            Type = type;
        }

        public CascaraType Type
        {
            get;
        }

        public override string ToString()
        {
            return string.Format("DataTypeElement: [ Name: {0}, ModifierCount: {1}, Type: {2} ]",
                Name, Modifiers.Count(), Type);
        }
    }

    internal sealed class DirectiveElement : CascaraElement
    {
        public DirectiveElement(string name, params CascaraModifier[] modifiers)
            : base(name, modifiers)
        {
        }

        public override string ToString()
        {
            return string.Format("DirectiveElement: [ Name: {0}, ModifierCount: {1} ]",
                Name, Modifiers.Count());
        }
    }
}
