using UnityEngine;
using System;

public class PlayerMovementScript : MonoBehaviour {
    public bool HumanPlay = false;
    public bool canMove = false;
    public float timeForMove = 0.2f;
    public float jumpHeight = 1.0f;

    public int minX = -4;
    public int maxX = 4;
    [SerializeField]
    private int bottomline = 12;

    public GameObject[] leftSide;
    public GameObject[] rightSide;

    public float leftRotation = -45.0f;
    public float rightRotation = 90.0f;

    private bool moving;
    private float elapsedTime;

    private Vector3 current;
    private Vector3 target;
    private float startY;

    private Rigidbody body;
    private GameObject mesh;

    private GameStateControllerScript gameStateController;
    private int score;
    public event Action OnGameOver;

    private float st = 0f;
    private float input_delay = 0.1f;
    public void Start() {
        current = transform.position;
        moving = false;
        startY = transform.position.y;

        body = GetComponentInChildren<Rigidbody>();

        mesh = GameObject.Find("Player/Chicken");

        score = 0;
        gameStateController = GameObject.Find("GameStateController").GetComponent<GameStateControllerScript>();
    }

    public void Update() {
        // If player is moving, update the player position, else receive input from user.
        if (moving)
            MovePlayer();
        else {
            // Update current to match integer position (not fractional).
            current = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
            //if (canMove && HumanPlay)
            //    KeyboardInput();
        }

        score = Mathf.Max(score, (int)current.z);
        gameStateController.score = score / 3;

        if (transform.transform.position.y < -5) GameOver();
    }

    /// <summary>
    /// Transfer Keyboard Input into Code
    /// </summary>
    private void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            InputTransfer(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            InputTransfer(2);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Mathf.RoundToInt(current.x) > minX)
                InputTransfer(3);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Mathf.RoundToInt(current.x) < maxX)
                InputTransfer(4);
        }

    }
    /// <summary>
    /// Transfer the Action values from agent to Code
    /// </summary>
    /// <param name="actionIndex"></param>
    /// <returns></returns>
    public float ActionHandle(int actionIndex)
    {
        float reward = 0f;
        if (IsMoving || !canMove || Time.time - st < input_delay) return 0f;
        reward = InputTransfer(actionIndex);
        if ((int)current.z > score) return 1f;
        return 0f;
    }

    /// <summary>
    /// Transfer Input from code to Moving actions
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private float InputTransfer(int code)
    {
        float reward = 0f;
        int successCheck = 0;
        switch (code)
        {
            case 1:
                successCheck = Move(new Vector3(0, 0, 3));
                break;
            case 2:
                successCheck = Move(new Vector3(0, 0, -3));
                break;
            case 3:
                if (Mathf.RoundToInt(current.x) > minX)
                    successCheck = Move(new Vector3(-3, 0, 0));
                break;
            case 4:
                if (Mathf.RoundToInt(current.x) < maxX)
                    successCheck = Move(new Vector3(3, 0, 0));
                break;
        }
        return reward;
    }

    /// <summary>
    /// Set the coordinate of destination and rotate the player
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private int Move(Vector3 distance) {
        var newPosition = current + distance;

        // Don't move if blocked by obstacle.
        if (Physics.CheckSphere(newPosition + new Vector3(0.0f, 0.5f, 0.0f), 0.1f)) 
            return 0;
        if (newPosition.z < score - bottomline)
            return 0;

        target = newPosition;

        moving = true;
        elapsedTime = 0;
        body.isKinematic = true;

        // Rotate Facing Direction
        switch (MoveDirection) {
            case "north":
                mesh.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case "south":
                mesh.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case "east":
                mesh.transform.rotation = Quaternion.Euler(0, 270, 0);
                break;
            case "west":
                mesh.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            default:
                break;
        }

        // Rotate arm and leg.
        foreach (var o in leftSide) {
            o.transform.Rotate(leftRotation, 0, 0);
        }

        foreach (var o in rightSide) {
            o.transform.Rotate(rightRotation, 0, 0);
        }

        return 1;
    }

    /// <summary>
    /// Moving animation of the player. Input has been turn off during animation 
    /// </summary>
    private void MovePlayer() {
        elapsedTime += Time.deltaTime;

        float weight = (elapsedTime < timeForMove) ? (elapsedTime / timeForMove) : 1;
        float x = Lerp(current.x, target.x, weight);
        float z = Lerp(current.z, target.z, weight);
        float y = Sinerp(current.y, startY + jumpHeight, weight);

        Vector3 result = new Vector3(x, y, z);
        transform.position = result; // note to self: why using transform produce better movement?
        //body.MovePosition(result);

        if (result == target) {
            st = Time.time;
            moving = false;
            current = target;
            body.isKinematic = false;
            body.AddForce(0, -10, 0, ForceMode.VelocityChange);

            // Return arm and leg to original position.
            foreach (var o in leftSide) {
                o.transform.rotation = Quaternion.identity;
            }

            foreach (var o in rightSide) {
                o.transform.rotation = Quaternion.identity;
            }
        }
    }

    private float Lerp(float min, float max, float weight) {
        return min + (max - min) * weight;
    }

    private float Sinerp(float min, float max, float weight) {
        return min + (max - min) * Mathf.Sin(weight * Mathf.PI);
    }

    public bool IsMoving {
        get { return moving; }
    }

    public string MoveDirection {
        get
        {
            if (moving) {
                float dx = target.x - current.x;
                float dz = target.z - current.z;
                if (dz > 0)
                    return "north";
                else if (dz < 0)
                    return "south";
                else if (dx > 0)
                    return "west";
                else
                    return "east";
            }
            else
                return null;
        }
    }

    public void GameOver() {
        canMove = false;
        OnGameOver?.Invoke();
    }

    public void Reset() {
        transform.position = new Vector3(0, 1, 0);
        transform.localScale = new Vector3(1, 1, 1);
        transform.rotation = Quaternion.identity;
        score = 0;
    }

}
