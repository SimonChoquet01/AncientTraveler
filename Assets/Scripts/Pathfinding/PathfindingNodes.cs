using System.Collections.Generic;
using UnityEngine;

public partial class Pathfinding
{
    /// <summary>
    /// This class is used as the node for an A* pathfinding algorithmn.
    /// </summary>
    public class PathNode
    {

        //--- Private Variables ---
        private int _x;
        private int _y;

        private Grid<PathNode> _grid;

        //--- Public Fields ---
        public bool traversable = true;

        public PathNode previousNode;

        public int gScore; //G - cost of the cheapest path from start to this node
        public int hCost;//H - estimated cost from this node to end
        public int fScore { get => gScore + hCost; }//F=G+H - estimated shortest path if passing through this node.
        public int x { get => _x; }
        public int y { get => _y; }


        //--- Constructor ---
        public PathNode(Grid<PathNode> grid, int x, int y)
        {
            _x = x;
            _y = y;
            _grid = grid;
        }

        //--- Custom Methods ---

        public List<PathNode> GetNeighbors()
        {
            List<PathNode> neighbors = new List<PathNode>();

            if (this._x - 1 >= 0)
            {
                //L
                neighbors.Add(_grid.GetItem(this._x - 1, this._y));
                //BL
                if (this._y - 1 >= 0) neighbors.Add(_grid.GetItem(this._x - 1, this._y - 1));
                //TL
                if (this._y + 1 < _grid.Height) neighbors.Add(_grid.GetItem(this._x - 1, this._y + 1));
            }
            if (this.x + 1 < _grid.Width)
            {
                //R
                neighbors.Add(_grid.GetItem(this._x + 1, this._y));
                //BR
                if (this._y - 1 >= 0) neighbors.Add(_grid.GetItem(this._x + 1, this._y - 1));
                //TR
                if (this._y + 1 < _grid.Height) neighbors.Add(_grid.GetItem(this._x + 1, this._y + 1));
            }
            //T
            if (this.y + 1 < _grid.Height) neighbors.Add(_grid.GetItem(this._x, this._y + 1));
            //B
            if (this.y - 1 >= 0) neighbors.Add(_grid.GetItem(this._x, this._y - 1));
            return neighbors;
        }
        
        //--- Allows Implicit Casting ---
        public static implicit operator Vector3(PathNode node)
        {
            return new Vector3(node._x, node._y, 0);
        }

    }

}
