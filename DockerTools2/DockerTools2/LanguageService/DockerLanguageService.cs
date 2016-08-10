using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DockerTools2.LanguageService
{
    [Guid("b09ec8c5-83b8-44b5-bbbf-e7c4bd17d3ac")]
    public class DockerLanguageService : Microsoft.VisualStudio.Package.LanguageService
    {
        public const string LanguageName = "Docker File";
        private LanguagePreferences preferences = null;

        public DockerLanguageService(object site)
        {
            SetSite(site);
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            return new DockerSource(this, buffer, new DockerColorizer(this, buffer, null));
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (preferences == null)
            {
                preferences = new LanguagePreferences(Site, typeof(DockerLanguageService).GUID, Name);

                if (preferences != null)
                {
                    preferences.Init();

                    preferences.EnableCodeSense = true;
                    preferences.EnableMatchBraces = true;
                    preferences.EnableMatchBracesAtCaret = true;
                    preferences.EnableShowMatchingBrace = true;
                    preferences.EnableCommenting = true; ;
                    preferences.HighlightMatchingBraceFlags = _HighlightMatchingBraceFlags.HMB_USERECTANGLEBRACES;
                    preferences.LineNumbers = true;
                    preferences.MaxErrorMessages = 100;
                    preferences.AutoOutlining = true;
                    preferences.MaxRegionTime = 2000;
                    preferences.ShowNavigationBar = false;
                    preferences.InsertTabs = false;
                    preferences.IndentSize = 2;

                    preferences.AutoListMembers = true;
                    preferences.EnableQuickInfo = true;
                    preferences.ParameterInformation = true;
                }
            }

            return preferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return null;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return null;
        }

        public override string GetFormatFilterList()
        {
            return null;
        }

        public override string Name => LanguageName;

        public override void Dispose()
        {
            try
            {
                if (preferences != null)
                {
                    preferences.Dispose();
                    preferences = null;
                }
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
