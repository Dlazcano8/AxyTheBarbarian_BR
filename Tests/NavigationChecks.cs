using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class NavigationChecks
{
  private static int checks;

  private static void Check(bool condition, string message)
  {
    checks++;
    if (!condition)
      throw new Exception(message);
  }

  private static int ReferenceDistance(NavigationGraph graph, GraphNode start, GraphNode goal)
  {
    var distances = new Dictionary<GraphNode, int> { [start] = 0 };
    var queue = new Queue<GraphNode>();
    queue.Enqueue(start);
    int[] dx = { 0, 0, -1, 1 };
    int[] dy = { -1, 1, 0, 0 };

    while (queue.Count > 0)
    {
      GraphNode current = queue.Dequeue();
      if (current == goal)
        return distances[current];

      for (int i = 0; i < 4; i++)
      {
        GraphNode next = graph.GetNode(current.X + dx[i], current.Y + dy[i]);
        if (next != null && !distances.ContainsKey(next))
        {
          distances[next] = distances[current] + 1;
          queue.Enqueue(next);
        }
      }
    }

    return -1;
  }

  private static void CheckPath(NavigationGraph graph, GraphNode start, GraphNode goal)
  {
    List<GraphNode> path = new AStarPathfinder(graph).FindPath(start, goal);
    int distance = ReferenceDistance(graph, start, goal);
    Check(path.Count == (distance < 0 ? 0 : distance + 1), "A* cost differs from reference BFS");
    if (path.Count == 0)
      return;

    Check(path[0] == start && path[path.Count - 1] == goal, "Wrong reconstruction order");
    Check(start.GValue == 0f && start.CameFrom == null, "Start not reset");
    Check(goal.GValue == distance, "Incorrect goal cost");
    Check(Math.Abs(start.HValue - Vector2.Distance(new Vector2(start.X, start.Y), new Vector2(goal.X, goal.Y))) < 0.0001f, "Heuristic is not Euclidean");

    for (int i = 0; i < path.Count; i++)
    {
      GraphNode node = path[i];
      Check(graph.GetNode(node.X, node.Y) == node, "Path enters blocked or foreign node");
      Check(node.F() == node.GValue + node.HValue, "F must equal G + H");
      if (i > 0)
      {
        GraphNode previous = path[i - 1];
        Check(Math.Abs(node.X - previous.X) + Math.Abs(node.Y - previous.Y) == 1, "Path uses a diagonal or skips a cell");
        Check(node.CameFrom == previous, "Broken CameFrom chain");
      }
    }
  }

  public static void Main(string[] args)
  {
    var straight = new NavigationGraph(new[] { "Z...P" });
    CheckPath(straight, straight.GetNode(0, 0), straight.GetNode(4, 0));
    var obstacle = new NavigationGraph(new[] { "Z#..", "...#", "#..P" });
    CheckPath(obstacle, obstacle.GetNode(0, 0), obstacle.GetNode(3, 2));
    var skeleton = new NavigationGraph(new[] { "Z.S.P", "....." });
    Check(skeleton.GetNode(2, 0) == null, "Skeleton must block navigation");
    CheckPath(skeleton, skeleton.GetNode(0, 0), skeleton.GetNode(4, 0));
    var disconnected = new NavigationGraph(new[] { "Z#P" });
    CheckPath(disconnected, disconnected.GetNode(0, 0), disconnected.GetNode(2, 0));
    var diagonal = new NavigationGraph(new[] { "Z#", "#P" });
    CheckPath(diagonal, diagonal.GetNode(0, 0), diagonal.GetNode(1, 1));
    CheckPath(straight, straight.GetNode(2, 0), straight.GetNode(2, 0));
    var search = new AStarPathfinder(straight);
    Check(search.FindPath(null, straight.GetNode(0, 0)).Count == 0, "Null start not handled");
    Check(search.FindPath(straight.GetNode(0, 0), null).Count == 0, "Null goal not handled");
    Check(search.FindPath(new GraphNode(0, 0), straight.GetNode(4, 0)).Count == 0, "Foreign node accepted");
    Check(straight.GetNeighbors(null).Count() == 0 && straight.GetNeighbors(new GraphNode(0, 0)).Count() == 0, "Foreign neighbors accepted");
    for (int x = 0; x < straight.Width; x++)
    {
      GraphNode node = straight.GetNode(x, 0);
      Check(float.IsPositiveInfinity(node.GValue) && node.HValue == 0f && node.CameFrom == null, "Search leaked previous state");
    }
    Console.WriteLine("PASS: straight path, obstacles, Skeleton, unreachable goal, four neighbors, reconstruction, same/null/foreign nodes and search-state reset.");

    string[] rows = File.ReadAllLines(args[0]);
    var graph = new NavigationGraph(rows);
    Check(graph.Width == 43 && graph.Height == 13, "Wrong dungeon dimensions");
    var nodes = new List<GraphNode>();
    GraphNode player = null, zombie = null;
    for (int y = 0; y < graph.Height; y++)
    for (int x = 0; x < graph.Width; x++)
    {
      GraphNode node = graph.GetNode(x, y);
      bool blocked = rows[y][x] == '#' || rows[y][x] == 'S';
      Check((node == null) == blocked, "Graph disagrees with TXT");
      Check(graph.WorldToCell(graph.CellToWorld(x, y)) == new Vector2Int(x, y), "Cell/world round-trip failed");
      if (node == null)
        continue;
      nodes.Add(node);
      if (rows[y][x] == 'P') player = node;
      if (rows[y][x] == 'Z') zombie = node;
      var neighbors = graph.GetNeighbors(node).ToList();
      Check(neighbors.Count <= 4 && neighbors.Distinct().Count() == neighbors.Count, "Duplicate/excess neighbors");
      int expected = 0;
      foreach (var offset in new[] { new Vector2Int(0,-1), new Vector2Int(0,1), new Vector2Int(-1,0), new Vector2Int(1,0) })
        if (graph.GetNode(x + offset.x, y + offset.y) != null) expected++;
      Check(neighbors.Count == expected, "Missing cardinal neighbor");
      Check(neighbors.All(n => Math.Abs(n.X - x) + Math.Abs(n.Y - y) == 1), "Non-cardinal neighbor");
    }
    Check(nodes.Count == 386, "Incorrect walkable-node count");
    Check(graph.GetNode(-1, 0) == null && graph.GetNode(43, 0) == null && graph.GetNode(0, -1) == null && graph.GetNode(0, 13) == null, "Out-of-bounds node accepted");
    Check(graph.GetNodeAtWorld(new Vector2(100f, 100f)) == null, "World position clamped into map");
    CheckPath(graph, zombie, player);
    var route = new AStarPathfinder(graph).FindPath(zombie, player);
    Check(route.Any(n => n.X == 13) && route.Any(n => n.X == 28), "Dungeon route does not cross both corridors");
    var random = new System.Random(41);
    for (int i = 0; i < 100; i++)
      CheckPath(graph, nodes[random.Next(nodes.Count)], nodes[random.Next(nodes.Count)]);
    var saved = route.Select(n => new Vector2Int(n.X, n.Y)).ToArray();
    CheckPath(graph, player, zombie);
    Check(route.Select(n => new Vector2Int(n.X, n.Y)).SequenceEqual(saved), "A later search corrupts an existing route");
    var even = new NavigationGraph(new[] { "..", ".." });
    for (int y = 0; y < 2; y++)
    for (int x = 0; x < 2; x++)
      Check(even.WorldToCell(even.CellToWorld(x, y)) == new Vector2Int(x, y), "Even-sized conversion failed");
    Check(even.CellToWorld(0, 0) == new Vector3(-0.5f, 0.5f, 0f), "Even map not centered");
    Console.WriteLine("PASS: real dungeon topology, 386 nodes, both corridors, conversions, 100 optimal-route comparisons and independent successive searches.");
    Console.WriteLine("Assertions: " + checks + ". Actual navigation classes and Unity math assemblies; Unity Editor/Play Mode not executed.");
  }
}
