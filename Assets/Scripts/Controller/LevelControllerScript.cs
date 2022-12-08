using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelControllerScript : MonoBehaviour {
    public int lineAhead = 12;
    public int lineBehind = 8;
    private const int unit = 3; // line width = 3
    private const int offset = 1;
    private int generated_idx;

    public GameObject[] linePrefabs;
    public Dictionary<int, GameObject> Lines { get; private set; }
    private GameObject player;

    public void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        Lines = new Dictionary<int, GameObject>();
        generated_idx = 1;
    }
	
    public void Update() {
        // Generate lines based on player position.
        var current_idx = (int)player.transform.position.z/unit;
        while (generated_idx <= current_idx + lineAhead)
        {
            var line = Instantiate(
                linePrefabs[Random.Range(0, linePrefabs.Length)],
                new Vector3(0, 0, generated_idx * unit + offset),
                Quaternion.identity
            );
            line.transform.localScale = new Vector3(1, 1, 3);
            Lines.Add(generated_idx, line);
            generated_idx ++ ;
        }

        // Remove lines based on player position.

        List<int> remove_keys = new List<int>();
        foreach (var line in Lines)
        {
            if( line.Key < current_idx - lineBehind)
            {
                Destroy(line.Value);
                remove_keys.Add(line.Key);
            }
        }
        foreach(var key in remove_keys)
        {
            Lines.Remove(key);
        }

	}

    public void Reset() {
        foreach (var line in Lines.Values)
        {
            Destroy(line);
        }
        Lines = new Dictionary<int, GameObject>();
        generated_idx = 1;
    }
}
