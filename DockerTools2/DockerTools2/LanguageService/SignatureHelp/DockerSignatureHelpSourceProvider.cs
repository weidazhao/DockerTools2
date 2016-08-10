using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [Name("Docker Signature Help Source")]
    [Order(Before = "default")]
    [ContentType(ContentTypes.DockerContentType)]
    internal class DockerSignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new DockerSignatureHelpSource(textBuffer));
        }
    }
}
