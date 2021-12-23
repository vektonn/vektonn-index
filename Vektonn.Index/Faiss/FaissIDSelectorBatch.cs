using System;

namespace Vektonn.Index.Faiss
{
    internal class FaissIdSelectorBatch : IDisposable
    {
        public readonly IntPtr Ptr = IntPtr.Zero;

        public FaissIdSelectorBatch(long[] ids)
        {
            FaissApi.faiss_IDSelectorBatch_new(ref Ptr, ids.Length, ids);
        }

        public void Dispose()
        {
            FaissApi.faiss_IDSelector_free(Ptr);
        }
    }
}
