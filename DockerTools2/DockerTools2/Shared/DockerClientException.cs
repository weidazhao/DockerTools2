//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace DockerTools2.Shared
{
    [Serializable]
    internal class DockerClientException : Exception
    {
        public DockerClientException()
        {
        }

        public DockerClientException(string message)
            : base(message)
        {
        }

        public DockerClientException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DockerClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
