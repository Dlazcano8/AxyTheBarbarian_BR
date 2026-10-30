using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(EnemyPhysics))]
public class HungryZombieController : MonoBehaviour
{
  [SerializeField, Min(0f)] private float speed = 2f;
  [SerializeField, Min(0.01f)] private float repathInterval = 0.35f;
  [SerializeField, Min(0.1f)] private float stuckTimeout = 1f;
  [SerializeField, Range(0.001f, 0.1f)] private float nodeArrivalDistance = 0.02f;
  [SerializeField] private Transform visual;

  private EnemyPhysics physics;
  private Rigidbody2D player;
  private NavigationGraph graph;
  private AStarPathfinder pathfinder;
  private List<GraphNode> path = new List<GraphNode>();
  private int pathIndex;
  private bool hasSearched;
  private float searchTimer;
  private float blockedTime;
  private Vector2 lastPosition;
  private Vector2Int lastStartCell;
  private Vector2Int lastTargetCell;

  public Vector2 Velocity { get; private set; }
  public float Orientation { get; private set; }
  public int PathSearchCount { get; private set; }

  private void Awake()
  {
    physics = GetComponent<EnemyPhysics>();
  }

  public void Initialize(Rigidbody2D target, NavigationGraph navigationGraph)
  {
    player = target;
    graph = navigationGraph;
    pathfinder = graph == null ? null : new AStarPathfinder(graph);
    path.Clear();
    pathIndex = 0;
    hasSearched = false;
    searchTimer = 0f;
    blockedTime = 0f;
    PathSearchCount = 0;
    lastPosition = physics.Position;
    Stop();
  }

  private void FixedUpdate()
  {
    if (player == null || graph == null || pathfinder == null)
    {
      Stop();
      return;
    }

    searchTimer = Mathf.Max(0f, searchTimer - Time.fixedDeltaTime);
    Vector2 position = physics.Position;
    TrackProgress(position);
    Vector2Int startCell = graph.WorldToCell(position);
    Vector2Int targetCell = graph.WorldToCell(player.position);
    bool needsPath = !hasSearched || targetCell != lastTargetCell ||
        (path.Count == 0 && startCell != lastStartCell) || IsOffPath(startCell) ||
        !IsNextNodeValid() || blockedTime >= Mathf.Max(0.1f, stuckTimeout);

    if (needsPath && searchTimer <= 0f)
      RecalculatePath(startCell, targetCell);

    if (IsOffPath(startCell) || !IsNextNodeValid())
    {
      Stop();
      return;
    }

    FollowPath(startCell, targetCell);
  }

  private void RecalculatePath(Vector2Int startCell, Vector2Int targetCell)
  {
    path = pathfinder.FindPath(
        graph.GetNode(startCell.x, startCell.y),
        graph.GetNode(targetCell.x, targetCell.y)
    );
    pathIndex = 0;
    lastStartCell = startCell;
    lastTargetCell = targetCell;
    hasSearched = true;
    searchTimer = Mathf.Max(0.01f, repathInterval);
    blockedTime = 0f;
    PathSearchCount++;

    if (path.Count > 1)
    {
      Vector2 start = graph.CellToWorld(path[0].X, path[0].Y);
      Vector2 segment = (Vector2)graph.CellToWorld(path[1].X, path[1].Y) - start;
      Vector2 offset = physics.Position - start;
      float along = Vector2.Dot(offset, segment);
      float tolerance = Mathf.Clamp(nodeArrivalDistance, 0.001f, 0.1f);

      if (along >= 0f && along <= 1f &&
          (offset - segment * along).sqrMagnitude <= tolerance * tolerance)
        pathIndex = 1;
    }
  }

  private bool IsOffPath(Vector2Int cell)
  {
    if (path.Count == 0)
      return false;

    int index = Mathf.Min(pathIndex, path.Count - 1);
    GraphNode next = path[index];

    if (cell.x == next.X && cell.y == next.Y)
      return false;

    if (pathIndex > 0)
    {
      GraphNode previous = path[pathIndex - 1];
      return cell.x != previous.X || cell.y != previous.Y;
    }

    return true;
  }

  private bool IsNextNodeValid()
  {
    if (pathIndex >= path.Count)
      return true;

    GraphNode node = path[pathIndex];
    return graph.GetNode(node.X, node.Y) == node;
  }

  private void FollowPath(Vector2Int startCell, Vector2Int targetCell)
  {
    float tolerance = Mathf.Clamp(nodeArrivalDistance, 0.001f, 0.1f);

    while (pathIndex < path.Count)
    {
      GraphNode node = path[pathIndex];
      Vector2 target = graph.CellToWorld(node.X, node.Y);

      if ((target - physics.Position).sqrMagnitude > tolerance * tolerance)
        break;

      pathIndex++;
    }

    if (pathIndex < path.Count)
    {
      GraphNode node = path[pathIndex];
      MoveTowards(graph.CellToWorld(node.X, node.Y));
    }
    else if (path.Count > 0 && startCell == targetCell)
    {
      MoveTowards(player.position);
    }
    else
    {
      Stop();
    }
  }

  private void MoveTowards(Vector2 target)
  {
    Vector2 offset = target - physics.Position;
    float distance = offset.magnitude;
    float step = Mathf.Max(0.0001f, Time.fixedDeltaTime);
    Velocity = offset.normalized * Mathf.Min(Mathf.Max(0f, speed), distance / step);

    if (Velocity.sqrMagnitude > 0.0001f)
    {
      Orientation = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;

      if (visual != null)
        visual.rotation = Quaternion.Euler(0f, 0f, Orientation);
    }

    physics.SetVelocity(Velocity);
  }

  private void TrackProgress(Vector2 position)
  {
    float expected = Velocity.sqrMagnitude * Time.fixedDeltaTime * Time.fixedDeltaTime;

    if (expected > 0f && (position - lastPosition).sqrMagnitude < expected * 0.01f)
      blockedTime += Time.fixedDeltaTime;
    else
      blockedTime = 0f;

    lastPosition = position;
  }

  private void Stop()
  {
    Velocity = Vector2.zero;

    if (physics != null)
      physics.SetVelocity(Velocity);
  }

  private void OnDisable()
  {
    Stop();
  }
}
