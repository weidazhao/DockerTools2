using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace DockerTools2.LanguageService
{
    internal static class DockerFactory
    {
        public delegate void AddSignatures(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span);

        public static AddSignatures GetMethod(string keyword)
        {
            switch (keyword.ToUpperInvariant())
            {
                case "ADD":
                    return Add;
                case "ARG":
                    return Arg;
                case "COPY":
                    return Copy;
                case "ENTRYPOINT":
                    return Entrypoint;
                case "ENV":
                    return Env;
                case "FROM":
                    return From;
                case "HEALTHCHECK":
                    return Healthcheck;
                case "LABEL":
                    return Label;
                case "MAINTAINER":
                    return Maintainer;
            }

            return null;
        }

        private static void Add(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<src> <dest>", "Example: hom?.txt /mydir/", span, session);

            signatures.Add(signature1);
        }

        private static void Arg(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<name>", "An argument without a default value", span, session);
            var signature2 = new DockerSignature(keyword, "<name>=<value>", "An argument with a default value", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
        }

        private static void Copy(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<src> <dest>", "Example: hom?.txt /mydir/", span, session);

            signatures.Add(signature1);
        }

        private static void Entrypoint(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "[\"executable\", \"param1\", \"param2\"]", "Example: [\"top\", \"-b\"]", span, session);
            var signature2 = new DockerSignature(keyword, "command param1 param2", "Example: exec top -b", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
        }

        private static void Env(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<key>=<value>", "Example: myName=\"John Doe\" myDog=Rex", span, session);
            var signature2 = new DockerSignature(keyword, "<key> <value>", "Example: myName John Doe", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
        }

        private static void Expose(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<port>", "A port number to expose", span, session);
            var signature2 = new DockerSignature(keyword, "<port> <port>...", "Multiple port numbers to expose", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
        }

        private static void From(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<image>", "Name of the image", span, session);
            var signature2 = new DockerSignature(keyword, "<image>:<tag>", "Image name and tag", span, session);
            var signature3 = new DockerSignature(keyword, "<image>@<digest>", "Image name and digest", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
            signatures.Add(signature3);
        }

        private static void Healthcheck(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "[options] CMD <command>", "Example: --interval=5m --timeout=3s CMD curl -f http://localhost/ || exit 1", span, session);
            var signature2 = new DockerSignature(keyword, "NONE", "disable any healthcheck inherited from the base image", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
        }

        private static void Label(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "key/value pair", "Example: version=\"1.0\"", span, session);
            var signature2 = new DockerSignature(keyword, "key/value pairs", "Example: multi.label1=\"value1\" multi.label2=\"value2\" other=\"value3\"", span, session);

            signatures.Add(signature1);
            signatures.Add(signature2);
        }

        private static void Maintainer(ISignatureHelpSession session, IList<ISignature> signatures, string keyword, ITrackingSpan span)
        {
            var signature1 = new DockerSignature(keyword, "<name>", "Example: Victor Vieux <victor@docker.com>", span, session);

            signatures.Add(signature1);
        }
    }
}
