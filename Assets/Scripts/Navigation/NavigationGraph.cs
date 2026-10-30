using System;
using System.Collections.Generic;
using UnityEngine;

public class NavigationGraph
{
  private readonly GraphNode[,] nodes;

  public int Width { get; }
  public int Height { get; }

  public NavigationGraph(string[] rows)
  {
    if (rows == null || rows.Length == 0 || string.IsNullOrEmpty(rows[0]))
      throw new ArgumentException("El mapa no puede estar vacio.", nameof(rows));

    Width = rows[0].Length;
    Height = rows.Length;
    nodes = new GraphNode[Width, Height];

    for (int y = 0; y < Height; y++)
    {
      if (rows[y] == null || rows[y].Length != Width)
        throw new ArgumentException("Todas las filas deben tener el mismo ancho.", nameof(rows));

      for (int x = 0; x < Width; x++)
      {
        if (rows[y][x] != '#' && rows[y][x] != 'S')
          nodes[x, y] = new GraphNode(x, y);
      }
    }
  }

  public GraphNode GetNode(int x, int y)
  {
    if (x < 0 || y < 0 || x >= Width || y >= Height)
      return null;

    return nodes[x, y];
  }

  public IEnumerable<GraphNode> GetNeighbors(GraphNode node)
  {
    if (node == null || GetNode(node.X, node.Y) != node)
      yield break;

    GraphNode neighbor = GetNode(node.X, node.Y - 1);
    if (neighbor != null)
      yield return neighbor;

    neighbor = GetNode(node.X, node.Y + 1);
    if (neighbor != null)
      yield return neighbor;

    neighbor = GetNode(node.X - 1, node.Y);
    if (neighbor != null)
      yield return neighbor;

    neighbor = GetNode(node.X + 1, node.Y);
    if (neighbor != null)
      yield return neighbor;
  }

  public Vector3 CellToWorld(int x, int y)
  {
    return new Vector3(x - (Width - 1) * 0.5f, (Height - 1) * 0.5f - y, 0f);
  }

  public Vector2Int WorldToCell(Vector2 position)
  {
    return new Vector2Int(
        Mathf.FloorToInt(position.x + Width * 0.5f),
        Mathf.FloorToInt(Height * 0.5f - position.y)
    );
  }

  public GraphNode GetNodeAtWorld(Vector2 position)
  {
    Vector2Int cell = WorldToCell(position);
    return GetNode(cell.x, cell.y);
  }

  public void ResetSearch()
  {
    foreach (GraphNode node in nodes)
      node?.ResetSearch();
  }
}
