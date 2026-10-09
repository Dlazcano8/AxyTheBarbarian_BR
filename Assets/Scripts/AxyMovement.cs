using UnityEngine;

public class AxyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Vector2 movement;

    void Update()
    {
        ProcessInput();
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
        transform.Translate(movement * speed * Time.deltaTime);
    }
}