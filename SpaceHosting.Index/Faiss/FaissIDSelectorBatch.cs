using System;

namespace SpaceHosting.Index.Faiss
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
            // todo: fix Faiss exports, IDSelector_free -> IDSelectorBatch_free
            FaissApi.faiss_IDSelector_free(Ptr);
        }
    }
}
