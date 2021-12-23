using System;
using static Vektonn.Index.Faiss.FaissApi;

namespace Vektonn.Index.Faiss
{
    internal class FaissParameterSpace : IDisposable
    {
        private readonly IntPtr ptr;

        public FaissParameterSpace()
        {
            faiss_ParameterSpace_new(ref ptr).ThrowOnFaissError();
        }

        public void Dispose()
        {
            faiss_ParameterSpace_free(ptr);
        }

        public void SetIndexParameter(FaissIndex faissIndex, string paramName, double paramValue)
        {
            faiss_ParameterSpace_set_index_parameter(ptr, faissIndex.IndexPtr, paramName, paramValue).ThrowOnFaissError();
        }

        public double GetIndexParameter(FaissIndex faissIndex, string paramName)
        {
            double paramValue = 0;
            faiss_ParameterSpace_get_index_parameter(ptr, faissIndex.IndexPtr, paramName, ref paramValue).ThrowOnFaissError();
            return paramValue;
        }
    }
}
