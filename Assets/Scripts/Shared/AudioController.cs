using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    private AudioSource audioSource;
    private GlobalListener globalListener;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        globalListener = GlobalListener.Instance;

        if (globalListener != null)
            globalListener.CollisionSoundRequested += PlayCollisionSound;
    }

    private void OnDisable()
    {
        if (globalListener != null)
            globalListener.CollisionSoundRequested -= PlayCollisionSound;

        globalListener = null;
    }

    public void PlayCollisionSound()
    {
        audioSource.Play();
    }
}
