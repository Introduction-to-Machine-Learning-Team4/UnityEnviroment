using UnityEngine;
using System.Collections;

public class CameraMovementScript : MonoBehaviour {

    public float minZ = 0.0f;
    public float speedIncrementZ = 0.0f;
    public float speedOffsetZ = 4.0f;
    public bool moving = false;

    private GameObject player;
    private Camera Cam;

    private Vector3 offset;
    private Vector3 initialOffset;

    public void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        Cam = GetComponent<Camera>();
    }
	
    public void Update() {

	}

    public void Reset() {
        moving = false;
    }
}
