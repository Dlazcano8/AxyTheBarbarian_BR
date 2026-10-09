
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StateController))]
[RequireComponent(typeof(AudioController))]
public class PhysicsController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private StateController state;
    private AudioController audioController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<StateController>();
        audioController = GetComponent<AudioController>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 nextPosition =
            rb.position + state.Movement * speed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall") ||
            collision.collider.CompareTag("Enemy"))
        {
            audioController.PlayCollisionSound();
        }
    }
}
