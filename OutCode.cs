using UnityEngine;

public class OutCode 
{
    private bool up;
    private bool down;
    private bool left;
    private bool right;

    public bool Up { get => up; set => up = value; }
    public bool Down { get => down; set => down = value; }
    public bool Left { get => left; set => left = value; }
    public bool Right { get => right; set => right = value; }

    public OutCode(Vector2 point)
    {
        up = point.y > 1;
        down = point.y < -1;
        left = point.x < -1;
        right = point.x > 1;
    }

    public OutCode()
    {

    }

    public OutCode(bool up, bool down, bool left, bool right)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }

    public static bool operator == (OutCode a, OutCode b)
    {
        return (a.up == b.up) && (a.down == b.down) && (a.left == b.left) && (a.right == b.right);
    }

    public static bool operator != (OutCode a, OutCode b)
    {
        return !(a == b);
    }

    public static OutCode operator * (OutCode a, OutCode b)
    {
        return new OutCode(a.up && b.up, a.down && b.down, a.left && b.left, a.right && b.right);
    }

    public static OutCode operator +(OutCode a, OutCode b)
    {
        return new OutCode(a.up || b.up, a.down || b.down, a.left || b.left, a.right || b.right);
    }
}
