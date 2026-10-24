
using UnityEngine;

public class BlindGazerState : MonoBehaviour
{
  [SerializeField] private float topLimit = 3f;
  [SerializeField] private float bottomLimit = -3f;

  private bool movingUp = true;

  public float TopLimit => topLimit;
  public float BottomLimit => bottomLimit;

  public Vector2 Direction => movingUp ? Vector2.up : Vector2.down;

  public void UpdateState(float positionY)
  {
    if (positionY >= topLimit)
      movingUp = false;
    else if (positionY <= bottomLimit)
      movingUp = true;
  }
}
