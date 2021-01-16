using System.Threading.Tasks;

namespace AutomationFramework
{
    public interface IKernel
    {
        string Version { get; }
        string Name { get; }
        Task Run(IRunInfo runInfo, IMetaData metaData);
    }
}
