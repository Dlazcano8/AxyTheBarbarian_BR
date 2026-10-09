using UnityEngine;

public class DrunkSkeletonArcher : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float shootRadius = 3f;

    private float timer;
    private GameObject currentArrow;

    void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    private void Shoot()
    {
        if (currentArrow != null)
            Destroy(currentArrow);

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(0f, shootRadius);

        Vector2 randomOffset = new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance
        );

        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

        currentArrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
    }
}