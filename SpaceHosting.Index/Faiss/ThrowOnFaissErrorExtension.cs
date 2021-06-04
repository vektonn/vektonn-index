using System.Runtime.InteropServices;

namespace SpaceHosting.Index.Faiss
{
    internal static class ThrowOnFaissErrorExtension
    {
        public static void ThrowOnFaissError(this int errorCode)
        {
            if (errorCode != 0)
            {
                var messagePtr = FaissApi.faiss_get_last_error();
                var errorMessage = Marshal.PtrToStringAnsi(messagePtr);

                throw new FaissException(errorCode, errorMessage);
            }
        }
    }
}
