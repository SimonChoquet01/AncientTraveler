using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that represents a 2D array of type T.
/// </summary>
/// <typeparam name="T">The data type this grid will collect.</typeparam>
public class Grid<T>
{

    //--- Private Variables ---
    private int _width;
    private int _height;
    private T[,] _array;

    //--- Public Fields ---
    public int Width { get => _width; }
    public int Height { get => _height; }

    //--- Constructor ---
    public Grid(int width, int height, T[,] array = null)
    {
        this._array = new T[width, height];

        this._width = width;
        this._height = height;

        if (array == null || array.Length != width * height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this._array[x, y] = default(T);
                }
            }
        }
        else
            this._array = array;
    }

    //--- Custom Methods ---
    public T GetItem(int x, int y)
    {
        return _array[x, y];
    }
    public T GetItem(Vector2Int position)
    {
        return _array[position.x, position.y];
    }
    public void SetItem(int x, int y, T item)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
            _array[x, y] = item;
    }

    public Vector2Int GetPosition(T item)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_array[x, y].Equals(item)) return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

}