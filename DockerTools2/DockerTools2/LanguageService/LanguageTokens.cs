using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerTools2.LanguageService
{
    public class LanguageTokens
    {
        public static Dictionary<string, string> Keywords = new Dictionary<string, string>
        {
             { "ADD", "The ADD instruction copies new files, directories or remote file URLs from <src> and adds them to the file system of the container at the path <dest>." },
             { "ARG", "The ARG instruction defines a variable that users can pass at build-time to the builder with the docker build command using the --build-arg <varname>=<value> flag." },
             { "CMD", "There can only be one CMD instruction in a Dockerfile. If you list more than one CMD then only the last CMD will take effect." },
             { "COPY", "The COPY instruction copies new files or directories from <src> and adds them to the file system of the container at the path <dest>." },
             { "ENTRYPOINT", "An ENTRYPOINT allows you to configure a container that will run as an executable." },
             { "ENV", "The ENV instruction sets the environment variable <key> to the value <value>. This value will be in the environment of all “descendant” Dockerfile commands and can be replaced inline in many as well." },
             { "EXPOSE", "The EXPOSE instruction informs Docker that the container listens on the specified network ports at runtime. " },
             { "FROM", "The FROM instruction sets the Base Image for subsequent instructions. As such, a valid Dockerfile must have FROM as its first instruction." },
             { "HEALTHCHECK", "The HEALTHCHECK instruction tells Docker how to test a container to check that it is still working. This can detect cases such as a web server that is stuck in an infinite loop and unable to handle new connections, even though the server process is still running." },
             { "LABEL", "The LABEL instruction adds metadata to an image. A LABEL is a key-value pair. To include spaces within a LABEL value, use quotes and backslashes as you would in command-line parsing." },
             { "MAINTAINER", "The MAINTAINER instruction allows you to set the Author field of the generated images." },
             { "ONBUILD", "The ONBUILD instruction adds to the image a trigger instruction to be executed at a later time, when the image is used as the base for another build." },
             { "RUN", "The RUN instruction will execute any commands in a new layer on top of the current image and commit the results. The resulting committed image will be used for the next step in the Dockerfile." },
             { "SHELL", "The SHELL instruction allows the default shell used for the shell form of commands to be overridden. The default shell on Linux is [\"/bin/sh\", \"-c\"], and on Windows is [\"cmd\", \"/S\", \"/C\"]. The SHELL instruction must be written in JSON form in a Dockerfile." },
             { "STOPSIGNAL", "The STOPSIGNAL instruction sets the system call signal that will be sent to the container to exit." },
             { "USER", "The USER instruction sets the user name or UID to use when running the image and for any RUN, CMD and ENTRYPOINT instructions that follow it in the Dockerfile." },
             { "VOLUME", "The VOLUME instruction creates a mount point with the specified name and marks it as holding externally mounted volumes from native host or other containers." },
             { "WORKDIR", "WORKDIR instruction sets the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD instructions that follow it in the Dockerfile. If the WORKDIR doesn’t exist, it will be created even if it’s not used in any subsequent Dockerfile instruction." },
        };
    }
}
