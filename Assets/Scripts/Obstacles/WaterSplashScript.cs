using UnityEngine;
using System.Collections;

public class WaterSplashScript : MonoBehaviour {


    void OnTriggerEnter(Collider other)
    {
        // Ignores other colliders unless it is player
        if (other.tag == "Player")
        {

            other.gameObject.GetComponent<PlayerMovementScript>().GameOver();
        }
    }

}
