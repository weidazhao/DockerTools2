using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class CommandRegistration : IVsTextViewCreationListener
    {
        [Import]
        IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        public IContentTypeRegistryService Registry { get; set; }

        [Import]
        ICompletionBroker CompletionBroker { get; set; }

        private static readonly Guid _contentTag = new Guid();

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            ITextDocument document = null;

            if (textView != null && TextDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out document))
            {
                if (!Path.GetFileName(document.FilePath).Equals("dockerfile", StringComparison.OrdinalIgnoreCase))
                    return;

                DockerTools2Package.ForecePackageLoad();

                IVsTextLines lines;
                ErrorHandler.ThrowOnFailure(textViewAdapter.GetBuffer(out lines));
                lines.SetLanguageServiceID(typeof(DockerLanguageService).GUID);

                IContentType contentType = Registry.GetContentType(ContentTypes.DockerContentType);
                textView.TextBuffer.ChangeContentType(contentType, _contentTag);

                CommandFilter filter = new CommandFilter(textView, CompletionBroker);
                IOleCommandTarget next;
                ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(filter, out next));
                filter.Next = next;
            }
        }
    }
}
