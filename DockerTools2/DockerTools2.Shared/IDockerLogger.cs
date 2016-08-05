namespace DockerTools2.Shared
{
    public interface IDockerLogger
    {
        void LogError(string error);

        void LogMessage(string message);

        void LogWarning(string warning);
    }
}
