using DuetAPI.ObjectModel;

namespace Meminisse
{
    /// <summary>
    /// 
    /// </summary>
    public class PositionEntity
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        public MachineStatus machineStatus { get; private set; }

        public PositionEntity(int x, int y, int z, MachineStatus machineStatus)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.machineStatus = machineStatus;
        }

        public PositionEntity(int[] pos, MachineStatus machineStatus)
        {
            if (pos.Length != 3)
                throw new System.Exception("Array needs to be of size 3, when creating a PositionEntity!");

            this.x = pos[0];
            this.y = pos[1];
            this.z = pos[2];
            this.machineStatus = machineStatus;
        }
    }
}