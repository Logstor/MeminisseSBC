namespace Meminisse
{
    public interface IState
    {
        /// <summary>
        /// The amount of milliseconds between data requests.
        /// </summary>
        /// <value>long</value>
        long logDelay
        {
            get;
        }

        void OnEnterState(IStateController control);
        void OnExitState(IStateController control);
        void HandleUpdate(IStateController control, long totalMilliseconds, EntityWrap entity);
    }
}