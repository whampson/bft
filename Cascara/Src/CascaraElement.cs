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

            return new DataTypeElement(name, null, type, modifiers);
        }

        public static CascaraElement CreateDataTypeAlias(string name, string target)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new DataTypeElement(name, target, null);
        }

        protected CascaraElement(string name, string aliasTarget, params CascaraModifier[] modifiers)
        {
            Name = name;
            AliasTarget = aliasTarget;
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

        public bool IsAlias
        {
            get { return !string.IsNullOrWhiteSpace(AliasTarget) && Modifiers.Count() == 0; }
        }

        public string AliasTarget
        {
            get;
        }
    }

    internal sealed class DataTypeElement : CascaraElement
    {
        public DataTypeElement(string name, string aliasTarget, CascaraType type, params CascaraModifier[] modifiers)
            : base(name, aliasTarget, modifiers)
        {
            Type = type;
        }

        public CascaraType Type
        {
            get;
        }

        public override string ToString()
        {
            return string.Format("DirectiveElement: [ Name: {0}, ModifierCount: {1}, Type: {2}, IsAlias: {3}, AliasTarget: {4} ]",
                Name, Modifiers.Count(), Type, IsAlias, AliasTarget);
        }
    }

    internal sealed class DirectiveElement : CascaraElement
    {
        public DirectiveElement(string name, params CascaraModifier[] modifiers)
            : base(name, null, modifiers)
        {
        }

        public override string ToString()
        {
            return string.Format("DirectiveElement: [ Name: {0}, ModifierCount: {1} ]",
                Name, Modifiers.Count());
        }
    }
}
