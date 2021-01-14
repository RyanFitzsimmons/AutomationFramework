namespace AutomationFramework
{
    public interface IKernel
    {
        string Version { get; }
        string Name { get; }
        void Run(IRunInfo runInfo, IMetaData metaData);
    }
}
