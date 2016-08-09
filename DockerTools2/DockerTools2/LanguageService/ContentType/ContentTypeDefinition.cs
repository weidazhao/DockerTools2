using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    public class DockerContentTypeDefinition
    {
        public const string DockerContentType = "DockerFile";

        [Export(typeof(ContentTypeDefinition))]
        [Name(DockerContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition IDockerFileContentType { get; set; }
    }
}
