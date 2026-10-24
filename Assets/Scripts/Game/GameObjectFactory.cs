using UnityEngine;

public class GameObjectFactory : MonoBehaviour
{
  [SerializeField] private GameObject wallPrefab;
  [SerializeField] private GameObject exitPrefab;
  [SerializeField] private GameObject blindGazerPrefab;
  [SerializeField] private GameObject drunkSkeletonPrefab;

  public bool IsConfigured =>
      wallPrefab != null && exitPrefab != null &&
      blindGazerPrefab != null && drunkSkeletonPrefab != null;

  public GameObject Create(char symbol, Vector3 position, Transform parent)
  {
    GameObject prefab;

    switch (symbol)
    {
      case '#':
        prefab = wallPrefab;
        break;
      case 'E':
        prefab = exitPrefab;
        break;
      case 'G':
        prefab = blindGazerPrefab;
        break;
      case 'S':
        prefab = drunkSkeletonPrefab;
        break;
      default:
        return null;
    }

    if (prefab == null)
    {
      Debug.LogError($"No hay un prefab asignado para el símbolo '{symbol}'.", this);
      return null;
    }

    return Instantiate(prefab, position, prefab.transform.rotation, parent);
  }
}
