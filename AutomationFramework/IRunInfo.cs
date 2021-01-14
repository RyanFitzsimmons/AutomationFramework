namespace AutomationFramework
{
    public interface IRunInfo
    {
        RunType Type { get; }
        StagePath Path { get; }
        bool GetIsValid(out string exceptionMsg);
    }
}
