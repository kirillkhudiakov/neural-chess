using UnityEngine;

public class ChessColors : MonoBehaviour
{
    public static Color White
    {
        get
        {
            return new Color(219F / 255, 219F / 255, 219F / 255);
        }
    }

    public static Color Black
    {
        get
        {
            return new Color(0F, 0F, 0F);
        }
    }
}