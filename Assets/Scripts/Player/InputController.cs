
using UnityEngine;

[RequireComponent(typeof(StateController))]
public class InputController : MonoBehaviour
{
    private StateController state;

    private void Awake()
    {
        state = GetComponent<StateController>();
    }

    private void Update()
    {
        ProcessInput();
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

        Vector2 movement = new Vector2(x, y).normalized;

        state.SetMovement(movement);
    }
}
