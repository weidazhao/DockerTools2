using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace DockerTools2.LanguageService
{
    public class DockerfileClassifier : IClassifier
    {
        private IClassificationType _keyword, _comment, _string, _symbol;
        public static Regex String = new Regex(@"""(?<content>[^""]+)?""?", RegexOptions.Compiled);
        public static Regex Tokens = new Regex(@"{{(?<content>[^}]+)}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public DockerfileClassifier(IClassificationTypeRegistryService registry)
        {
            _keyword = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            _comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _string = registry.GetClassificationType(PredefinedClassificationTypeNames.String);
            _symbol = registry.GetClassificationType(DockerfileClassificationTypes.Keyword);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> list = new List<ClassificationSpan>();

            string text = span.GetText();
            int index = text.IndexOf("#", StringComparison.Ordinal);

            if (index > -1)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + index, text.Length - index);
                list.Add(new ClassificationSpan(result, _comment));
            }

            if (index == -1 || index > 0)
            {
                string[] args = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (args.Any() && LanguageTokens.Keywords.ContainsKey(args[0].Trim().ToUpperInvariant()))
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start, args[0].Length);
                    list.Add(new ClassificationSpan(result, _keyword));
                }
            }

            // Strings
            var matches = String.Matches(text);
            foreach (Match match in matches)
            {
                if (index == -1 || match.Index < index)
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);
                    list.Add(new ClassificationSpan(result, _string));
                }
            }

            // tokens
            var tokenMatches = Tokens.Matches(text);
            foreach (Match match in tokenMatches)
            {
                if (index == -1 || match.Index < index)
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start + match.Index, match.Length);
                    list.Add(new ClassificationSpan(result, _symbol));
                }
            }

            return list;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }
    }
}