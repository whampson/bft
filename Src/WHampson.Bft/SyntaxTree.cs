//#region License
///* Copyright (c) 2017 Wes Hampson
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in all
// * copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// * SOFTWARE.
// */
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static WHampson.BFT.Keyword;

//namespace WHampson.BFT
//{
//    class SyntaxTree
//    {
//        public SyntaxTree(ISyntaxTreeNode root)
//        {
//            Root = root;
//        }

//        public ISyntaxTreeNode Root { get; }
//    }

//    internal interface ISyntaxTreeNode
//    {
//        Dictionary<Modifier, string> Modifiers { get; }
//        List<ISyntaxTreeNode> Children { get; }
//    }

//    internal class DataTypeTreeNode : ISyntaxTreeNode
//    {
//        public DataTypeTreeNode(BuiltinType type, Dictionary<Modifier, string> modifiers)
//        {
//            Type = type;
//            Modifiers = modifiers;
//            Children = new List<ISyntaxTreeNode>();
//        }

//        public BuiltinType Type { get; }
//        public Dictionary<Modifier, string> Modifiers { get; }
//        public List<ISyntaxTreeNode> Children { get; }

//        public override string ToString()
//        {
//            string str = "Type = " + Type + "; ";
//            str += "Modifiers = { ";
//            foreach (KeyValuePair<Modifier, string> modPair in Modifiers)
//            {
//                str += modPair.Key + " => " + modPair.Value + ", ";
//            }
//            str += "}; ";
//            str += "Children = " + Children.Count + ";";

//            return str;
//        }
//    }

//    internal class DirectiveTreeNode : ISyntaxTreeNode
//    {
//        public DirectiveTreeNode(Directive directive, Dictionary<Modifier, string> modifiers)
//        {
//            Directive = directive;
//            Modifiers = modifiers;
//            Children = new List<ISyntaxTreeNode>();
//        }

//        public Directive Directive { get; }
//        public Dictionary<Modifier, string> Modifiers { get; }
//        public List<ISyntaxTreeNode> Children { get; }

//        public override string ToString()
//        {
//            string str = "Directive = " + Directive + "; ";
//            str += "Modifiers = { ";
//            foreach (KeyValuePair<Modifier, string> modPair in Modifiers)
//            {
//                str += modPair.Key + " => " + modPair.Value + ", ";
//            }
//            str += "}; ";
//            str += "Children = " + Children.Count + ";";

//            return str;
//        }
//    }
//}
