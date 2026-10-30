using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPhysics : MonoBehaviour
{
  private Rigidbody2D rb;
  private Vector2 velocity;
  private ContactFilter2D contactFilter;

  public Vector2 Position => rb.position;

  private void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    rb.gravityScale = 0f;
    rb.freezeRotation = true;
    contactFilter = new ContactFilter2D();
    contactFilter.useTriggers = false;
  }

  public void SetVelocity(Vector2 value)
  {
    velocity = value;
  }

  public int Cast(Vector2 direction, float distance, RaycastHit2D[] results)
  {
    return rb.Cast(direction, contactFilter, results, distance);
  }

  private void FixedUpdate()
  {
    rb.linearVelocity = velocity;
  }

  private void OnDisable()
  {
    velocity = Vector2.zero;

    if (rb != null)
      rb.linearVelocity = Vector2.zero;
  }
}
