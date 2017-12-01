#region License
/* Copyright (c) 2017 Wes Hampson
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WHampson.Cascara
{
    internal sealed class LayoutFileProcessor2
    {
        private const string IdentifierPattern = @"^[a-zA-Z_][\da-zA-Z_]*$";    // consider using built-in function in Symbol
        private const string ValueofOpPattern = @"\${(.+?)}";
        private const string OffsetofOpPattern = @"\$\[([\[\]\S]+)\]";
        private const string SizeofOpPattern = @"\$\((.+?)\)";
        private const string TypeOpPattern = @"type[ ]+(.+)";

        private delegate int DirectiveProcessAction(XElement elem);
        private delegate T AttributeProcessAction<T>(XAttribute attr);

        private IntPtr dataPtr;
        private int dataLength;
        private int currentDataOffset;

        private TextWriter echoWriter;

        //private bool isEvaluatingUnion;
        private bool isEvalutingTypedef;
        private bool isConductingDryRun;
        private int dryRunRecursionDepth;

        //private Dictionary<string, TypeDefinition> typeMap;
        private Dictionary<string, DirectiveProcessAction> directiveActionMap;
        private Dictionary<string, Delegate> attributeActionMap;
        private Dictionary<string, double> localsMap;

        private Stack<Symbol> symbolStack;
        private Stack<BinaryLayout> layoutFileStack;

        public static Symbol Process(BinaryLayout layout, IntPtr dataPtr, int dataLength, TextWriter echoWriter)
        {
            return new LayoutFileProcessor2(dataPtr, dataLength, echoWriter).Process(layout);
        }

        private LayoutFileProcessor2(IntPtr dataPtr, int dataLength, TextWriter echoWriter)
        {
            // TODO: validate params

            this.dataPtr = dataPtr;
            this.dataLength = dataLength;
            currentDataOffset = 0;

            this.echoWriter = echoWriter;

            isEvalutingTypedef = false;
            isConductingDryRun = false;
            dryRunRecursionDepth = 0;

            directiveActionMap = new Dictionary<string, DirectiveProcessAction>();
            attributeActionMap = new Dictionary<string, Delegate>();
            localsMap = new Dictionary<string, double>();

            symbolStack = new Stack<Symbol>();
            layoutFileStack = new Stack<BinaryLayout>();

            symbolStack.Push(Symbol.CreateRootSymbol());

            //BuildTypeMap();
            BuildDirectiveActionMap();
            BuildAttributeActionMap();
        }

        private Symbol CurrentSymbol
        {
            get { return symbolStack.Peek(); }
        }

        private BinaryLayout CurrentLayout
        {
            get { return layoutFileStack.Peek(); }
        }

        private Symbol Process(BinaryLayout layout)
        {
            // Prevent 'include' cycles
            if (layoutFileStack.Contains(layout))
            {
                //throw new LayoutException(layout, "Layout File inclusion cycle detected.");
            }

            //layoutFileStack.Push(layoutFile);

            //int bytesProcessed = ProcessStructMembers(layoutFile.Document.Root);
            //layoutFileStack.Pop();

            //return bytesProcessed;

            return default(Symbol);
        }

        private int ProcessAlignDirective(XElement elem) { return default(int); }
        private int ProcessEchoDirective(XElement elem) { return default(int); }
        private int ProcessIncludeDirective(XElement elem) { return default(int); }
        private int ProcessLocalDirective(XElement elem) { return default(int); }
        private int ProcessTypedefDirective(XElement elem) { return default(int); }

        private int ProcessCountAttribute(XAttribute attr) { return default(int); }
        private TypeDefinition ProcessKindAttribute(XAttribute attr) { return default(TypeDefinition); }
        private string ProcessMessageAttribute(XAttribute attr) { return default(string); }
        private string ProcessNameAttribute(XAttribute attr) { return default(string); }
        private bool ProcessNewlineAttribute(XAttribute attr) { return default(bool); }
        private string ProcessPathAttribute(XAttribute attr) { return default(string); }
        private bool ProcessRawAttribute(XAttribute attr) { return default(bool); }
        private double ProcessValueAttribute(XAttribute attr) { return default(double); }
        private int ProcessWidthAttribute(XAttribute attr) { return default(int); }

        /// <summary>
        /// Populates the map of directive names to directive processing functions.
        /// </summary>
        private void BuildDirectiveActionMap()
        {
            directiveActionMap[Keywords.Align] = ProcessAlignDirective;
            directiveActionMap[Keywords.Echo] = ProcessEchoDirective;
            directiveActionMap[Keywords.Include] = ProcessIncludeDirective;
            directiveActionMap[Keywords.Local] = ProcessLocalDirective;
            directiveActionMap[Keywords.Typedef] = ProcessTypedefDirective;
        }

        /// <summary>
        /// Populates the map of attribute names to attribute processing functions.
        /// </summary>
        private void BuildAttributeActionMap()
        {
            attributeActionMap[Keywords.Count] = (AttributeProcessAction<int>) ProcessCountAttribute;
            attributeActionMap[Keywords.Kind] = (AttributeProcessAction<TypeDefinition>) ProcessKindAttribute;
            attributeActionMap[Keywords.Message] = (AttributeProcessAction<string>) ProcessMessageAttribute;
            attributeActionMap[Keywords.Name] = (AttributeProcessAction<string>) ProcessNameAttribute;
            attributeActionMap[Keywords.Newline] = (AttributeProcessAction<bool>) ProcessNewlineAttribute;
            attributeActionMap[Keywords.Path] = (AttributeProcessAction<string>) ProcessPathAttribute;
            attributeActionMap[Keywords.Raw] = (AttributeProcessAction<bool>) ProcessRawAttribute;
            attributeActionMap[Keywords.Value] = (AttributeProcessAction<double>) ProcessValueAttribute;
            attributeActionMap[Keywords.Width] = (AttributeProcessAction<int>) ProcessWidthAttribute;
        }
    }
}
