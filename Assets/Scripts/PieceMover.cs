using UnityEngine;

public class PieceMover : MonoBehaviour
{
    bool NeedToMove = false, PieceIsMoving = false;
    Vector3 StartPoint, EndPoint;
    float CurrentTime;

    // Update is called once per frame
    void Update()
    {
        if (NeedToMove)
        {
            NeedToMove = false;
            PieceIsMoving = true;
            CurrentTime = 0;
            StartPoint = transform.position;
        }
        if (PieceIsMoving)
        {
            transform.position = Vector3.Lerp(StartPoint, EndPoint, CurrentTime);
            CurrentTime += Time.deltaTime;
            if (CurrentTime > 1)
            {
                PieceIsMoving = false;
            }
        }
    }

    public void SmoothMove(Vector3 target)
    {
        EndPoint = target;
        NeedToMove = true;
    }
}