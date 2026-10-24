
using UnityEngine;

[RequireComponent(typeof(SkeletonShooting))]
public class SkeletonShootTimer : MonoBehaviour
{
  [SerializeField] private float shootInterval = 1.5f;

  private float timer;
  private SkeletonShooting shooting;

  private void Awake()
  {
    shooting = GetComponent<SkeletonShooting>();
  }

  private void Update()
  {
    UpdateTimer();
  }

  private void UpdateTimer()
  {
    timer += Time.deltaTime;

    if (timer >= shootInterval)
    {
      timer -= shootInterval;
      shooting.Shoot();
    }
  }
}
