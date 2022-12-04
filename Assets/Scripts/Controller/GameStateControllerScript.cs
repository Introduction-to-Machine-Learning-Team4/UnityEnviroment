using UnityEngine;
using UnityEngine.UI;

public class GameStateControllerScript : MonoBehaviour {
    public PlayerMovementScript PMScript;
    public GameStateControllerScript GSCSript;
    public LevelControllerScript LCScript;
    public CameraMovementScript CMScript;

    public GameObject mainMenuCanvas;
    public GameObject playCanvas;
    public GameObject gameOverCanvas;

    public Text playScore;
    public Text gameOverScore;
    public Text topScore;
    public Text playerName;

    public int score, top;

    private GameObject currentCanvas;
    private string state;


    public void Start() {
        currentCanvas = null;
        PMScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementScript>();
        Play();
    }
    public void OnDestroy()
    {
    }

    public void Update() {
        if (state == "play") {
            topScore.text = PlayerPrefs.GetInt("Top").ToString();
            playScore.text = score.ToString();
            playerName.text = PlayerPrefs.GetString("Name");
        }
    }

    public void Play() {
        CurrentCanvas = playCanvas;
        state = "play";
        score = 0;

        PMScript.canMove = true;
    }

    public void ResetGame() {
        state = "gameover";

        CMScript.Reset();
        LCScript.Reset();
        PMScript.Reset();
        
        Play();
    }

    private GameObject CurrentCanvas {
        get {
            return currentCanvas;
        }
        set {
            if (currentCanvas != null) {
                currentCanvas.SetActive(false);
            }
            currentCanvas = value;
            currentCanvas.SetActive(true);
        }
    }
}
