using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelControllerScript : MonoBehaviour {
    public int minZ = 3;
    public int lineAhead = 40;
    public int lineBehind = 20;

    public GameObject[] linePrefabs;

    private Dictionary<int, GameObject> lines;
    private List<GameObject> lineList;
    private GameObject player;

    public void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        lines = new Dictionary<int, GameObject>();
        lineList = new List<GameObject>();
	}
	
    public void Update() {
        // Generate lines based on player position.
        var playerZ = (int)player.transform.position.z;
        for (var z = Mathf.Max(minZ, playerZ - lineBehind); z <= playerZ + lineAhead; z += 1) {
            if (!lines.ContainsKey(z)) {

                var line = Instantiate(
                    linePrefabs[Random.Range(0, linePrefabs.Length)],
                    new Vector3(0, 0, z * 3 - 5), 
                    Quaternion.identity
                );

                line.transform.localScale = new Vector3(1, 1, 3);
                lines.Add(z, line);
                lineList.Add(line);
            }
        }

        // Remove lines based on player position.
        List<GameObject> remove = new List<GameObject>();
        foreach(var line in lineList)
        {
            if (line == null) continue;
            var lineZ = line.transform.position.z;
            if (lineZ < playerZ - lineBehind)
            {
                Destroy(line);
                remove.Add(line);
                lines.Remove((int)lineZ);
            }
        }
        lineList.RemoveAll(x => remove.Contains(x));
	}

    public void Reset() {
        foreach (var line in lineList)
            Destroy(line);
        lineList = new List<GameObject>();
        lines = new Dictionary<int, GameObject>();
    }
}
