using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

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
        GSCScript.ResetGame();
        startTime = Time.time;
        startup = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (PMScript != null)
            PlayerObservation(sensor); // 2
        if (LCScript != null)
            LevelObservation(sensor); // 28
    }

    private void PlayerObservation(VectorSensor sensor)
    {
        sensor.AddObservation(PMScript.transform.position.x);
        sensor.AddObservation(PMScript.transform.position.z);
    }

    private void LevelObservation(VectorSensor sensor)
    {
        var LinesDict = LCScript.Lines;
        var current_z = (int)PMScript.transform.position.z;
        for(int i = -1; i <= 2; i++) // 7 per loop , total size = 28
        {
            var current_line = current_z + i;
            if (LinesDict.ContainsKey(current_line))
            {
                switch (LinesDict[current_z + i].tag)
                {
                    case "Road":
                        sensor.AddObservation(1);
                        var olist = LinesDict[current_line].GetComponent<RoadCarGenerator>().GetObjectsList();
                        ObjectsObservation(sensor, olist); // 6
                        break;
                    case "Water":
                        sensor.AddObservation(2);
                        olist = LinesDict[current_line].GetComponent<TrunkGeneratorScript>().GetObjectsList();
                        ObjectsObservation(sensor, olist);
                        break;
                    case "Grass":
                        sensor.AddObservation(0);
                        ObjectsObservation(sensor, emptyList);
                        break;
                }
            }
            else
            {
                sensor.AddObservation(-1);
                ObjectsObservation(sensor, emptyList);
            }
        }
    }

    /// <summary>
    /// Add the obstacles to observation.Collect x and z coordinate of object.<br/>
    /// Pad with -100 if object not exist.<br/>
    /// Add 3 objects (size = 6) in total
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="olist"></param>
    private void ObjectsObservation(VectorSensor sensor,List<GameObject> olist)
    {
        for (int j = 0; j < 3; j++)
        {
            if (j < olist.Count)
            {
                Vector3 pos = olist[j].transform.position;
                sensor.AddObservation(pos.x);
                sensor.AddObservation(pos.z);
            }
            else
            { 
                sensor.AddObservation(-10.0f + Random.Range(-0.1f,0.1f));
                sensor.AddObservation(-10.0f + Random.Range(-0.1f,0.1f));
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var reward = PMScript.ActionHandle(actions.DiscreteActions[0]);

        var currentStayTime = Time.time - lastUpdateTime;
        if (!startup)
        {
            if (actions.DiscreteActions[0] == 1)
                reward = 0.1f;
            else if (actions.DiscreteActions[0] != 0)
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

        currentStayTime = Time.time - lastUpdateTime; // Update again on startup
        if (currentStayTime > maxResetTime && startup)
        {
            EndEpisode();
        }

        //Debug.Log(actions.DiscreteActions[0]);
        //Debug.Log(GetCumulativeReward());
    }
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
    private void GameOver()
    {
        SetReward(-1f);
        EndEpisode();
    }


}
