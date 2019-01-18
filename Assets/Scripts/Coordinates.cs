using UnityEngine;

public class Coordinates : MonoBehaviour
{
    public static Vector3 GetCoord(string cell, bool isPawn)
    {
        float x = -0.21F, y, z = -0.21F, d = 0.06F;
        if (isPawn)
            y = 0.79F;
        else
            y = 0.78F;

        float hor = float.Parse(cell[0].ToString());
        float vert = float.Parse(cell[2].ToString());

        for (; hor > 1; x += d, hor--) ;
        for (; vert > 1; z += d, vert--) ;

        return new Vector3(x, y, z);
    }

    public static GameObject FindCellByCoord(int x, int y)
    {
        var name = x.ToString() + ":" + y.ToString();
        return GameObject.Find(name);
    }

    public static void MovePiece(string initialCell, string finalCell)
    {
        var cell1 = GameObject.Find(initialCell);
        var cell2 = GameObject.Find(finalCell);
        var piece = cell1.transform.GetChild(0).gameObject;

        if (cell2.transform.childCount > 0)
            Destroy(cell2.transform.GetChild(0).gameObject);

        piece.transform.parent = cell2.transform;

        bool isPawn = piece.name.StartsWith("White Pawn") || piece.name.StartsWith("Black Pawn");
        var target = GetCoord(cell2.name, isPawn);
        piece.GetComponent<PieceMover>().SmoothMove(target);
    }
    public static void MovePiece(int x0, int y0, int x1, int y1)
    {
        var cell1 = FindCellByCoord(x0, y0);
        var cell2 = FindCellByCoord(x1, y1);
        var piece = cell1.transform.GetChild(0).gameObject;

        if (cell2.transform.childCount > 1)
            Debug.Log(cell2.transform.GetChild(0).name + " " + cell2.transform.GetChild(1).name);

        if (cell2.transform.childCount > 0)
            Destroy(cell2.transform.GetChild(0).gameObject);

        piece.transform.parent = cell2.transform;

        bool isPawn = piece.name.StartsWith("White Pawn") || piece.name.StartsWith("Black Pawn");
        var target = GetCoord(cell2.name, isPawn);
        piece.GetComponent<PieceMover>().SmoothMove(target);
    }

    public static void DeletePiece(int x, int y)
    {
        var cell = FindCellByCoord(x, y);
        if (cell.transform.childCount > 0)
            Destroy(cell.transform.GetChild(0).gameObject);
    }
}