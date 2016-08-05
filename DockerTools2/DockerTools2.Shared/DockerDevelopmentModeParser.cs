using System;

namespace DockerTools2.Shared
{
    public static class DockerDevelopmentModeParser
    {
        public static bool TryParse(string value, out DockerDevelopmentMode mode)
        {
            mode = DockerDevelopmentMode.Regular;

            if (StringComparer.Ordinal.Equals(value, "Docker Fast") || StringComparer.Ordinal.Equals(value, "Fast"))
            {
                mode = DockerDevelopmentMode.Fast;
                return true;
            }
            else if (StringComparer.Ordinal.Equals(value, "Docker Regular") || StringComparer.Ordinal.Equals(value, "Regular"))
            {
                mode = DockerDevelopmentMode.Regular;
                return true;
            }

            return false;
        }
    }
}
