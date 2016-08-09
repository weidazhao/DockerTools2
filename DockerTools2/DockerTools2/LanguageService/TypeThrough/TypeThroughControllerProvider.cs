using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DockerTools2.LanguageService
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [ContentType(DockerContentTypeDefinition.DockerContentType)]
    [Name("Docker TypeThrough Completion Controller")]
    [Order(Before = "Default Completion Controller")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class DockerTypeThroughControllerProvider : IIntellisenseControllerProvider
    {
        public IIntellisenseController TryCreateIntellisenseController(ITextView view, IList<ITextBuffer> subjectBuffers)
        {
            if (subjectBuffers.Count > 0 && DockerTools2Package.LanguageService.Preferences.EnableMatchBraces)
            {
                return view.Properties.GetOrCreateSingletonProperty(() => new DockerypeThroughController(view, subjectBuffers));
            }

            return null;
        }
    }

    internal class DockerypeThroughController : TypeThroughController
    {
        public DockerypeThroughController(ITextView textView, IList<ITextBuffer> subjectBuffers)
            : base(textView, subjectBuffers)
        { }

        protected override bool CanComplete(ITextBuffer textBuffer, int position)
        {
            var line = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
            return line.Start.Position + line.GetText().TrimEnd('\r', '\n', ' ', ';', ',').Length == position + 1;
        }

        protected override char GetCompletionCharacter(char typedCharacter)
        {
            switch (typedCharacter)
            {
                case '[':
                    return ']';
                case '(':
                    return ')';
                case '{':
                    return '}';
                case '"':
                    return '"';
                case '\'':
                    return '\'';
            }

            return '\0';
        }
    }
}

