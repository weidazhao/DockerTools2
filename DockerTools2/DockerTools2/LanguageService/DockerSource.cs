using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DockerTools2.LanguageService
{
    internal class DockerSource : Source
    {
        public DockerSource(Microsoft.VisualStudio.Package.LanguageService service, IVsTextLines textLines, Colorizer colorizer)
            : base(service, textLines, colorizer)
        { }

        public override CommentInfo GetCommentFormat()
        {
            return new CommentInfo
            {
                UseLineComments = true,
                LineStart = "#",
            };
        }
    }
}

