using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace DockerTools2.LanguageService
{
    internal class DockerSignatureHelpSource : ISignatureHelpSource
    {
        private ITextBuffer _buffer;

        public DockerSignatureHelpSource(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            SnapshotPoint? point = session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (!point.HasValue)
                return;

            var line = point.Value.GetContainingLine();
            string lineText = line.Extent.GetText();
            string[] words = lineText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0 || !LanguageTokens.Keywords.ContainsKey(words[0].ToUpperInvariant()))
                return;

            string keyword = words[0].ToUpperInvariant();
            DockerFactory.AddSignatures addSignatures = DockerFactory.GetMethod(keyword);

            if (addSignatures != null)
            {
                var applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(line.Start, line.Length, SpanTrackingMode.EdgeNegative);

                //signatures.Clear();
                addSignatures(session, signatures, keyword, applicableToSpan);

                //if (session == null || session.Properties == null)
                //    return;

                session.Properties.AddProperty("keyword", keyword);
                //session.Match();
            }
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
           // int number = 0;

            if (session.Properties.ContainsProperty("keyword"))
            {
                //string keyword = session.Properties["keyword"] as string;
                //string methodName = DockerFactory.GetMethod(keyword).Method.Name;
                ////if (keyword.Values.Count > 0 && (methodName == "Margins" || methodName == "Corners"))
                ////{
                ////    number = 4 - keyword.Values.Count;
                ////}
                return session.Signatures[0];
            }

            return null;
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}
