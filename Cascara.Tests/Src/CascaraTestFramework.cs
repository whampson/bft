using Xunit.Abstractions;

namespace Cascara.Tests
{
    public abstract class CascaraTestFramework
    {
        public CascaraTestFramework(ITestOutputHelper output)
        {
            Output = output;
        }

        protected ITestOutputHelper Output
        {
            get;
        }
    }
}
