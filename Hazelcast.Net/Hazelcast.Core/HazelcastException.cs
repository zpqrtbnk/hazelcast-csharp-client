using System;

namespace Hazelcast.Core
{
    [Serializable]
    public class HazelcastException : SystemException
    {
        public HazelcastException()
        {
        }

        public HazelcastException(string message) : base(message)
        {
        }

        public HazelcastException(string message, Exception cause) : base(message, cause)
        {
        }

        public HazelcastException(Exception cause) : base(cause.Message)
        {
        }
    }

    [Serializable]
    public class QueryException : HazelcastException 
    {

        public QueryException() {
        }

        public QueryException(String message) :base(message){}

        public QueryException(string message, Exception cause): base(message, cause){}

        public QueryException(Exception cause) : base(cause.Message) { }
    }
}