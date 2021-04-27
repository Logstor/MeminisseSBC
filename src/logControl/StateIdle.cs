using DuetAPI.ObjectModel;

namespace Meminisse
{
    public class StateIdle : IState
    {
        long IState.logDelay { get; } = 60000L / Config.instance.LogsPrMin;

        private Logger logger = Logger.instance;

        void IState.OnEnterState(IStateController control) 
        {
            this.logger.D("Entering Idle State");
        }

        void IState.OnExitState(IStateController control)
        {
            this.logger.D("Exiting Idle State");
        }

        void IState.HandleUpdate(IStateController control, long totalMilliseconds, EntityWrap entity)
        {
            switch(entity.machineStatus)
            {
                case MachineStatus.Processing:
                    control.ChangeState(new StateProcessing(control.GetCurrentFilename()));
                    break;
                case MachineStatus.Idle:
                case MachineStatus.Off:
                case MachineStatus.Halted:
                case MachineStatus.ChangingTool:
                default:
                    break;
            }
        }
    }
}