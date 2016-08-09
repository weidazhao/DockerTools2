using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("Docker QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType(DockerContentTypeDefinition.DockerContentType)]
    internal class DockerQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        IClassifierAggregatorService ClassifierAggregatorService { get; set; }

        [Import]
        IGlyphService GlyphService { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new DockerQuickInfo(textBuffer, ClassifierAggregatorService, GlyphService));
        }
    }
}
