using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public partial class Pathfinding : MonoBehaviour
{
    //--- Constants ---
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14; //(Mathf.Sqrt(2) / 2) * 10 

    //--- Editor Parameters ---
    [Header("Configuration")]
    [SerializeField] [Tooltip("The bottom left corner of the level.")] private Vector2Int _boundsStart = new Vector2Int(-10, -10);
    [SerializeField] [Tooltip("The top right corner of the level.")] private Vector2Int _boundsEnd = new Vector2Int(10, 10);

    //--- Private Variables ---
    private Grid<PathNode> _grid;
    private List<PathNode> _openedList;
    private List<PathNode> _closedList;
    private Tilemap _map;

    //--- Custom Methods ---

    public List<Vector3> RequestAirPath(Vector3 start, Vector3 end)
    {
        //Convert world positions to navGrid positions
        Vector2Int startPointOnGrid = new Vector2Int((int)Mathf.Floor(start.x - _boundsStart.x), (int)Mathf.Floor(start.y - _boundsStart.y));
        Vector2Int endPointOnGrid = new Vector2Int((int)Mathf.Floor(end.x - _boundsStart.x), (int)Mathf.Floor(end.y - _boundsStart.y));
        //Clamp nav grid positions to valid array indexes
        startPointOnGrid = new Vector2Int(Mathf.Clamp(startPointOnGrid.x, 0, _grid.Width - 1), Mathf.Clamp(startPointOnGrid.y, 0, _grid.Height - 1));
        endPointOnGrid = new Vector2Int(Mathf.Clamp(endPointOnGrid.x, 0, _grid.Width - 1), Mathf.Clamp(endPointOnGrid.y, 0, _grid.Height - 1));

        //Calculate Path if possible
        List<PathNode> nodes = findAirPath(startPointOnGrid, endPointOnGrid);

        //Empty path
        if (nodes.Count == 0) return new List<Vector3>();

        //Return the position queue
        List<Vector3> returnL = new List<Vector3>();
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 nodePos = (((Vector3)nodes[i]) + Vector3.one * 0.5f) + new Vector3(_boundsStart.x, _boundsStart.y, 0);
            returnL.Add(nodePos);
        }

        //Debug Draw
        for (int i = 0; i < returnL.Count - 1; i++)
        {
            Debug.DrawLine(returnL[i] - Vector3.forward * 2, returnL[i + 1] - Vector3.forward * 2, Color.green);
        }

        return returnL;
    }
    private List<PathNode> findAirPath(Vector2Int start, Vector2Int end)
    {
        PathNode startNode = _grid.GetItem(start);
        PathNode endNode = _grid.GetItem(end);

        _openedList = new List<PathNode>() { startNode };
        _closedList = new List<PathNode>();

        resetNodes();

        startNode.gScore = 0;
        startNode.hCost = minimumCost(startNode, endNode);

        while (_openedList.Count > 0)
        {
            PathNode currentNode = _openedList.OrderBy(PathNode => PathNode.fScore).ToList()[0]; //Used Linq to find the lowest fScore
            if (currentNode == endNode)
            {
                return reconstructPath(currentNode);
            }

            _openedList.Remove(currentNode);
            _closedList.Add(currentNode);

            foreach(PathNode neighbor in currentNode.GetNeighbors())
            {
                if (_closedList.Contains(neighbor)) continue;

                if (!neighbor.traversable)
                {
                    _closedList.Add(neighbor);
                    continue;
                }

                int tentativeGScore = currentNode.gScore + minimumCost(currentNode, neighbor);
                if (tentativeGScore < neighbor.gScore)
                {
                    neighbor.previousNode = currentNode;
                    neighbor.gScore = tentativeGScore;
                    neighbor.hCost = minimumCost(neighbor, endNode);

                    if (!_openedList.Contains(neighbor)) _openedList.Add(neighbor);
                }
            }

        }
        return new List<PathNode>(); //Failed to find path
    }
    private List<PathNode> reconstructPath(PathNode end)
    {
        List<PathNode> newPath = new List<PathNode>();
        while (end.previousNode != null)
        {
            newPath.Add(end);
            end = end.previousNode;
        }
        newPath.Add(end);
        newPath.Reverse();
        return newPath;
    }
    private int minimumCost(PathNode from, PathNode to)
    {
        //The shortest possible path is not simply the shortest distance in this case
        //Usually a straight path followed by a diagonal section
        int length = Mathf.Abs(to.x - from.x);
        int height = Mathf.Abs(to.y - from.y);
        int straightPath = Mathf.Abs(length - height);
        return DIAGONAL_COST * Mathf.Min(length, height) + STRAIGHT_COST * straightPath;
    }
    private Vector2Int clampPosition(Vector2Int vector)
    {
        int x = Mathf.Clamp(vector.x, _boundsStart.x, _boundsEnd.x);
        int y = Mathf.Clamp(vector.y, _boundsStart.y, _boundsEnd.y);
        return new Vector2Int(x, y);
    }

    private void resetNodes()
    {
        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                PathNode node = _grid.GetItem(x, y);
                node.gScore = int.MaxValue;
                node.previousNode = null;
            }
        }
    }
    private void generateNodes()
    {
        //Generate nodes
        int width = _boundsEnd.x - _boundsStart.x;
        int height = _boundsEnd.y - _boundsStart.x;
        _grid = new Grid<PathNode>(width, height);
        for (int x = _boundsStart.x; x < _boundsEnd.x; x++)
        {
            for (int y = _boundsStart.x; y < _boundsEnd.y; y++)
            {
                PathNode node = new PathNode(_grid, x - _boundsStart.x, y - _boundsStart.y);
                _grid.SetItem(x - _boundsStart.x, y - _boundsStart.y, node);
                node.traversable = true;
                if (x >= _map.cellBounds.min.x && x < _map.cellBounds.max.x && y >= _map.cellBounds.min.y && y < _map.cellBounds.max.y)
                    node.traversable = (_map.GetTile(new Vector3Int(x, y, 0)) == null);
            }
        }

    }

    //--- Unity Methods ---
    private void OnDrawGizmosSelected()
    {
        Tilemap temp = GetComponent<Tilemap>();
        for (int x = temp.cellBounds.min.x; x < temp.cellBounds.max.x; x++)
        {
            for (int y = temp.cellBounds.min.y; y < temp.cellBounds.max.y; y++)
            {
                if(temp.GetTile(new Vector3Int(x, y, 0)) == null)
                {
                    Gizmos.color = new Color(0,0,1,0.25f);
                    Gizmos.DrawCube(temp.CellToWorld(new Vector3Int(x, y, 0))+(Vector3.one/2), Vector3.one);
                }
            }
        }
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Vector3 size = new Vector3(_boundsEnd.x - _boundsStart.x, _boundsEnd.y - _boundsStart.y, 0);
        Gizmos.DrawWireCube(new Vector3(_boundsStart.x, _boundsStart.y, 0) + size / 2, size);
    }
    void Start()
    {
        _map = GetComponent<Tilemap>();
        generateNodes();
    }

}

