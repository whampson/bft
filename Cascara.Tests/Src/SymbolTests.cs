using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WHampson.Cascara.Tests
{
    [TestClass]
    public class SymbolTests
    {
        private Symbol root;

        [TestInitialize]
        public void InitializeTests()
        {
            root = Symbol.CreateRootSymbol();
        }

        [TestMethod]
        public void CreateRootSymbol()
        {
            // Expected results
            string expectedName = null;
            string expectedFQName = "";
            Symbol expectedParent = null;
            bool expectedIsCollection = false;
            int expectedElementCount = -1;
            List<string> expectedChildNames = new List<string>();

            // Execution
            Symbol sym = Symbol.CreateRootSymbol();

            // Outputs
            List<string> childNames = sym.GetAllFullyQualifiedNames();

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreEqual(expectedParent, sym.Parent);
            Assert.AreEqual(expectedIsCollection, sym.IsCollection);
            Assert.AreEqual(expectedElementCount, sym.ElementCount);
            Assert.IsTrue(expectedChildNames.Count == childNames.Count
                && expectedChildNames.All(childNames.Contains));
        }

        [TestMethod]
        public void CreateNamelessSymbol()
        {
            // Expected results
            string expectedName = null;
            string expectedFQName = "";
            Symbol expectedParent = root;
            bool expectedIsCollection = false;
            int expectedElementCount = -1;
            List<string> expectedChildNames = new List<string>();

            // Execution
            Symbol sym = Symbol.CreateNamelessSymbol(root);

            // Outputs
            List<string> childNames = sym.GetAllFullyQualifiedNames();

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreEqual(expectedParent, sym.Parent);
            Assert.AreEqual(expectedIsCollection, sym.IsCollection);
            Assert.AreEqual(expectedElementCount, sym.ElementCount);
            Assert.IsTrue(expectedChildNames.Count == childNames.Count
                && expectedChildNames.All(childNames.Contains));
        }

        [TestMethod]
        public void Insert_One()
        {
            // Inputs
            string name = "foo";

            // Expected results (root)
            string rootExpectedName = null;
            string rootExpectedFQName = "";
            Symbol rootExpectedParent = null;
            bool rootExpectedIsCollection = false;
            int rootExpectedElementCount = -1;
            List<string> rootExpectedChildNames = new List<string>()
            {
                "foo"
            };

            // Expected results (sym)
            string symExpectedName = "foo";
            string symExpectedFQName = "foo";
            Symbol symExpectedParent = root;
            bool symExpectedIsCollection = false;
            int symExpectedElementCount = -1;
            List<string> symExpectedChildNames = new List<string>();

            // Execution
            Symbol sym = root.Insert(name);

            // Outputs
            List<string> rootChildNames = root.GetAllFullyQualifiedNames();
            List<string> symChildNames = sym.GetAllFullyQualifiedNames();

            // Assertion (root)
            Assert.AreEqual(rootExpectedName, root.Name);
            Assert.AreEqual(rootExpectedFQName, root.FullyQualifiedName);
            Assert.AreSame(rootExpectedParent, root.Parent);
            Assert.AreEqual(rootExpectedIsCollection, root.IsCollection);
            Assert.AreEqual(rootExpectedElementCount, root.ElementCount);
            Assert.IsTrue(rootExpectedChildNames.Count == rootChildNames.Count
                && rootExpectedChildNames.All(rootChildNames.Contains));

            // Assertion (sym)
            Assert.IsNotNull(sym);
            Assert.AreEqual(symExpectedName, sym.Name);
            Assert.AreEqual(symExpectedFQName, sym.FullyQualifiedName);
            Assert.AreSame(symExpectedParent, sym.Parent);
            Assert.AreEqual(symExpectedIsCollection, sym.IsCollection);
            Assert.AreEqual(symExpectedElementCount, sym.ElementCount);
            Assert.IsTrue(symExpectedChildNames.Count == symChildNames.Count
                && symExpectedChildNames.All(symChildNames.Contains));
        }

        [TestMethod]
        public void Insert_Many()
        {
            // Inputs
            string name1 = "foo";
            string name2 = "bar";
            string name3 = "baz";

            // Expected results (root)
            string rootExpectedName = null;
            string rootExpectedFQName = "";
            Symbol rootExpectedParent = null;
            bool rootExpectedIsCollection = false;
            int rootExpectedElementCount = -1;
            List<string> rootExpectedChildNames = new List<string>()
            {
                "foo",
                "bar",
                "baz"
            };

            // Expected results (sym1)
            string sym1ExpectedName = "foo";
            string sym1ExpectedFQName = "foo";
            Symbol sym1ExpectedParent = root;
            bool sym1ExpectedIsCollection = false;
            int sym1ExpectedElementCount = -1;
            List<string> sym1ExpectedChildNames = new List<string>();

            // Expected results (sym2)
            string sym2ExpectedName = "bar";
            string sym2ExpectedFQName = "bar";
            Symbol sym2ExpectedParent = root;
            bool sym2ExpectedIsCollection = false;
            int sym2ExpectedElementCount = -1;
            List<string> sym2ExpectedChildNames = new List<string>();

            // Expected results (sym3)
            string sym3ExpectedName = "baz";
            string sym3ExpectedFQName = "baz";
            Symbol sym3ExpectedParent = root;
            bool sym3ExpectedIsCollection = false;
            int sym3ExpectedElementCount = -1;
            List<string> sym3ExpectedChildNames = new List<string>();

            // Execution
            Symbol sym1 = root.Insert(name1);
            Symbol sym2 = root.Insert(name2);
            Symbol sym3 = root.Insert(name3);

            // Outputs
            List<string> rootChildNames = root.GetAllFullyQualifiedNames();
            List<string> sym1ChildNames = sym1.GetAllFullyQualifiedNames();
            List<string> sym2ChildNames = sym2.GetAllFullyQualifiedNames();
            List<string> sym3ChildNames = sym3.GetAllFullyQualifiedNames();

            // Assertion (root)
            Assert.AreEqual(rootExpectedName, root.Name);
            Assert.AreEqual(rootExpectedFQName, root.FullyQualifiedName);
            Assert.AreSame(rootExpectedParent, root.Parent);
            Assert.AreEqual(rootExpectedIsCollection, root.IsCollection);
            Assert.AreEqual(rootExpectedElementCount, root.ElementCount);
            Assert.IsTrue(rootExpectedChildNames.Count == rootChildNames.Count
                && rootExpectedChildNames.All(rootChildNames.Contains));

            // Assertion (sym1)
            Assert.IsNotNull(sym1);
            Assert.AreEqual(sym1ExpectedName, sym1.Name);
            Assert.AreEqual(sym1ExpectedFQName, sym1.FullyQualifiedName);
            Assert.AreSame(sym1ExpectedParent, sym1.Parent);
            Assert.AreEqual(sym1ExpectedIsCollection, sym1.IsCollection);
            Assert.AreEqual(sym1ExpectedElementCount, sym1.ElementCount);
            Assert.IsTrue(sym1ExpectedChildNames.Count == sym1ChildNames.Count
                && sym1ExpectedChildNames.All(sym1ChildNames.Contains));

            // Assertion (sym2)
            Assert.IsNotNull(sym2);
            Assert.AreEqual(sym2ExpectedName, sym2.Name);
            Assert.AreEqual(sym2ExpectedFQName, sym2.FullyQualifiedName);
            Assert.AreSame(sym2ExpectedParent, sym2.Parent);
            Assert.AreEqual(sym2ExpectedIsCollection, sym2.IsCollection);
            Assert.AreEqual(sym2ExpectedElementCount, sym2.ElementCount);
            Assert.IsTrue(sym2ExpectedChildNames.Count == sym2ChildNames.Count
                && sym2ExpectedChildNames.All(sym2ChildNames.Contains));

            // Assertion (sym3)
            Assert.IsNotNull(sym3);
            Assert.AreEqual(sym3ExpectedName, sym3.Name);
            Assert.AreEqual(sym3ExpectedFQName, sym3.FullyQualifiedName);
            Assert.AreSame(sym3ExpectedParent, sym3.Parent);
            Assert.AreEqual(sym3ExpectedIsCollection, sym3.IsCollection);
            Assert.AreEqual(sym3ExpectedElementCount, sym3.ElementCount);
            Assert.IsTrue(sym3ExpectedChildNames.Count == sym3ChildNames.Count
                && sym3ExpectedChildNames.All(sym3ChildNames.Contains));
        }

        [TestMethod]
        public void Insert_Chain()
        {
            // Inputs
            string name1 = "abc";
            string name2 = "def";
            string name3 = "ghi";

            // Expected results (root)
            List<string> rootExpectedChildNames = new List<string>()
            {
                "abc",
                "abc.def",
                "abc.def.ghi"
            };

            // Expected results (sym)
            string symExpectedName = "ghi";
            string symExpectedFQName = "abc.def.ghi";
            List<string> symExpectedChildNames = new List<string>();

            // Execution
            Symbol sym = root.Insert(name1).Insert(name2).Insert(name3);

            // Outputs
            List<string> rootChildNames = root.GetAllFullyQualifiedNames();
            List<string> symChildNames = sym.GetAllFullyQualifiedNames();

            // Assertion (root)
            Assert.IsTrue(rootExpectedChildNames.Count == rootChildNames.Count
                && rootExpectedChildNames.All(rootChildNames.Contains));

            // Assertion (sym)
            Assert.AreEqual(symExpectedName, sym.Name);
            Assert.AreEqual(symExpectedFQName, sym.FullyQualifiedName);
            Assert.IsTrue(symExpectedChildNames.Count == symChildNames.Count
                && symExpectedChildNames.All(symChildNames.Contains));
        }

        [TestMethod]
        public void Insert_Duplicate()
        {
            // Inputs
            string name = "xyz";

            // Setup
            root.Insert(name);

            // Execution
            Symbol sym = root.Insert(name);

            // Assertion
            Assert.IsNull(sym);
        }

        [TestMethod]
        public void Insert_InvalidName()
        {
            // Inputs
            string name1 = null;
            string name2 = "";
            string name3 = "   ";
            string name4 = "0abcdef";
            string name5 = "abc def";
            string name6 = "abc-def";
            string name7 = "abc%def";
            string name8 = "!@#$%^&";

            // Execution
            Symbol sym1 = root.Insert(name1);
            Symbol sym2 = root.Insert(name2);
            Symbol sym3 = root.Insert(name3);
            Symbol sym4 = root.Insert(name4);
            Symbol sym5 = root.Insert(name5);
            Symbol sym6 = root.Insert(name6);
            Symbol sym7 = root.Insert(name7);
            Symbol sym8 = root.Insert(name8);

            // Assertion
            Assert.IsNull(sym1);
            Assert.IsNull(sym2);
            Assert.IsNull(sym3);
            Assert.IsNull(sym4);
            Assert.IsNull(sym5);
            Assert.IsNull(sym6);
            Assert.IsNull(sym7);
            Assert.IsNull(sym8);
        }

        [TestMethod]
        public void Insert_Collection()
        {
            // Inputs
            string name = "xyz";
            int count = 3;
            List<string> expectedChildNames = new List<string>()
            {
                "xyz",
                "xyz[0]",
                "xyz[1]",
                "xyz[2]",
            };

            // Expected results
            int expectedCount = count;
            bool expectedIsCollection = true;

            // Execution
            Symbol sym = root.Insert(name, count);

            // Outputs
            List<string> childNames = root.GetAllFullyQualifiedNames();

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreEqual(expectedCount, sym.ElementCount);
            Assert.AreEqual(expectedIsCollection, sym.IsCollection);
            Assert.IsTrue(expectedChildNames.Count == childNames.Count
                && expectedChildNames.All(childNames.Contains));
        }

        [TestMethod]
        public void Insert_IntoCollection()
        {
            // Inputs
            string name1 = "abc";
            string name2 = "def";
            int count = 5;

            // Setup
            Symbol sym1 = root.Insert(name1, count);

            // Expected results
            string expectedName = "def";
            string expectedFQName = "abc[0].def";
            Symbol expectedParent = sym1[0];
            List<string> expectedChildNames = new List<string>()
            {
                "abc",
                "abc[0]",
                "abc[0].def",
                "abc[1]",
                "abc[1].def",
                "abc[2]",
                "abc[2].def",
                "abc[3]",
                "abc[3].def",
                "abc[4]",
                "abc[4].def"
            };

            // Execution
            Symbol sym = sym1.Insert(name2);

            // Outputs
            List<string> childNames = root.GetAllFullyQualifiedNames();

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreSame(expectedParent, sym.Parent);
            Assert.IsTrue(expectedChildNames.Count == childNames.Count
                && expectedChildNames.All(childNames.Contains));
        }

        [TestMethod]
        public void Lookup()
        {
            // Inputs
            string name = "abc";

            // Setup
            root.Insert(name);

            // Expected results
            string expectedName = "abc";
            string expectedFQName = "abc";
            Symbol expectedParent = root;

            // Execution
            Symbol sym = root.Lookup(name);

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreEqual(expectedParent, sym.Parent);
        }

        [TestMethod]
        public void Lookup_Nonexistent()
        {
            // Inputs
            string name = "abc";

            // Execution
            Symbol sym = root.Lookup(name);

            // Assertion
            Assert.IsNull(sym);
        }

        [TestMethod]
        public void Lookup_Leaf()
        {
            // Inputs
            string name1 = "primus";
            string name2 = "really";
            string name3 = "sucks";

            // Setup
            Symbol sym1 = root.Insert(name1);
            Symbol sym2 = sym1.Insert(name2);
            Symbol sym3 = sym2.Insert(name3);

            // Expected results
            string expectedName = "sucks";
            string expectedFQName = "primus.really.sucks";
            Symbol expectedParent = sym2;

            // Execution
            Symbol sym = root.Lookup(expectedFQName);

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreSame(sym3, sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreEqual(expectedParent, sym.Parent);
        }

        [TestMethod]
        public void Lookup_Root()
        {
            // Inputs
            string name1 = "john";
            string name2 = "the";
            string name3 = "fisherman";

            // Setup
            Symbol sym1 = root.Insert(name1);
            Symbol sym2 = sym1.Insert(name2);
            Symbol sym3 = sym2.Insert(name3);

            // Expected results
            string expectedName = "john";
            string expectedFQName = "john";
            Symbol expectedParent = root;

            // Execution
            Symbol sym = sym3.Lookup(name1);

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreSame(sym1, sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreEqual(expectedParent, sym.Parent);
        }

        [TestMethod]
        public void Lookup_Self()
        {
            // Inputs
            string name1 = "tommy";
            string name2 = "the";
            string name3 = "cat";

            // Setup
            Symbol sym1 = root.Insert(name1);
            Symbol sym2 = sym1.Insert(name2);
            Symbol sym3 = sym2.Insert(name3);

            // Expected results
            string expectedName = "cat";
            string expectedFQName = "tommy.the.cat";
            Symbol expectedParent = sym2;

            // Execution
            Symbol sym = sym3.Lookup(expectedFQName);

            // Assertion
            Assert.IsNotNull(sym);
            Assert.AreSame(sym3, sym);
            Assert.AreEqual(expectedName, sym.Name);
            Assert.AreEqual(expectedFQName, sym.FullyQualifiedName);
            Assert.AreEqual(expectedParent, sym.Parent);
        }

        [TestMethod]
        public void IsLeaf()
        {
            // Inputs
            string name1 = "fruit";
            string name2 = "pear";

            // Execution
            Symbol notLeaf = root.Insert(name1);
            Symbol leaf = notLeaf.Insert(name2);

            // Assertion
            Assert.IsFalse(notLeaf.IsLeaf);
            Assert.IsTrue(leaf.IsLeaf);
        }

        //[TestMethod]
        //public void Equals_Self()
        //{
        //    // Inputs
        //    string name = "someVar";

        //    // Setup
        //    Symbol sym = root.Insert(name);

        //    // Expected results
        //    bool expectedEquality = true;

        //    // Execution
        //    bool equality = sym.Equals(sym);

        //    // Assertion
        //    Assert.AreEqual(equality, expectedEquality);
        //}
    }
}
