using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LineType = LevelControllerScript.LineType;

public class GridCollision : MonoBehaviour
{
    public enum Obstacles
    {
        Player = -1,
        Empty = 0,
        Car = 1,
        Trunk = 2,
    }

    LineType OnLine;
    Obstacles obs;

    [SerializeField]
    private bool isPlayer = false;

    private void Start()
    {
        if (isPlayer) obs = Obstacles.Player;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isLine = true;
        bool isObstacle = true;
        if (other.CompareTag("Player") || isPlayer) return;
        switch(other.tag)
        {
            case "Road":
                OnLine = LineType.Road;
                break;
            case "Water":
                OnLine = LineType.Water;
                break;
            case "Grass":
                OnLine = LineType.Grass;
                break;
            default:
                isLine = false;
                break;
        }
        switch(other.tag)
        {
            case "Car":
                obs = Obstacles.Car;
                break;
            case "Trunk":
                obs = Obstacles.Trunk;
                break;
            default:
                isObstacle = false;
                break;
        }

        if(!isLine && !isObstacle)
        {
            OnLine = LineType.Others;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || isPlayer) return;
        switch (other.tag)
        {
            case "Car":
                obs = Obstacles.Empty;
                break;
            case "Trunk":
                obs = Obstacles.Empty;
                break;
        }
    }

    public float GetGridValue()
    {
        if (obs == Obstacles.Player) return -1.0f;
        if (obs == Obstacles.Car) return 1.0f;
        if (OnLine == LineType.Water && obs == Obstacles.Trunk) return 0.0f;
        if (OnLine == LineType.Water) return 2.0f;
        return 0.0f;
    }
    /* Player -1
     * Grass,Empty Road / Trunk on Water 0
     * Car on Road 1
     * Water 2
     */
}
