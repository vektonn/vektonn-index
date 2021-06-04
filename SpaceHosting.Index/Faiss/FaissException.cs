using System;

namespace SpaceHosting.Index.Faiss
{
    public class FaissException : Exception
    {
        public FaissException(int errorCode, string errorMessage)
            : base($"{errorCode}: {errorMessage}")
        {
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; }
    }
}
