using DuetAPI.ObjectModel;

namespace Meminisse
{
    public class StatePaused : IState
    {
        long IState.logDelay { get; } = 60000L / Config.instance.LogsPrMin;

        private ILogController logController;

        private Logger logger = Logger.instance;

        public StatePaused(ILogController logController)
        {
            this.logController = logController;
        }

        void IState.OnEnterState(IStateController control)
        {
            this.logger.D("Entering Pause State");
        }

        void IState.OnExitState(IStateController control)
        {
            this.logger.D("Exiting Pause State");
        }

        void IState.HandleUpdate(IStateController control, long totalMilliseconds, EntityWrap entity)
        {
            switch (entity.machineStatus)
            {
                case MachineStatus.Resuming:
                case MachineStatus.Processing:
                    control.ChangeState(new StateProcessing(this.logController));
                    break;

                case MachineStatus.Idle:
                case MachineStatus.Halted:
                case MachineStatus.Off:
                    control.ChangeState(new StateIdle());
                    break;

                case MachineStatus.ChangingTool:
                default:
                    break;
            }
        }
    }
}