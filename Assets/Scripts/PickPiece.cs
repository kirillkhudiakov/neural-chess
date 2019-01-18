using UnityEngine;

public class PickPiece : MonoBehaviour
{
    GameManager GM;
    AI AIObj;

    // Use this for initialization
    void Start()
    {
        GM = FindObjectOfType<GameManager>();
        AIObj = FindObjectOfType<AI>();
    }

    private void OnMouseUp()
    {
        if (GM.UserMove && !GM.GameOver)
        {
            bool stepDone = false;

            if (GM.IsPiecePicked)
            {
                int[] begin = GetNumericCoord(GM.PiecePicked);
                int[] end = GetNumericCoord(gameObject);
                int index = GM.Board.GetIndex(begin[0], begin[1]);
                if (GM.Board.CanMove(index, end[0], end[1]))
                {
                    GM.Board.MakeStep(index, end[0], end[1], true);
                    Unpick();
                    GM.CheckResult();

                    if (!GM.GameOver)
                        AIObj.NextMove();

                    stepDone = true;
                }
            }

            if (!stepDone)
            {

                if (GM.Side == 1)
                {
                    switch (tag)
                    {
                        case "White Piece":
                            if (GM.IsPiecePicked)
                                Unpick();
                            GM.IsPiecePicked = true;
                            GM.PiecePicked = gameObject;
                            GetComponent<MeshRenderer>().material.color = Color.yellow;
                            break;

                        case "Cell":
                            if (transform.childCount == 1 && transform.GetChild(0).gameObject.tag == "White Piece")
                            {
                                if (GM.IsPiecePicked)
                                    Unpick();
                                GM.IsPiecePicked = true;
                                GM.PiecePicked = transform.GetChild(0).gameObject;
                                GM.PiecePicked.GetComponent<MeshRenderer>().material.color = Color.yellow;
                            }
                            break;
                    }
                }
                else if (GM.Side == -1)
                {
                    switch (tag)
                    {
                        case "Black Piece":
                            if (GM.IsPiecePicked)
                                Unpick();
                            GM.IsPiecePicked = true;
                            GM.PiecePicked = gameObject;
                            GetComponent<MeshRenderer>().material.color = Color.yellow;
                            break;

                        case "Cell":
                            if (transform.childCount == 1 && transform.GetChild(0).gameObject.tag == "Black Piece")
                            {
                                if (GM.IsPiecePicked)
                                    Unpick();
                                GM.IsPiecePicked = true;
                                GM.PiecePicked = transform.GetChild(0).gameObject;
                                GM.PiecePicked.GetComponent<MeshRenderer>().material.color = Color.yellow;
                            }
                            break;
                    }
                }
            }
        }
    }

    void Unpick()
    {
        GM.IsPiecePicked = false;
        if (GM.PiecePicked.tag == "White Piece")
        {
            GM.PiecePicked.GetComponent<MeshRenderer>().material.color = ChessColors.White;
        }
        else if (GM.PiecePicked.tag == "Black Piece")
        {
            GM.PiecePicked.GetComponent<MeshRenderer>().material.color = ChessColors.Black;
        }
    }

    int[] GetNumericCoord(GameObject obj)
    {
        string cell;
        int[] coord = new int[2];
        if (obj.tag == "Cell")
            cell = obj.name;
        else
            cell = obj.transform.parent.name;
        coord[0] = int.Parse(cell[0].ToString());
        coord[1] = int.Parse(cell[2].ToString());
        return coord;
    }
}