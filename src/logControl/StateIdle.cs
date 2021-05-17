using DuetAPI.ObjectModel;

namespace Meminisse
{
    public class StateIdle : IState
    {
        long IState.logDelay { get { return 60000L / Config.instance.IdleCheckPrMin; } }

        private Logger logger = Logger.instance;

        void IState.OnEnterState(IStateController control) 
        {
            Logger.instance.D("Entering Idle State");
        }

        void IState.OnExitState(IStateController control)
        {
            Logger.instance.D("Exiting Idle State");
        }

        void IState.HandleUpdate(IStateController control, long totalMilliseconds, EntityWrap entity)
        {
            switch(entity.machineStatus)
            {
                case MachineStatus.Processing:
                    control.ChangeState(new StateProcessing(control, control.GetCurrentFilename()));
                    break;
                case MachineStatus.Idle:
                case MachineStatus.Off:
                case MachineStatus.Halted:
                case MachineStatus.ChangingTool:
                default:
                    // Update configuration if necessary
                    Config.instance.Refresh();
                    break;
            }
        }

        void IState.OnCancel()
        {
            Logger.instance.T("Idle state cancelling");
        }
    }
}