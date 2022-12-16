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
    public GridPlot gridScript;

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
        //if (gridScript != null)
        //    sensor.AddObservation(gridScript.GetGrid());

        if (PMScript != null)
            PlayerObservation(sensor); // 2
        if (LCScript != null)
            LevelObservation(sensor, -1, 5); // 65

        //string s = "(";
        //foreach (var i in sensor.GetObservations())
        //{
        //    s += i.ToString();
        //    s += ", ";
        //}
        //s += ")";
        //Debug.Log(s);

    }

    private void PlayerObservation(VectorSensor sensor)
    {
        sensor.AddObservation(PMScript.transform.position.x);
        sensor.AddObservation(PMScript.transform.position.z);
    }

    private void LevelObservation(VectorSensor sensor, int start_idx, int line_count)
    {
        var LinesDict = LCScript.Lines;
        var start_line = (int)PMScript.transform.position.z / 3;
        start_line = start_line + start_idx;
        var end_line = start_line + line_count - 1;

        LineTypeObservation(sensor, start_line, end_line);
        LineObservation(sensor, start_line, end_line);
    }

    /// <summary>
    /// Add the line type to observation
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="start_line"></param>
    /// <param name="end_line"></param>
    private void LineTypeObservation(VectorSensor sensor,int start_line,int end_line)
    {
        string s = "";
        var LinesDict = LCScript.Lines;
        for (int current_line = start_line; current_line <= end_line; current_line++)
        {
            if (LinesDict.ContainsKey(current_line))
            {
                switch (LinesDict[current_line].tag)
                {
                    case "Road":
                        s += "1, ";
                        sensor.AddObservation((float)LineType.Road);
                        break;
                    case "Water":
                        s += "2, ";
                        sensor.AddObservation((float)LineType.Water);
                        break;
                    case "Grass":
                        s += "0, ";
                        sensor.AddObservation((float)LineType.Grass);
                        break;
                }
            }
            else
            {
                s += "-1, ";
                sensor.AddObservation((float)LineType.Others);
            }
        }
        //Debug.Log(s);
    }
    
    /// <summary>
    /// Add the Obstacles on the line to observation
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="start_line"></param>
    /// <param name="end_line"></param>
    private void LineObservation(VectorSensor sensor, int start_line, int end_line)
    {
        var LinesDict = LCScript.Lines;
        for (int current_line = start_line; current_line <= end_line; current_line++)
        {
            if (LinesDict.ContainsKey(current_line))
            {
                switch (LinesDict[current_line].tag)
                {
                    case "Road":
                        var olist = LinesDict[current_line].GetComponent<RoadCarGenerator>().GetObjectsList();
                        ObjectsObservation(sensor, olist, LineType.Road);
                        break;
                    case "Water":
                        olist = LinesDict[current_line].GetComponent<TrunkGeneratorScript>().GetObjectsList();
                        ObjectsObservation(sensor, olist, LineType.Water);
                        break;
                    case "Grass":
                        ObjectsObservation(sensor, emptyList, LineType.Grass);
                        break;
                }
            }
            else
            {
                ObjectsObservation(sensor, emptyList, LineType.Others);
            }
        }
    }


    /// <summary>
    /// Add the obstacles to observation.Collect x and z coordinate of object.<br/>
    /// Pad with -10 if object not exist.<br/>
    /// Add 3 objects (size = 6) in total
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="olist"></param>
    private void ObjectsObservation(VectorSensor sensor,List<GameObject> olist,LineType type)
    {
        float pad = -10.0f;
        float obj_type;
        switch(type)
        {
            case LineType.Grass:
                obj_type = 0;
                break;
            case LineType.Road:
                obj_type = 1;
                break;
            case LineType.Water:
                obj_type = 2;
                break;
            default:
                obj_type = 0;
                break;
        }
        for (int j = 0; j < 3; j++) // 4 per loop, total 12
        {
            if (j < olist.Count)
            {
                Vector3 pos = olist[j].transform.position;
                sensor.AddObservation(obj_type);
                sensor.AddObservation(pos.x);
                sensor.AddObservation(pos.z);
                sensor.AddObservation(olist[j].transform.localScale.x);
            }
            else
            {
                // padding if no objects
                sensor.AddObservation(0);
                sensor.AddObservation(pad + Random.Range(-0.1f,0.1f));
                sensor.AddObservation(pad + Random.Range(-0.1f,0.1f));
                sensor.AddObservation(pad + Random.Range(-0.1f, 0.1f));
            }
        }
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
