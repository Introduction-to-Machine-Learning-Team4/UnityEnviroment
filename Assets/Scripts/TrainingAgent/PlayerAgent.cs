using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using ACTIONS = PlayerMovementScript.ACTIONS; // for simplicity

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
    private float penaltyRate = 0.0001f;
    [SerializeField]
    private float maxPenalty = 0.01f;
    private float DeathPenalty = 0.5f;

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
        // total 67
        if (PMScript != null)
            PlayerObservation(sensor); // 2
        if (LCScript != null)
            LevelObservation(sensor); // 65

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

    private void LevelObservation(VectorSensor sensor)
    {
        var LinesDict = LCScript.Lines;
        var start_line = (int)PMScript.transform.position.z / 3;
        string s = "(";
        for (int i = -1; i <= 3; i++) // 13 per loop
        {
            int current_line = start_line + i;
            if (LinesDict.ContainsKey(current_line))
            {
                switch (LinesDict[start_line + i].tag)
                {
                    case "Road":
                        s += "1, ";
                        sensor.AddObservation(1); // 1
                        var olist = LinesDict[current_line].GetComponent<RoadCarGenerator>().GetObjectsList();
                        ObjectsObservation(sensor, olist, 1); // 12
                        break;
                    case "Water":
                        s += "2, ";
                        sensor.AddObservation(2);
                        olist = LinesDict[current_line].GetComponent<TrunkGeneratorScript>().GetObjectsList();
                        ObjectsObservation(sensor, olist, 2);
                        break;
                    case "Grass":
                        s += "0, ";
                        sensor.AddObservation(0);
                        ObjectsObservation(sensor, emptyList, 0);
                        break;
                }
            }
            else
            {
                s += "-1, ";
                sensor.AddObservation(-1);
                ObjectsObservation(sensor, emptyList, -1);
            }   
        }
        s += ")\n";
        //Debug.Log(s);
    }

    /// <summary>
    /// Add the obstacles to observation.Collect x and z coordinate of object.<br/>
    /// Pad with -10 if object not exist.<br/>
    /// Add 3 objects (size = 6) in total
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="olist"></param>
    private void ObjectsObservation(VectorSensor sensor,List<GameObject> olist,int LineType)
    {
        float pad = -10.0f;
        float obj_type;
        switch(LineType)
        {
            case 0:
                obj_type = 0;
                //pad = -10.0f;
                break;
            case 1:
                obj_type = 1;
                //pad = -20.0f;
                break;
            case 2:
                obj_type = 2;
                //pad = -30.0f;
                break;
            default:
                obj_type = 0;
                //pad = -100.0f;
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
                reward = 0.1f;
            else if (next == ACTIONS.DOWN)
                reward = -0.1f;
            if (Time.time - startTime > StartUpTime)
            {
                startup = true;
                lastUpdateTime = Time.time;
            }
        }
        else if (reward == 1f)
        {
            lastUpdateTime = Time.time; // Reset Timer
        }
        else if (currentStayTime > maxStayTime)
        {
            reward = -1f * penaltyRate * (currentStayTime - maxStayTime); // Penalty Increase
            reward = reward < -1f * maxPenalty ? -1f * maxPenalty : reward; // Cap
        }

        SetReward(reward);

        currentStayTime = Time.time - lastUpdateTime; // Update again on startup so that currentStayTime = 0
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
        if (DeathPenalty > 0.0f) DeathPenalty *= -1.0f;
        SetReward(DeathPenalty);
        EndEpisode();
    }

}
