//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace DockerTools2.Shared
{
    [Serializable]
    public class CommandLineClientException : Exception
    {
        public CommandLineClientException()
        {
        }

        public CommandLineClientException(string message)
            : base(message)
        {
        }

        public CommandLineClientException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CommandLineClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
