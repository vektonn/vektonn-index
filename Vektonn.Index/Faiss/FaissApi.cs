using System;
using System.Runtime.InteropServices;

namespace Vektonn.Index.Faiss
{
    internal class FaissApi
    {
        // const char *faiss_get_last_error()
        [DllImport(@"libfaiss_c")]
        internal static extern IntPtr faiss_get_last_error();

        /** Build and index with the sequence of processing steps described in
          *  the string.
          */
        // int faiss_index_factory(FaissIndex** p_index, int d, const char* description, FaissMetricType metric);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_index_factory(ref IntPtr p_index, int d, string description, FaissMetricType metric);

        /** same as IndexIDMap but also provides an efficient reconstruction
          *  implementation via a 2-way index.
          */
        // int faiss_IndexIDMap2_new(FaissIndexIDMap2** p_index, FaissIndex* index);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_IndexIDMap2_new(ref IntPtr p_index, IntPtr index);

        // int faiss_Index_is_trained(const FaissIndex *)
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_is_trained(IntPtr index);

        // int faiss_Index_add(FaissIndex* index, idx_t n, const float* x);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_add(IntPtr index, long n, float[] x);

        // idx_t faiss_Index_ntotal(const FaissIndex *)
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_ntotal(IntPtr index);

        // int faiss_Index_search(const FaissIndex* index, idx_t n, const float* x, idx_t k, float* distances, idx_t* labels)
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_search(IntPtr index, long n, float[] x, long k, float[] distances, long[] labels);

        // void faiss_Index_free(FaissIndex *obj)
        [DllImport(@"libfaiss_c")]
        internal static extern void faiss_Index_free(IntPtr obj);

        // int faiss_write_index_fname(const FaissIndex *idx, const char *fname)
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_write_index_fname(IntPtr idx, string fname);

        /** Reconstruct a stored vector (or an approximation if lossy coding)
          *
          * this function may not be defined for some indexes
          * @param index       opaque pointer to index object
          * @param key         id of the vector to reconstruct
          * @param recons      reconstucted vector (size d)
          */
        // int faiss_Index_reconstruct(const FaissIndex* index, idx_t key, float* recons);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_reconstruct(IntPtr index, long key, float[] recons);

        /** Same as add, but stores xids instead of sequential ids.
          *
          * The default implementation fails with an assertion, as it is
          * not supported by all indexes.
          *
          * @param index  opaque pointer to index object
          * @param x_ids   if non-null, ids to store for the vectors (size n)
          */
        // int faiss_Index_add_with_ids(FaissIndex* index, idx_t n, const float* x, const long long* xids);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_add_with_ids(IntPtr index, long n, float[] x, long[] x_ids);

        /** removes IDs from the index. Not supported by all indexes
          * @param index       opaque pointer to index object
          * @param n_removed     output for the number of IDs removed
          */
        // int faiss_Index_remove_ids(FaissIndex* index, const FaissIDSelector* sel, long long* n_removed);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_Index_remove_ids(IntPtr index, IntPtr id_selector, ref IntPtr n_removed);

        /** Remove ids from a set. Repetitions of ids in the indices set
          * passed to the constructor does not hurt performance. The hash
          * function used for the bloom filter and GCC's implementation of
          * unordered_set are just the least significant bits of the id. This
          * works fine for random ids or ids in sequences but will produce many
          * hash collisions if lsb's are always the same */
        // int faiss_IDSelectorBatch_new(FaissIDSelectorBatch** p_sel, long long n, const idx_t* indices);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_IDSelectorBatch_new(ref IntPtr p_sel, long n, long[] indices);

        [DllImport(@"libfaiss_c")]
        internal static extern void faiss_IDSelector_free(IntPtr obj);

        #region Parameter space tuning

        // int faiss_ParameterSpace_new(FaissParameterSpace** pSpace);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_ParameterSpace_new(ref IntPtr pSpace);

        // void faiss_ParameterSpace_free(FaissParameterSpace* pSpace);
        [DllImport(@"libfaiss_c")]
        internal static extern void faiss_ParameterSpace_free(IntPtr pSpace);

        // set one of the parameters
        // int faiss_ParameterSpace_set_index_parameter(const FaissParameterSpace* pSpace, FaissIndex* pIndex, const char* paramName, double paramValue);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_ParameterSpace_set_index_parameter(IntPtr pSpace, IntPtr pIndex, string paramName, double paramValue);

        // get one of the parameters
        // int faiss_ParameterSpace_get_index_parameter(const FaissParameterSpace* pSpace, FaissIndex* pIndex, const char* paramName, double* paramValue);
        [DllImport(@"libfaiss_c")]
        internal static extern int faiss_ParameterSpace_get_index_parameter(IntPtr pSpace, IntPtr pIndex, string paramName, ref double paramValue);

        #endregion
    }
}
