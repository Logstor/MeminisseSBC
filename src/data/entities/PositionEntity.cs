
/// <summary>
/// 
/// </summary>
public class PositionEntity
{
    /// <summary>
    /// { x, y, z }
    /// </summary>
    public int[] pos { get; set; }

    public PositionEntity(int x, int y, int z)
    {
        this.pos[0] = x;
        this.pos[1] = y;
        this.pos[2] = z;
    }

    public PositionEntity(int[] pos)
    {
        if (pos.Length != 3)
            throw new System.Exception("Array needs to be of size 3, when creating a PositionEntity!");

        this.pos = pos;
    }

    public int getX() { return pos[0]; }
    public int getY() { return pos[1]; }
    public int getZ() { return pos[2]; }
}