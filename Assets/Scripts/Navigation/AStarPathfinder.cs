using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
  private readonly NavigationGraph graph;

  public AStarPathfinder(NavigationGraph graph)
  {
    this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
  }

  public List<GraphNode> FindPath(GraphNode start, GraphNode goal)
  {
    graph.ResetSearch();

    if (start == null || goal == null ||
        graph.GetNode(start.X, start.Y) != start || graph.GetNode(goal.X, goal.Y) != goal)
      return new List<GraphNode>();

    var open = new List<GraphNode> { start };
    var closed = new HashSet<GraphNode>();
    start.GValue = 0f;
    start.HValue = Estimate(start, goal);

    while (open.Count > 0)
    {
      GraphNode current = open[0];

      for (int i = 1; i < open.Count; i++)
      {
        GraphNode candidate = open[i];

        if (candidate.F() < current.F() ||
            (candidate.F() == current.F() && candidate.HValue < current.HValue))
          current = candidate;
      }

      open.Remove(current);

      if (current == goal)
        return ReconstructPath(goal);

      closed.Add(current);

      foreach (GraphNode neighbor in graph.GetNeighbors(current))
      {
        if (closed.Contains(neighbor))
          continue;

        float cost = current.GValue + 1f;

        if (cost >= neighbor.GValue)
          continue;

        neighbor.GValue = cost;
        neighbor.HValue = Estimate(neighbor, goal);
        neighbor.CameFrom = current;

        if (!open.Contains(neighbor))
          open.Add(neighbor);
      }
    }

    return new List<GraphNode>();
  }

  private float Estimate(GraphNode node, GraphNode goal)
  {
    return Vector2.Distance(new Vector2(node.X, node.Y), new Vector2(goal.X, goal.Y));
  }

  private List<GraphNode> ReconstructPath(GraphNode goal)
  {
    var path = new List<GraphNode>();

    for (GraphNode node = goal; node != null; node = node.CameFrom)
      path.Add(node);

    path.Reverse();
    return path;
  }
}
