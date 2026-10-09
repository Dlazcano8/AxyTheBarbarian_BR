using UnityEngine;

public class StateController : MonoBehaviour
{
    public Vector2 Movement { get; private set; }

    public void SetMovement(Vector2 movement)
    {
        Movement = movement;
    }
}
