using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Pathfinding : MonoBehaviour
{
    private List<Vector2Int> path = new List<Vector2Int>();
   
    private Vector2Int next;
    private Vector2Int current;

    [Header("Start Positions")]
    [Range(0, 20)]
    public int startX = 0;
    [Range(0, 20)]
    public int startY = 1;
    [Header("Goal Positions")]
    [Range(0, 20)]
    public int goalX = 4;
    [Range(0, 20)]
    public int goalY = 4;

    [Header("Obstacle Add")]

    [SerializeField]
    private List<Obstacle> addedObstacles = new List<Obstacle>();
    private List<Vector2Int> previousObstacles = new List<Vector2Int>();


    [Header("Probability")]
    [Range(0f, 100f)]
    public float Probability;

    [Header("Grid sizes")]
    [Range(0,20)]
    public int Xsize;
    [Range(0,20)]
    public int Ysize;
    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };


    private int[,] grid = new int[,] { };
    private float lastProbability = -1f;
    [System.Serializable]
    private class Obstacle
    {
        [Range(0, 20)]
        public int x;
        [Range(0, 20)]
        public int y;
    }

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
        // Only generate a grid if size or probability has changed
        if (grid == null || grid.GetLength(0) != Ysize || grid.GetLength(1) != Xsize || Probability != lastProbability )
        {
            GenerateRandomGrid(Xsize, Ysize, Probability);
            lastProbability = Probability;
    
        }
        UpdateObstacles();
        FindPath(new Vector2Int(startX, startY), new Vector2Int(goalX, goalY));
    }
    private void OnDrawGizmos()
    {
        // Ensure the grid is initialized
        if (grid == null)
        {
            // Optionally, initialize it with default values if desired
            GenerateRandomGrid(Xsize, Ysize, Probability);
            foreach (var obstacle in addedObstacles)
            {
                ApplyObstacle(new Vector2Int(obstacle.x, obstacle.y));
            }
        }
        float cellSize = 1f;

        // Draw grid cells
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                Gizmos.color =  grid[y, x] == 0 ? Color.white :
                                grid[y, x] == 1 ? Color.black :
                                grid[y, x] == 2 ? Color.yellow :
                                Color.cyan;
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
        Gizmos.DrawCube(new Vector3(startX * cellSize, 0, startY * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goalX * cellSize, 0, goalY * cellSize), new Vector3(cellSize, 0.1f, cellSize));
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
            Xsize = width;
            Ysize = height;
        
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
                
                    float randomValue = Random.Range(0f, 100f);
                    grid[y, x] = randomValue < obstacleProbability ? 1 : 0;
            }
        }
      
    }
    private void ApplyObstacle(Vector2Int position)
    {
        if (IsInBounds(position) && grid[position.y, position.x] == 0) // Only apply if the cell is empty
        {
            grid[position.y, position.x] = 2; // Mark as a yellow obstacle
        }
    }
    private void ClearObstacle(Vector2Int position)
    {
        if (IsInBounds(position) && grid[position.y, position.x] == 2) // Clear only if it’s a yellow obstacle
        {
            grid[position.y, position.x] = 0; // Clear the obstacle
        }
    }


    private void UpdateObstacles()
    {
        // Clear previous obstacles
        foreach (var obstacle in previousObstacles)
        {
            ClearObstacle(obstacle);
        }
        previousObstacles.Clear();
        // Apply current obstacles
        foreach (var obstacle in addedObstacles)
        {
            Vector2Int newPosition = new Vector2Int(obstacle.x, obstacle.y);
            ApplyObstacle(newPosition);
            previousObstacles.Add(newPosition);
        }
       

    }


    public void ClearAddedObstacles()
    {
        addedObstacles.Clear();
        GenerateRandomGrid(Xsize, Ysize, Probability);
        Debug.Log("All obstacles cleared.");
    }
    // New method to handle adding an obstacle at a specific position
    public void AddObstacle(Vector2Int position)
    {
        // Clear existing obstacle if present
        if (addedObstacles.Any(o => o.x == position.x && o.y == position.y))
        {
            ClearObstacle(position);
            addedObstacles.RemoveAll(o => o.x == position.x && o.y == position.y);
        }

        if (IsInBounds(position) && !addedObstacles.Any(o => o.x == position.x && o.y == position.y))
        {
            addedObstacles.Add(new Obstacle { x = position.x, y = position.y });
            ApplyObstacle(position);
            FindPath(new Vector2Int(startX, startY), new Vector2Int(goalX, goalY));
        }
        else
        {
            Debug.Log("Invalid position for an obstacle or already occupied.");
        }
    }

    // Call this method when the sliders are changed to update the obstacles
    public void UpdateObstaclePositions()
    {
        foreach (var obstacle in addedObstacles)
        {
            Vector2Int newPosition = new Vector2Int(obstacle.x, obstacle.y);
            if (!IsInBounds(newPosition) || (previousObstacles.Contains(newPosition) && previousObstacles.Any(o => o.x == newPosition.x && o.y == newPosition.y)))
                continue;

            // Clear the old position
            ClearObstacle(new Vector2Int(obstacle.x, obstacle.y));
            // Apply obstacle to the new position
            ApplyObstacle(newPosition);
        }
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
                //  pathfinding.EraseAddedObstacles(); // Call the method when button is clicked
                pathfinding.ClearAddedObstacles();
            }
           
        }
    }
#endif
}
