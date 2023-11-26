using System.Collections.Generic;
using UnityEngine;

public class Ship
{
    public string Name { get; private set; }
    public List<Vector2Int> Positions { get; private set; }
    public int Hits { get; private set; }
    public int Size { get; private set; }

    public Ship(string name, int size)
    {
        Name = name;
        Positions = new List<Vector2Int>(size);
        Hits = 0;
        Size = size;
    }

    public void AddPosition(int row, int col)
    {
        Positions.Add(new Vector2Int(row, col));
    }

    public void RegisterHit()
    {
        Hits++;
    }

    public bool IsSunk()
    {
        return Hits >= Positions.Count;
    }
}
