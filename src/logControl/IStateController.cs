namespace Meminisse
{
    public interface IStateController
    {
        void ChangeState(IState newState);

        string GetCurrentFilename();

        string GetCurrentFilePath();
    }
}