
using UnityEngine;

public class PlayerOutcomeController : MonoBehaviour
{
  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.collider.CompareTag("Enemy"))
    {
      GlobalListener.Instance?.NotifyDefeat();
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Exit"))
    {
      GlobalListener.Instance?.NotifyVictory();
    }
    else if (other.CompareTag("Arrow"))
    {
      GlobalListener.Instance?.NotifyDefeat();
    }
  }
}
