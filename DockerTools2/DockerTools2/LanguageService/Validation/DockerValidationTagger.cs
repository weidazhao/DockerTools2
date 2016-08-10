using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace DockerTools2.LanguageService
{
    public class DockerValidationTagger : ITagger<IErrorTag>
    {
        private ITextBuffer _buffer;

        public DockerValidationTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || spans[0].IsEmpty)
                yield break;

            var span = spans[0];
            var line = span.Snapshot.GetLineFromPosition(span.Start);
            var text = line.GetText();

            if (string.IsNullOrWhiteSpace(text))
                yield break;

            string[] tokens = text.Split(' ');

            if (!tokens.Any() || tokens[0].Trim().StartsWith("#"))
                yield break;

            string keyword = tokens[0].ToUpperInvariant();

            if (!LanguageTokens.Keywords.ContainsKey(keyword))
            {
                if (line.LineNumber > 1)
                {
                    var prevLine = span.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                    var prevText = prevLine.GetText().TrimEnd();

                    if (prevText.EndsWith("\\"))
                        yield break;
                }

                var sSpan = new SnapshotSpan(span.Start, keyword.Length);
                var tag = new ErrorTag("Intellisense", $"\"{tokens[0].Trim()}\" is not a known Dockerfile keyword.");
                yield return new TagSpan<IErrorTag>(sSpan, tag);
            }
        }


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
