using WHampson.Cascara;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public class LayoutVersionTests : CascaraTestFramework
    {
        public LayoutVersionTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData(1, 0, 0,   1, 0, 0,  false, false, true, true, true, 0)]
        [InlineData(1, 0, 1,   1, 0, 1,  false, false, true, true, true, 0)]
        [InlineData(1, 1, 0,   1, 1, 0,  false, false, true, true, true, 0)]
        [InlineData(1, 1, 1,   1, 1, 1,  false, false, true, true, true, 0)]
        [InlineData(1, 0, 1,   1, 0, 0,  false, true, false, true, false, -1)]
        [InlineData(1, 1, 0,   1, 0, 0,  false, true, false, true, false, -1)]
        [InlineData(1, 1, 1,   1, 0, 0,  false, true, false, true, false, -1)]
        [InlineData(2, 0, 0,   1, 0, 0,  false, true, false, true, false, -1)]
        [InlineData(2, 1, 0,   1, 0, 0,  false, true, false, true, false, -1)]
        [InlineData(2, 1, 1,   1, 0, 0,  false, true, false, true, false, -1)]
        [InlineData(1, 0, 0,   1, 0, 1,  true, false, true, false, false, 1)]
        [InlineData(1, 0, 0,   1, 1, 0,  true, false, true, false, false, 1)]
        [InlineData(1, 0, 0,   1, 1, 1,  true, false, true, false, false, 1)]
        [InlineData(1, 0, 0,   2, 0, 0,  true, false, true, false, false, 1)]
        [InlineData(1, 0, 0,   2, 1, 0,  true, false, true, false, false, 1)]
        [InlineData(1, 0, 0,   2, 1, 1,  true, false, true, false, false, 1)]
        public void Comparison(int majorA, int minorA, int patchA, int majorB, int minorB, int patchB,
            bool expectedLT, bool expectedGT, bool expectedLE, bool expectedGE, bool expectedEQ,
            int expectedCompareTo)
        {
            // Arrange
            LayoutVersion verA = new LayoutVersion(majorA, minorA, patchA);
            LayoutVersion verB = new LayoutVersion(majorB, minorB, patchB);

            // Act
            bool lt = verA < verB;
            bool gt = verA > verB;
            bool le = verA <= verB;
            bool ge = verA >= verB;
            bool eq1 = verA == verB;
            bool eq2 = verB == verA;
            bool eq3 = !(verA != verB);
            bool eq4 = !(verB != verA);
            int compareTo = verA.CompareTo(verB);

            // Assert
            Assert.Equal(lt, expectedLT);
            Assert.Equal(gt, expectedGT);
            Assert.Equal(le, expectedLE);
            Assert.Equal(ge, expectedGE);
            Assert.Equal(eq1, expectedEQ);
            Assert.Equal(eq2, expectedEQ);
            Assert.Equal(eq3, expectedEQ);
            Assert.Equal(eq4, expectedEQ);
            Assert.Equal(compareTo, expectedCompareTo);
        }
    }
}
