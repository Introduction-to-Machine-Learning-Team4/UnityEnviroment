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
                sensor.AddObservation(-100.0f);
                sensor.AddObservation(-100.0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var reward = PMScript.ActionHandle(actions.DiscreteActions[0]);
        SetReward(reward);
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
