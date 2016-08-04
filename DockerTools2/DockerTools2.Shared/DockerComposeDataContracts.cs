//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DockerTools2.Shared
{
    public class DockerComposeDevelopmentDocument
    {
        [YamlMember(Alias = "version")]
        public string Version { get; set; }

        [YamlMember(Alias = "services")]
        public Dictionary<string, DockerComposeService> Services { get; set; }
    }

    public class DockerComposeService
    {
        [YamlMember(Alias = "build")]
        public DockerComposeBuild Build { get; set; }

        [YamlMember(Alias = "labels")]
        public List<string> Labels { get; set; }
    }

    public class DockerComposeBuild
    {
        [YamlMember(Alias = "args")]
        public Dictionary<string, string> Args { get; set; }
    }
}
