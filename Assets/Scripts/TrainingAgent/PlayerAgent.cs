using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using ACTIONS = PlayerMovementScript.ACTIONS; // for simplicity
using LineType = LevelControllerScript.LineType;

public class PlayerAgent : Agent
{
    public PlayerMovementScript PMScript;
    public GameStateControllerScript GSCScript;
    public LevelControllerScript LCScript;

    [SerializeField]
    private float maxStayTime = 5.0f;
    [SerializeField]
    private float maxResetTime = 30.0f;
    [SerializeField]
    private float StayPenaltyRate = 0.0001f;
    [SerializeField]
    private float MaxStayPenalty = 0.001f;
    [SerializeField]
    private float DeathPenaltyRate = 0.8f;


    private float lastUpdateTime = 0.0f;
    private bool startup = false;
    private float StartUpTime = 15.0f;
    private float startTime = 0.0f;

    private List<GameObject> emptyList;
    private int GridSize = 7;
    private float GridUnit = 3.0f;
    void Start()
    {
        PMScript.OnGameOver += GameOver;
        PMScript.OnDecision += RequestDecision;
        emptyList = new List<GameObject>();
    }
    private void OnDestroy()
    {
        PMScript.OnGameOver -= GameOver;
        PMScript.OnDecision -= RequestDecision;
    }

    public override void OnEpisodeBegin()
    {
        PMScript.canMove = false;
        if(!GSCScript.isReset)
            GSCScript.ResetGame();
        startTime = Time.time;
        lastUpdateTime = Time.time;
        startup = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // total 49
        if (LCScript != null)
            LevelObservation(sensor, -GridSize/2, GridSize);
    }

    private void LevelObservation(VectorSensor sensor, int start_idx, int line_count)
    {
        var LinesDict = LCScript.Lines;
        var start_line = (int)PMScript.transform.position.z / 3;
        start_line = start_line + start_idx;
        var end_line = start_line + line_count - 1;

        List<float> Grid = new List<float>();
        for (int current_line = start_line; current_line <= end_line; current_line++)
        {
            bool HasPlayer = ((int)PMScript.transform.position.z / 3 == current_line)? true : false;
            if (LinesDict.ContainsKey(current_line))
            {
                switch (LinesDict[current_line].tag)
                {
                    case "Road":
                        var olist = LinesDict[current_line].GetComponent<RoadCarGenerator>().GetObjectsList();
                        Grid.AddRange(ObjectsObservation(sensor, olist, LineType.Road, HasPlayer));
                        break;
                    case "Water":
                        olist = LinesDict[current_line].GetComponent<TrunkGeneratorScript>().GetObjectsList();
                        Grid.AddRange(ObjectsObservation(sensor, olist, LineType.Water, HasPlayer));
                        break;
                    case "Grass":
                        Grid.AddRange(ObjectsObservation(sensor, emptyList, LineType.Grass, HasPlayer));
                        break;
                }
            }
            else
            {
                Grid.AddRange(ObjectsObservation(sensor, emptyList, LineType.Others, HasPlayer));
            }
        }

        sensor.AddObservation(Grid);

        string s = "\n";
        int br = 0;
        foreach(var val in Grid)
        {
            if(br == GridSize)
            {
                s += "\n";
                br = 0;
            }
            s += val.ToString();
            s += ", ";
            br++;
        }
        //Debug.Log(s);
    }

