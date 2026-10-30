using UnityEngine;

public class GameObjectFactory : MonoBehaviour
{
  [SerializeField] private GameObject wallPrefab;
  [SerializeField] private GameObject exitPrefab;
  [SerializeField] private GameObject blindGazerPrefab;
  [SerializeField] private GameObject drunkSkeletonPrefab;
  [SerializeField] private GameObject cowardRatPrefab;
  [SerializeField] private GameObject hungryZombiePrefab;

  public bool IsConfigured =>
      wallPrefab != null && exitPrefab != null &&
      blindGazerPrefab != null && drunkSkeletonPrefab != null;

  public bool IsRatConfigured =>
      cowardRatPrefab != null &&
      cowardRatPrefab.GetComponent<CowardRatController>() != null &&
      cowardRatPrefab.GetComponent<EnemyPhysics>() != null;

  public bool IsZombieConfigured =>
      hungryZombiePrefab != null &&
      hungryZombiePrefab.GetComponent<HungryZombieController>() != null &&
      hungryZombiePrefab.GetComponent<EnemyPhysics>() != null;

  public GameObject Create(
      char symbol, Vector3 position, Transform parent,
      Rigidbody2D player = null, NavigationGraph graph = null
  )
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
      case 'R':
        if (!IsRatConfigured || player == null)
        {
          Debug.LogError("Asigna CowardRat.prefab con sus componentes y la referencia a Axy.", this);
          return null;
        }

        prefab = cowardRatPrefab;
        break;
      case 'Z':
        if (!IsZombieConfigured || player == null || graph == null)
        {
          Debug.LogError("Asigna HungryZombie.prefab, la referencia a Axy y el grafo del nivel.", this);
          return null;
        }

        prefab = hungryZombiePrefab;
        break;
      default:
        return null;
    }

    if (prefab == null)
    {
      Debug.LogError($"No hay un prefab asignado para el símbolo '{symbol}'.", this);
      return null;
    }

    GameObject instance = Instantiate(prefab, position, prefab.transform.rotation, parent);

    if (symbol == 'R')
      instance.GetComponent<CowardRatController>().Initialize(player);
    else if (symbol == 'Z')
      instance.GetComponent<HungryZombieController>().Initialize(player, graph);

    return instance;
  }
}
