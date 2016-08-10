using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    public static class DockerfileClassificationTypes
    {
        public const string Keyword = "Dockerfile Token";

        [Export, Name(Keyword)]
        public static ClassificationTypeDefinition DockerfileClassificationBold { get; set; }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DockerfileClassificationTypes.Keyword)]
    [Name(DockerfileClassificationTypes.Keyword)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    internal sealed class DockerfileBoldFormatDefinition : ClassificationFormatDefinition
    {
        public DockerfileBoldFormatDefinition()
        {
            IsBold = true;
            DisplayName = DockerfileClassificationTypes.Keyword;
        }
    }
}
