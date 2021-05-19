namespace Meminisse
{
    public interface IStateController
    {
        bool ConfigFileListening { get; set; }

        void ChangeState(IState newState);

        string GetCurrentFilename();

        string GetCurrentFilePath();
    }
}