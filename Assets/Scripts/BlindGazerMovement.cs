
using UnityEngine;

public class BlindGazerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;

    private bool movingUp = true;
    private float topLimit = 3f;
    private float bottomLimit = -3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
        UpdateState();
    }

    private void Move()
    {
        Vector2 direction = movingUp ? Vector2.up : Vector2.down;

        Vector2 nextPosition =
            rb.position + direction * speed * Time.fixedDeltaTime;

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            bottomLimit,
            topLimit
        );

        rb.MovePosition(nextPosition);
    }

    private void UpdateState()
    {
        if (rb.position.y >= topLimit)
            movingUp = false;
        else if (rb.position.y <= bottomLimit)
            movingUp = true;
    }
}
