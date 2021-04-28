using System;
using System.Collections.Generic;

using DuetAPI.ObjectModel;

namespace Meminisse
{
    public class StateProcessing : IState
    {
        long IState.logDelay { get; } = 60000L / Config.instance.LogsPrMin;

        private ILogController logController;

        private Logger logger = Logger.instance;

        /// <summary>
        /// Create a new StateProcessing instance, which creates and initializes a new log file.
        /// </summary>
        public StateProcessing(string filename)
        {
            // Initialize persistent logging
            this.logController = new CSVLogController();
            this.logController.Init(filename, this.CreateInitLogList());
            this.logController.FlushToFile();
        }

        /// <summary>
        /// When we should continue logging in an already active log file.
        /// </summary>
        /// <param name="logController"></param>
        public StateProcessing(ILogController logController)
        {
            this.logController = logController;
        }

        void IState.OnEnterState(IStateController control)
        {
            Logger.instance.D("Entering Processing State");
        }

        void IState.OnExitState(IStateController control)
        {
            Logger.instance.D("Exiting Processing State");
        }

        void IState.HandleUpdate(IStateController control, long totalMilliseconds, EntityWrap entity)
        {
            // Check status
            switch (entity.machineStatus)
            {
                case MachineStatus.Off:
                case MachineStatus.Halted:
                case MachineStatus.Idle:
                    this.ChangeStateAndFlush(control, new StateIdle());
                    break;

                case MachineStatus.Paused:
                case MachineStatus.ChangingTool:
                    this.ChangeStateAndFlush(control, new StatePaused(this.logController));
                    break;

                case MachineStatus.Processing:
                default:
                    try { this.WriteLog(totalMilliseconds, entity); }
                    catch (Exception e)
                    {
                        Logger.instance.D(e.ToString());
                        Logger.instance.E("Failed writing to log file, trying to recover with new log file");
                        control.ChangeState(new StateIdle());
                    }
                    break;
            }
        }

        private void WriteLog(long totalMilliseconds, EntityWrap entities)
        {
            this.logController.Add(totalMilliseconds, this.CreateLogList(entities));
            this.logController.FlushToFile();
        }

        private void ChangeStateAndFlush(IStateController control, IState newState)
        {
            this.logController.FlushToFile();
            control.ChangeState(newState);
        }

        private List<LogEntity> CreateInitLogList()
        {
            List<LogEntity> list = new List<LogEntity>(10);

            if (Config.instance.LogPosition)
                list.Add(LogEntity.Position);

            if (Config.instance.LogSpeed)
                list.Add(LogEntity.Speed);
            
            if (Config.instance.LogTime)
                list.Add(LogEntity.Time);
            
            if (Config.instance.LogExtrusion)
                list.Add(LogEntity.Extrusion);
            
            if (Config.instance.LogBaby)
                list.Add(LogEntity.Babystep);
            
            return list;
        }

        private List<ILogEntity> CreateLogList(EntityWrap entities)
        {
            List<ILogEntity> list = new List<ILogEntity>(10);

            if (Config.instance.LogPosition)
                list.Add(entities.position);

            if (Config.instance.LogSpeed)
                list.Add(entities.speed);

            if (Config.instance.LogTime)
                list.Add(entities.time);

            if (Config.instance.LogExtrusion)
                list.Add(entities.extrusion);

            if (Config.instance.LogBaby)
                list.Add(entities.babystep);

            return list;
        }
    }
}