using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Pathfinding : MonoBehaviour
{
    private List<Vector2Int> path = new List<Vector2Int>();
   
    private Vector2Int next;
    private Vector2Int current;

    [Header("Start and Goal Positions")]
    public Vector2Int start = new Vector2Int(0, 1);
    public Vector2Int goal = new Vector2Int(4, 4);

    [Header("Obstacle Add")]
    public Vector2Int ObstacleAdd;

    [Header("Probability")]
    [Range(0f, 100f)]
    public float Probability;

    [Header("Grid sizes")]
    [Range(0,20)]
    public int Xsize;
    [Range(0,20)]
    public int Ysize;


    private HashSet<Vector2Int> addedObstacles = new HashSet<Vector2Int>();
    private float lastProbability = -1f; // a value so that prob can constantly change in scene

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private int[,] grid = new int[,] { };
    /* private int[,] grid = new int[,]
     {

         { 0, 1, 0, 0, 0 },
         { 0, 1, 0, 1, 0 },
         { 0, 0, 0, 1, 0 },
         { 0, 1, 1, 1, 0 },
         { 0, 0, 0, 0, 0 } 
     };*/

    

    private void OnValidate()
    {
        // Only regenerate grid if needed (e.g., Xsize, Ysize, Probability changed)
        if (grid == null || grid.GetLength(0) != Ysize || grid.GetLength(1) != Xsize || Probability != lastProbability)
        {
            GenerateRandomGrid(Xsize, Ysize, Probability);
        }
   
        FindPath(start, goal);
        AddObstacle(ObstacleAdd);
        lastProbability = Probability;
    }
    private void OnDrawGizmos()
    {
        // Ensure the grid is initialized
        if (grid == null)
        {
            // Optionally, initialize it with default values if desired
            GenerateRandomGrid(Xsize, Ysize, Probability);
            return; // Exit early if grid is not initialized
        }
        float cellSize = 1f;

        // Draw grid cells
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        // Draw path
        foreach (var step in path)
        {
            Vector3 cellPosition = new Vector3(step.x * cellSize, 0, step.y * cellSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
        }

        // Draw start and goal
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(start.x * cellSize, 0, start.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goal.x * cellSize, 0, goal.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        
    }

    private bool IsInBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < grid.GetLength(1) && point.y >= 0 && point.y < grid.GetLength(0);
    }

    private void FindPath(Vector2Int start, Vector2Int goal)
    {
        path.Clear();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (Vector2Int direction in directions)
            {
                next = current + direction;

                if (IsInBounds(next) && grid[next.y, next.x] == 0 && !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log("Path not found.");
            return;
        }

        // Trace path from goal to start
        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();
    }

    public void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        grid = new int[height, width];


        // Loop through each cell in the grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float randomValue = Random.Range(0f, 100f);
                grid[y, x] = randomValue < obstacleProbability ? 1 : 0;
            }
        }
        // Restore previously added obstacles
        foreach (var obstacle in addedObstacles)
        {
            if (IsInBounds(obstacle))
            {
                grid[obstacle.y, obstacle.x] = 1; // Ensure obstacle is set
            }
        }
        FindPath(start, goal); // Recalculate path with new grid
        
    }


    public void AddObstacle(Vector2Int position)
    {
        if (IsInBounds(position) && grid[position.y, position.x] == 0)
        {
            // Place the obstacle in the grid
            grid[position.y, position.x] = 1; // Mark the grid cell as an obstacle
            addedObstacles.Add(position);
            FindPath(start, goal);
        }
        else
        {
            Debug.Log("Invalid position for an obstacle or already occupied.");
        }  
    }
    public void EraseAddedObstacles()
    {
        foreach (var obstacle in addedObstacles)
        {
            if (IsInBounds(obstacle))
            {
                grid[obstacle.y, obstacle.x] = 0; // Remove the obstacle from the grid
            }
        }
        addedObstacles.Clear(); // Clear the set of added obstacles
       
        Debug.Log("Added obstacles erased.");
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(Pathfinding))]
    public class PathfindingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw default inspector fields

            Pathfinding pathfinding = (Pathfinding)target;

            // Button to erase added obstacles
            if (GUILayout.Button("Erase Added Obstacles"))
            {
                pathfinding.EraseAddedObstacles(); // Call the method when button is clicked
            }
        }
    }
#endif
}
