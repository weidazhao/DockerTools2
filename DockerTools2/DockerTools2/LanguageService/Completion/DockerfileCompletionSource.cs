using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Intel = Microsoft.VisualStudio.Language.Intellisense;

namespace DockerTools2.LanguageService
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(DockerContentTypeDefinition.DockerContentType)]
    [Name("DockerfileCompletion")]
    class DockerfileCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        IGlyphService GlyphService = null;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new DockerfileCompletionSource(textBuffer, GlyphService);
        }
    }

    class DockerfileCompletionSource : ICompletionSource
    {
        private ITextBuffer _buffer;
        private bool _disposed = false;
        private static ImageSource _glyph;

        public DockerfileCompletionSource(ITextBuffer buffer, IGlyphService glyphService)
        {
            _buffer = buffer;
            _glyph = glyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_disposed)
                return;

            List<Intel.Completion> completions = new List<Intel.Completion>();
            foreach (string item in LanguageTokens.Keywords.Keys)
            {
                completions.Add(new Intel.Completion(item, item, LanguageTokens.Keywords[item], _glyph, item));
            }

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(snapshot);

            if (triggerPoint == null)
                return;

            var line = triggerPoint.Value.GetContainingLine();
            string text = line.GetText();
            int index = text.IndexOf(' ');
            int hash = text.IndexOf('#');
            SnapshotPoint start = triggerPoint.Value;

            if (hash > -1 && hash < triggerPoint.Value.Position || (index > -1 && (start - line.Start.Position) > index))
                return;

            while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
            {
                start -= 1;
            }

            var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint.Value), SpanTrackingMode.EdgeInclusive);

            completionSets.Add(new CompletionSet("Dockerfile", "Dockerfile", applicableTo, completions, Enumerable.Empty<Intel.Completion>()));
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}