
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BlindGazerState))]
public class BlindGazerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 2f;

    private Rigidbody2D rb;
    private BlindGazerState state;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<BlindGazerState>();
    }

    private void FixedUpdate()
    {
        state.UpdateState(rb.position.y);
        Move();
    }

    private void Move()
    {
        Vector2 nextPosition =
            rb.position + state.CalculateVelocity(speed) * Time.fixedDeltaTime;

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            state.BottomLimit,
            state.TopLimit
        );

        rb.MovePosition(nextPosition);
    }
}
