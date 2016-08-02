using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public interface IDockerClient
    {
        Task<DockerClientResult> ExecuteAsync(string commandWithArguments, CancellationToken cancellationToken);
    }
}
