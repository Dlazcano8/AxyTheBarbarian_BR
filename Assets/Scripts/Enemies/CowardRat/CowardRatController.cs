using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(EnemyPhysics))]
public class CowardRatController : MonoBehaviour
{
  [SerializeField, Min(0f)] private float wanderSpeed = 2f;
  [SerializeField, Min(0f)] private float fleeSpeed = 3f;
  [SerializeField, Min(0f)] private float detectionDistance = 5f;
  [SerializeField, Min(0f)] private float wanderTurnSpeed = 90f;
  [SerializeField, Min(0.01f)] private float wanderChangeInterval = 0.5f;
  [SerializeField, Range(0f, 180f)] private float wanderAngleRange = 45f;
  [SerializeField, Min(0.01f)] private float wallLookAhead = 0.5f;
  [SerializeField, Min(0.01f)] private float wallAvoidanceDuration = 0.35f;
  [SerializeField] private Transform visual;

  private EnemyPhysics physics;
  private Rigidbody2D player;
  private float wanderTargetAngle;
  private float wanderTimer;
  private float avoidanceTimer;
  private Vector2 avoidanceDirection;
  private readonly RaycastHit2D[] hits = new RaycastHit2D[16];

  public bool IsFleeing { get; private set; }
  public float Orientation { get; private set; }
  public Vector2 Velocity { get; private set; }

  private void Awake()
  {
    physics = GetComponent<EnemyPhysics>();
    Orientation = Random.Range(0f, 360f);
    wanderTargetAngle = Orientation;
  }

  public void Initialize(Rigidbody2D target)
  {
    player = target;
  }

  private void FixedUpdate()
  {
    if (player == null)
    {
      Velocity = Vector2.zero;
      physics.SetVelocity(Velocity);
      return;
    }

    Vector2 away = physics.Position - player.position;
    float distance = Mathf.Max(0f, detectionDistance);
    bool wasFleeing = IsFleeing;
    IsFleeing = away.sqrMagnitude <= distance * distance;

    if (wasFleeing != IsFleeing)
    {
      avoidanceTimer = 0f;
      wanderTimer = 0f;
      wanderTargetAngle = Orientation;
    }

    Vector2 direction = IsFleeing && away.sqrMagnitude > 0.0001f
        ? away.normalized
        : GetWanderDirection();
    float speed = Mathf.Max(0f, IsFleeing ? fleeSpeed : wanderSpeed);
    Vector2 desiredDirection = direction;
    direction = AvoidWalls(direction, speed);

    if (!IsFleeing && Vector2.Dot(direction, desiredDirection) < 0.99f)
    {
      wanderTargetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
      wanderTimer = Mathf.Max(0.01f, wanderChangeInterval);
    }

    Velocity = direction * speed;

    if (Velocity.sqrMagnitude > 0.0001f)
    {
      Orientation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

      if (visual != null)
        visual.rotation = Quaternion.Euler(0f, 0f, Orientation);
    }

    physics.SetVelocity(Velocity);
  }

  private Vector2 GetWanderDirection()
  {
    wanderTimer -= Time.fixedDeltaTime;

    if (wanderTimer <= 0f)
    {
      float angleRange = Mathf.Clamp(wanderAngleRange, 0f, 180f);
      wanderTargetAngle = Orientation + Random.Range(-angleRange, angleRange);
      wanderTimer = Mathf.Max(0.01f, wanderChangeInterval);
    }

    float angle = Mathf.MoveTowardsAngle(
        Orientation,
        wanderTargetAngle,
        Mathf.Max(0f, wanderTurnSpeed) * Time.fixedDeltaTime
    ) * Mathf.Deg2Rad;

    return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
  }

  private Vector2 AvoidWalls(Vector2 direction, float speed)
  {
    float distance = Mathf.Max(wallLookAhead, speed * Time.fixedDeltaTime + 0.05f);

    if (avoidanceTimer > 0f)
    {
      avoidanceTimer -= Time.fixedDeltaTime;

      if (!TryGetWallNormal(avoidanceDirection, distance, out _))
        return avoidanceDirection;
    }

    if (!TryGetWallNormal(direction, distance, out Vector2 normal))
      return direction;

    Vector2 tangent = new Vector2(-normal.y, normal.x);

    if (Vector2.Dot(tangent, direction) < 0f)
      tangent = -tangent;

    Vector2 candidate = tangent;

    if (TryGetWallNormal(candidate, distance, out _))
    {
      candidate = -tangent;

      if (TryGetWallNormal(candidate, distance, out _))
      {
        candidate = normal;

        if (TryGetWallNormal(candidate, distance, out _))
        {
          candidate = -direction;

          if (TryGetWallNormal(candidate, distance, out _))
            return Vector2.zero;
        }
      }
    }

    avoidanceDirection = candidate.normalized;
    avoidanceTimer = Mathf.Max(0.01f, wallAvoidanceDuration);
    return avoidanceDirection;
  }

  private bool TryGetWallNormal(Vector2 direction, float distance, out Vector2 normal)
  {
    normal = Vector2.zero;
    float nearest = float.PositiveInfinity;
    int count = physics.Cast(direction, distance, hits);

    for (int i = 0; i < count; i++)
    {
      RaycastHit2D hit = hits[i];

      if (hit.collider != null && hit.collider.CompareTag("Wall") &&
          Vector2.Dot(direction, hit.normal) < -0.05f && hit.distance < nearest)
      {
        normal = hit.normal;
        nearest = hit.distance;
      }
    }

    return nearest < float.PositiveInfinity;
  }

  private void OnDisable()
  {
    Velocity = Vector2.zero;

    if (physics != null)
      physics.SetVelocity(Velocity);
  }
}
