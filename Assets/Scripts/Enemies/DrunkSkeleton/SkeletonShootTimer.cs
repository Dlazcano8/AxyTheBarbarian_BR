
using UnityEngine;

[RequireComponent(typeof(SkeletonShooting))]
public class SkeletonShootTimer : MonoBehaviour
{
  private const float MinShootInterval = 0.01f;

  [SerializeField, Min(MinShootInterval)] private float shootInterval = 1.5f;

  private float timer;
  private SkeletonShooting shooting;

  private void Awake()
  {
    shooting = GetComponent<SkeletonShooting>();
  }

  private void OnValidate()
  {
    shootInterval = Mathf.Max(MinShootInterval, shootInterval);
  }

  private void Update()
  {
    UpdateTimer();
  }

  private void UpdateTimer()
  {
    float interval = Mathf.Max(MinShootInterval, shootInterval);
    timer += Time.deltaTime;

    while (timer >= interval)
    {
      timer -= interval;
      shooting.Shoot();
    }
  }
}
