using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Operations;

namespace DockerTools2.LanguageService
{
    [Export(typeof(IBraceCompletionContext))]
    internal class BraceCompletionContext : IBraceCompletionContext
    {
        private readonly IEditorOperations _editorOperations;
        private readonly ITextUndoHistory _undoHistory;

        public BraceCompletionContext(IEditorOperations editorOperations, ITextUndoHistory undoHistory)
        {
            _editorOperations = editorOperations;
            _undoHistory = undoHistory;
        }

        public bool AllowOverType(IBraceCompletionSession session)
        {
            return true;
        }

        public void Finish(IBraceCompletionSession session) { }

        public void Start(IBraceCompletionSession session) { }

        public void OnReturn(IBraceCompletionSession session)
        {
            var closingPointPosition = session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot);

            using (var undo = _undoHistory.CreateTransaction("Insert new line."))
            {
                _editorOperations.AddBeforeTextBufferChangePrimitive();

                _editorOperations.MoveLineUp(false);
                _editorOperations.MoveToEndOfLine(false);
                _editorOperations.InsertNewLine();

                _editorOperations.AddAfterTextBufferChangePrimitive();
                undo.Complete();
            }
        }
    }
}
