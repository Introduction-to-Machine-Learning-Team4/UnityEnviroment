using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlot : MonoBehaviour
{
    List<GameObject> Rows;
    // Start is called before the first frame update
    void Start()
    {
        Rows = new List<GameObject>();
        foreach(Transform row in transform)
        {
            Rows.Add(row.gameObject);
        }
    }

    public List<float> GetGrid()
    {
        string s = "\n";
        List<float> grids = new List<float>();
        foreach (var row in Rows)
        {
            foreach(Transform grid in row.transform)
            {
                var grid_value = grid.GetComponent<GridCollision>().GetGridValue();
                grids.Add(grid_value);
                s += grid_value.ToString();
                s += ", ";
            }
            s += "\n";
        }
        Debug.Log(s);
        return grids;
    }
}
