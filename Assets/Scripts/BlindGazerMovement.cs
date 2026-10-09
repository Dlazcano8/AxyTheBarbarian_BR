
using UnityEngine;

public class BlindGazerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;

    private bool movingUp = true;
    private float topLimit = 3f;
    private float bottomLimit = -3f;

    void Update()
    {
        Move();
        UpdateState();
    }

    private void Move()
    {
        Vector2 direction = movingUp ? Vector2.up : Vector2.down;

        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void UpdateState()
    {
        if (transform.position.y >= topLimit)
            movingUp = false;
        else if (transform.position.y <= bottomLimit)
            movingUp = true;
    }
}
