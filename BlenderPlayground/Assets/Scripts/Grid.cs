using UnityEngine;
using System.Collections;

//this is straight from: https://unity3d.college/2017/10/08/simple-unity3d-snap-grid-system/
public class Grid : MonoBehaviour {

    [SerializeField]
    private bool DEBUG_DRAW_GRID = false;
    private float cell_size = 0.5f;
    private float extent = 1000f;
    public float min_grid_size = 0.001f;

    public float getGridSize() { return cell_size; }

    public int setGridSize(float new_cell_size)
    {
        if(new_cell_size < min_grid_size)
        {
            Debug.LogWarning("cannot set grid cell size to be less than " + min_grid_size + "mm squared.");
            return 0;
        }
        else
        {
            cell_size = new_cell_size;
            return 1;
        }
    }

    public Vector3 GetNearestPointOnGrid(Vector3 position, bool clamp_y=true)
    {
        position -= transform.position;

        int xCount = Mathf.RoundToInt(position.x / cell_size);
        int yCount = Mathf.RoundToInt(position.y / cell_size);
        int zCount = Mathf.RoundToInt(position.z / cell_size);

        Vector3 result = new Vector3(
            (float)xCount * cell_size,
            (float)yCount * cell_size,
            (float)zCount * cell_size
        );

        result += transform.position;
        if (clamp_y)
        {
            result = new Vector3(result.x, 0, result.z);
        }

        return result;
    }

    private void OnDrawGizmos()
    {
        if (DEBUG_DRAW_GRID)
        {
            Gizmos.color = Color.yellow;
            float cells = 40;
            for (float x = 0; x < cells; x += cell_size)
            {
                for (float z = 0; z < cells; z += cell_size)
                {
                    var point = GetNearestPointOnGrid(new Vector3(x, 0f, z)) + new Vector3(-20, 0, -20);
                    Gizmos.DrawSphere(point, 0.1f);
                }

            }
        }
    }
}
