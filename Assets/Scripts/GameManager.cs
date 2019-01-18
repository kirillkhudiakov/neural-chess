using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject StartingCanvas;
    public GameObject MainCanvas;
    public GameObject PlayAgainButton;
    public GameObject QuitGameButton;
    public GameObject ExitText;
    public Position Board;

    Animator CamAnimator;
    Text AlarmText;

    public int Side { get; set; }
    public bool IsPiecePicked { get; set; }
    public GameObject PiecePicked;
    public bool UserMove { get; set; }
    public bool GameOver { get; set; }

    public delegate void Selection();
    public event Selection WhiteSelection;
    public event Selection BlackSelection;

    public GameObject WhiteQueenPrefab;
    public GameObject BlackQueenPrefab;

    public AudioSource GameOverAudio;
    public AudioSource TapButtonAudio;
    public AudioSource WhiteStepAudio;
    public AudioSource BlackStepAudio;
    
    public int Complexity { get; set; }


    // Use this for initialization
    void Start()
    {
        Complexity = 1;
        UserMove = false;
        GameOver = false;

        StartingCanvas.SetActive(true);
        CamAnimator = FindObjectOfType<Camera>().GetComponent<Animator>();
        Board = new Position();
        Board.Promotion += MakePromotion;
        Board.StepDone += PlayStepSFX;
        AlarmText = GameObject.Find("Alarm Text").GetComponent<Text>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }

    public void BlackSelected()
    {
        TapButtonAudio.Play();
        StartingCanvas.SetActive(false);
        CamAnimator.SetTrigger("GoBlack");
        Side = -1;
        Invoke("ShowExitText", 2.5F);
        BlackSelection();
        UserMove = false;
    }

    public void WhiteSelected()
    {
        TapButtonAudio.Play();
        StartingCanvas.SetActive(false);
        CamAnimator.SetTrigger("GoWhite");
        Side = 1;
        Invoke("ShowExitText", 2.5F);
        WhiteSelection();
        UserMove = true;
    }

    public void StartNewGame()
    {
        TapButtonAudio.Play();
        SceneManager.LoadScene("Main Scene");
    }

    public void QuitGame()
    {
        TapButtonAudio.Play();
        Application.Quit();
    }

    public void ShowExitText()
    {
        ExitText.SetActive(true);
    }

    public void CheckResult()
    {
        if (Board.IsMate(true) || Board.IsMate(false) || Board.IsStal)
        {
            GameOver = true;
            ExitText.SetActive(false);
            PlayAgainButton.SetActive(true);
            QuitGameButton.SetActive(true);
            AlarmText.enabled = true;
            GameOverAudio.Play();
            if (Board.IsStal)
            {
                AlarmText.text = "STALEMATE";
            }
            else if (Board.IsMate(true))
            {
                if (Side == 1)
                    AlarmText.text = "YOU LOSE";
                else
                    AlarmText.text = "YOU WIN";
            }
            else if (Board.IsMate(false))
            {
                if (Side == -1)
                    AlarmText.text = "YOU LOSE";
                else
                    AlarmText.text = "YOU WIN";
            }
        }
        else if (Board.IsCheck(true))
        {
            AlarmText.enabled = true;
            AlarmText.text = "White under check";
        }
        else if (Board.IsCheck(false))
        {
            AlarmText.enabled = true;
            AlarmText.text = "Black under check";
        }
        else if (AlarmText.enabled)
        {
            AlarmText.enabled = false;
        }
    }

    public void MakePromotion(int x, int y)
    {
        var cell = Coordinates.FindCellByCoord(x, y);
        var pawn = cell.transform.GetChild(0).gameObject;
        bool isWhite = pawn.tag == "White Piece";
        Destroy(pawn);

        var point = Coordinates.GetCoord(cell.name, false);
        GameObject queen;
        if (isWhite)
            queen = Instantiate(WhiteQueenPrefab, point, Quaternion.Euler(-90, 0, 0));
        else
            queen = Instantiate(BlackQueenPrefab, point, Quaternion.Euler(-90, 0, 0));
        queen.transform.parent = cell.transform;
    }

    void PlayStepSFX(int side)
    {
        if (side == 1)
        {
            WhiteStepAudio.Play();
        }
        else
        {
            BlackStepAudio.Play();
        }
    }

    public void ComplexityChanged(int index)
    {
        Complexity = index;
    }
}