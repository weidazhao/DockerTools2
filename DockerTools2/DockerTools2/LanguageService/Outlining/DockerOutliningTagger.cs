using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace DockerTools2.LanguageService
{
    public class DockerOutliningTagger : ITagger<IOutliningRegionTag>
    {
        private ITextBuffer _buffer;

        public DockerOutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || !DockerTools2Package.LanguageService.Preferences.AutoOutlining)
                yield break;

            var lines = _buffer.CurrentSnapshot.Lines;
            ITextSnapshotLine start = null, end = null;

            foreach (var line in lines)
            {
                string text = line.GetText();

                if (start == null && !string.IsNullOrWhiteSpace(text))
                    start = line;

                else if (start != null && !string.IsNullOrWhiteSpace(text))
                    end = line;

                if (line.Extent.IsEmpty || string.IsNullOrWhiteSpace(text) || line.LineNumber == lines.Last().LineNumber)
                {
                    if (start != null && end != null)
                    {
                        var span = new SnapshotSpan(start.Start, end.End);
                        yield return CreateTag(span, start.GetText(), span.GetText());
                    }

                    start = end = null;
                }
            }
        }

        private static TagSpan<IOutliningRegionTag> CreateTag(SnapshotSpan span, string text, string tooltip = null)
        {
            var tag = new OutliningRegionTag(false, false, text, tooltip);
            return new TagSpan<IOutliningRegionTag>(span, tag);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
