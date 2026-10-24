
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  private bool isFinished;
  private string resultMessage;
  private GlobalListener globalListener;

  private void OnEnable()
  {
    globalListener = GlobalListener.Instance;

    if (globalListener != null)
    {
      globalListener.Victory += Win;
      globalListener.Defeat += Lose;
    }
  }

  private void OnDisable()
  {
    if (globalListener != null)
    {
      globalListener.Victory -= Win;
      globalListener.Defeat -= Lose;
    }

    globalListener = null;
  }

  private void Update()
  {
    if (isFinished && Input.GetKeyDown(KeyCode.R))
    {
      Time.timeScale = 1f;
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
  }

  public void Win()
  {
    EndGame("¡GANASTE!");
  }

  public void Lose()
  {
    EndGame("¡PERDISTE!");
  }

  private void EndGame(string message)
  {
    if (isFinished)
      return;

    isFinished = true;
    resultMessage = message;

    Time.timeScale = 0f;
  }

  private void OnGUI()
  {
    if (!isFinished)
      return;

    float width = 340f;
    float height = 100f;

    Rect area = new Rect(
        (Screen.width - width) / 2f,
        (Screen.height - height) / 2f,
        width,
        height
    );

    GUI.Box(area, resultMessage + "\n\nPresiona R para reiniciar");
  }

  private void OnDestroy()
  {
    if (isFinished)
      Time.timeScale = 1f;
  }
}