    /// <summary>
    /// Add the obstacles to observation.Collect x and z coordinate of object.<br/>
    /// Pad with -10 if object not exist.<br/>
    /// Add 3 objects (size = 6) in total
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="olist"></param>
    private List<float> ObjectsObservation(VectorSensor sensor,List<GameObject> olist,LineType type,bool HasPlayer)
    {
        List<float> LineGrid = new List<float>();
        float fill = (type == LineType.Water) ? 2.0f : 0.0f;
        for (int i = 0; i < GridSize; i++)
        {
            if (HasPlayer && i == GridSize / 2)
                LineGrid.Add(-1);
            else
                LineGrid.Add(fill);
        }

        float player_x = PMScript.transform.position.x;
        float GridBound = GridSize * GridUnit / 2.0f;
        for (int j = 0; j < olist.Count; j++)
        {

            // Get Boundary
            Transform BoundaryL = olist[j].transform.Find("BoundaryL"); // side on local axis
            Transform BoundaryR = olist[j].transform.Find("BoundaryR");
            float left = 0; // side on global axis
            float right = 0;
            if (BoundaryL.position.x < BoundaryR.position.x)
            {
                left = BoundaryL.position.x;
                right = BoundaryR.position.x;
            }
            else
            {
                left = BoundaryR.position.x;
                right = BoundaryL.position.x;
            }
            left -= player_x; // Coordinate respective to player
            right -= player_x;
            if (right < -GridBound || left > GridBound) continue; // out of grid
            //Debug.Log(left.ToString() + " " + right.ToString());


            left += GridBound; // relocate to positive for easier to assign in list
            right += GridBound;
            left /= GridUnit; // Normalize
            right /= GridUnit;
            // Replace Grid Value
            if (type == LineType.Water)
            {
                for (int i = Mathf.FloorToInt(left); i <= right; i++)
                {
                    if (i < 0 || i >= GridSize) continue;
                    LineGrid[i] = 0;
                }

            }
            else if(type == LineType.Road)
            {
                for (int i = Mathf.FloorToInt(left); i <= right; i++)
                {
                    if (i < 0 || i >= GridSize) continue;
                    LineGrid[i] = 1;
                }
            }
        }
        return LineGrid;
    }

    /// <summary>
    /// Decode the action and caculate the reward. <br />
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        ACTIONS next = InputTransfer(actions.DiscreteActions[0]);
        var reward = PMScript.ActionHandle(next);

        var currentStayTime = Time.time - lastUpdateTime;
        if (!startup)
        {
            if (next == ACTIONS.UP)
                reward += 0.2f;
            else if (next == ACTIONS.DOWN)
                reward += -0.1f;
            if (Time.time - startTime > StartUpTime)
            {
                startup = true;
                lastUpdateTime = Time.time;
                currentStayTime = 0;
            }
        }
        else if (reward == 1f)
        {
            lastUpdateTime = Time.time; // Reset Timer
        }
        else if (currentStayTime > maxStayTime)
        {
            var penalty = StayPenaltyRate * (currentStayTime - maxStayTime); // Penalty Increase
            penalty = (penalty > MaxStayPenalty) ? MaxStayPenalty : penalty; // Cap
            reward -= penalty;
        }

        SetReward(reward);

        if (currentStayTime > maxResetTime && startup)
        {
            EndEpisode();
        }

        //Debug.Log(currentStayTime);
        //Debug.Log(actions.DiscreteActions[0]);
        //Debug.Log(reward);
        //Debug.Log(GetCumulativeReward());
    }

    /// <summary>
    /// Encode the physical keyboard input to code
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var DiscreteActionOut = actionsOut.DiscreteActions;
    
        if (Input.GetKey(KeyCode.UpArrow))
        {
            DiscreteActionOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            DiscreteActionOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            DiscreteActionOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            DiscreteActionOut[0] = 4;
        }
        else
            DiscreteActionOut[0] = 0;
    }

    /// <summary>
    /// Decode the code to corresponding enum type <br />
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public ACTIONS InputTransfer(int code)
    {
        ACTIONS move = ACTIONS.STOP;
        switch (code)
        {
            case 0:
                return ACTIONS.STOP;
            case 1:
                return ACTIONS.UP;
            case 2:
                return ACTIONS.DOWN;
            case 3:
                return ACTIONS.LEFT;
            case 4:
                return ACTIONS.RIGHT;
        }
        return move;
    }

    // Listener
    private void GameOver()
    {
        float score = PMScript.GetScore();
        SetReward(-1 * DeathPenaltyRate * score); // Penalty Increase over score
        EndEpisode();
    }

}
