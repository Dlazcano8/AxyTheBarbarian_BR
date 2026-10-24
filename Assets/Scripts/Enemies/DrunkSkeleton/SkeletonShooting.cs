
using UnityEngine;

public class SkeletonShooting : MonoBehaviour
{
  [SerializeField] private GameObject arrowPrefab;
  [SerializeField, Min(0f)] private float shootRadius = 3f;

  private GameObject currentArrow;

  private void OnValidate()
  {
    shootRadius = Mathf.Max(0f, shootRadius);
  }

  public void Shoot()
  {
    if (arrowPrefab == null)
      return;

    if (currentArrow != null)
      Destroy(currentArrow);

    float angle = Random.Range(0f, Mathf.PI * 2f);
    float distance = Random.Range(0f, Mathf.Max(0f, shootRadius));

    Vector2 randomOffset = new Vector2(
        Mathf.Cos(angle) * distance,
        Mathf.Sin(angle) * distance
    );

    Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

    currentArrow = Instantiate(
        arrowPrefab,
        spawnPosition,
        Quaternion.identity
    );
  }
}
