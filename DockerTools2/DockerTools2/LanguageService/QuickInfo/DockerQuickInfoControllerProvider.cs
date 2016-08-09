using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("Docker QuickInfo Controller")]
    [ContentType(DockerContentTypeDefinition.DockerContentType)]
    public class DockerQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new DockerQuickInfoController(textView, subjectBuffers, this);
        }
    }
}
