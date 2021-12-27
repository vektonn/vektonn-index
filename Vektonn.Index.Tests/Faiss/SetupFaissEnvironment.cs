using NUnit.Framework;
using Vektonn.Index.Faiss;

namespace Vektonn.Index.Tests.Faiss
{
    [SetUpFixture]
    public class SetupFaissEnvironment
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            FaissApi.omp_set_num_threads(1);
        }
    }
}
