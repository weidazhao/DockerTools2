using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    [Export(typeof(IBraceCompletionContextProvider))]
    [ContentType(DockerContentTypeDefinition.DockerContentType)]
    [BracePair('{', '}')]
    [BracePair('(', ')')]
    [BracePair('[', ']')]
    [BracePair('"', '"')]
    [BracePair('\'', '\'')]
    internal sealed class BraceCompletionContextProvider : IBraceCompletionContextProvider
    {
        [Import]
        private IEditorOperationsFactoryService EditOperationsFactory = null;

        [Import]
        private ITextUndoHistoryRegistry UndoHistoryRegistry = null;

        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            var editorOperations = EditOperationsFactory.GetEditorOperations(textView);
            var undoHistory = UndoHistoryRegistry.GetHistory(textView.TextBuffer);

            if (IsValidBraceCompletionContext(textView, openingPoint, openingBrace))
            {
                context = new BraceCompletionContext(editorOperations, undoHistory);
                return true;
            }
            else
            {
                context = null;
                return false;
            }
        }

        private bool IsValidBraceCompletionContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace)
        {
            if (openingPoint.Position  == 0)
                return true;

            if (openingBrace == '"' || openingBrace == '\'')
            {
                var prevChar = openingPoint.Snapshot.GetText(openingPoint.Position - 1, 1)[0];
                return char.IsWhiteSpace(prevChar);
            }

            return true;
        }
    }
}
