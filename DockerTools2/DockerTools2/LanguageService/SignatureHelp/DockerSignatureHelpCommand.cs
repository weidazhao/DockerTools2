using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DockerTools2.LanguageService
{
    internal sealed class DockerSignatureHelpCommand : IOleCommandTarget
    {
        IOleCommandTarget _nextCommandHandler;
        ITextView _textView;
        ISignatureHelpBroker _broker;
        ISignatureHelpSession _session;

        public DockerSignatureHelpCommand(IVsTextView textViewAdapter, ITextView textView, ISignatureHelpBroker broker)
        {
            _textView = textView;
            _broker = broker;

            //add this to the filter chain
            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                var typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);

                if (typedChar == 27 && _session != null)
                {
                    DismissSession();
                }
                else if ((_session == null || _session.IsDismissed) && _textView.Caret.Position.BufferPosition > 0)
                {
                    SnapshotPoint point = _textView.Caret.Position.BufferPosition - 1;
                    var line = point.GetContainingLine();
                    string lineText = line.Extent.GetText().Trim();

                    if (!string.IsNullOrWhiteSpace(lineText) && !lineText.StartsWith("#"))
                    {
                        string[] words = lineText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (words.Length > 0 && LanguageTokens.Keywords.ContainsKey(words[0].ToUpperInvariant()))
                        {
                            _session = _broker.TriggerSignatureHelp(_textView);
                        }
                    }
                }
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
                    DismissSession();
            }

            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void DismissSession()
        {
            if (_session != null)
            {
                _session.Dismiss();
                _session = null;
            }
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

        }
    }
}
