
using UnityEngine;

public class AxyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private AudioSource collisionAudio;
    private Vector2 movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collisionAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        ProcessInput();
    }

    private void FixedUpdate()
    {
        UpdateState();
    }

    private void ProcessInput()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            y = 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            y = -1f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            x = -1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            x = 1f;

        movement = new Vector2(x, y).normalized;
    }

    private void UpdateState()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall") ||
            collision.collider.CompareTag("Enemy"))
        {
            if (collisionAudio != null && collisionAudio.clip != null)
                collisionAudio.Play();
        }
    }
}
