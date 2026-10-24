using UnityEngine;

[RequireComponent(typeof(GameObjectFactory))]
public class DungeonLevelLoader : MonoBehaviour
{
  [SerializeField] private TextAsset levelFile;
  [SerializeField] private Rigidbody2D player;

  private GameObject generatedLevel;

  private void Start()
  {
    LoadLevel();
  }

  public void LoadLevel()
  {
    if (generatedLevel != null)
      return;

    GameObjectFactory factory = GetComponent<GameObjectFactory>();

    if (levelFile == null || player == null || factory == null || !factory.IsConfigured)
    {
      Debug.LogError("Asigna el archivo del nivel, el Rigidbody2D de Axy y los cuatro prefabs de la Factory.", this);
      return;
    }

    if (!TryReadLevel(out string[] rows, out Vector2Int playerCell))
      return;

    int width = rows[0].Length;
    int height = rows.Length;

    generatedLevel = new GameObject("GeneratedDungeon");

    player.position = GetCellPosition(playerCell.x, playerCell.y, width, height);

    for (int row = 0; row < height; row++)
    {
      for (int column = 0; column < width; column++)
      {
        factory.Create(
            rows[row][column],
            GetCellPosition(column, row, width, height),
            generatedLevel.transform
        );
      }
    }
  }

  private bool TryReadLevel(out string[] rows, out Vector2Int playerCell)
  {
    rows = levelFile.text.TrimStart('\uFEFF')
        .Replace("\r\n", "\n").Replace('\r', '\n')
        .TrimEnd('\n').Split('\n');
    playerCell = Vector2Int.zero;

    int width = rows[0].Length;
    int playerCount = 0;
    int exitCount = 0;

    if (width == 0)
    {
      Debug.LogError("El archivo del nivel está vacío.", this);
      return false;
    }

    for (int row = 0; row < rows.Length; row++)
    {
      if (rows[row].Length != width)
      {
        Debug.LogError($"La fila {row + 1} no tiene el mismo ancho que el resto del mapa.", this);
        return false;
      }

      for (int column = 0; column < width; column++)
      {
        char symbol = rows[row][column];

        switch (symbol)
        {
          case 'P':
            playerCount++;
            playerCell = new Vector2Int(column, row);
            break;
          case 'E':
            exitCount++;
            break;
          case '#':
          case '.':
          case 'G':
          case 'S':
            break;
          default:
            Debug.LogError($"Símbolo '{symbol}' no válido en fila {row + 1}, columna {column + 1}.", this);
            return false;
        }
      }
    }

    if (playerCount != 1 || exitCount != 1)
    {
      Debug.LogError("El mapa debe contener exactamente un inicio P y una salida E.", this);
      return false;
    }

    return true;
  }

  private Vector3 GetCellPosition(int column, int row, int width, int height)
  {
    return new Vector3(
        column - (width - 1) * 0.5f,
        (height - 1) * 0.5f - row,
        0f
    );
  }
}
