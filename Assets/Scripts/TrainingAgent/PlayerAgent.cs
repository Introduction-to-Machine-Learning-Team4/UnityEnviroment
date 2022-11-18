using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class PlayerAgent : Agent
{
    public PlayerMovementScript PMScript;
    public GameStateControllerScript GSCScript;
    void Start()
    {
        PMScript.OnGameOver += GameOver;
    }
    private void OnDestroy()
    {
        PMScript.OnGameOver -= GameOver;
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (PMScript == null) return;
        sensor.AddObservation(PMScript.transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(actions.DiscreteActions[0]);
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
