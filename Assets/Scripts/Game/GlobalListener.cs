using System;
using UnityEngine;

public class GlobalListener : MonoBehaviour
{
  private static GlobalListener instance;
  private bool isFinished;

  public static GlobalListener Instance
  {
    get
    {
      if (instance == null)
        instance = FindAnyObjectByType<GlobalListener>();

      return instance;
    }
  }

  public event Action CollisionSoundRequested;
  public event Action Victory;
  public event Action Defeat;

  private void Awake()
  {
    if (instance != null && instance != this)
    {
      Destroy(gameObject);
      return;
    }

    instance = this;
  }

  public void NotifyCollisionSound()
  {
    CollisionSoundRequested?.Invoke();
  }

  public void NotifyVictory()
  {
    if (isFinished)
      return;

    isFinished = true;
    Debug.Log("¡GANASTE!");
    Victory?.Invoke();
  }

  public void NotifyDefeat()
  {
    if (isFinished)
      return;

    isFinished = true;
    Debug.Log("¡PERDISTE!");
    Defeat?.Invoke();
  }

  private void OnDestroy()
  {
    if (instance != this)
      return;

    instance = null;
    CollisionSoundRequested = null;
    Victory = null;
    Defeat = null;
  }
}
