using UnityEngine;

public class blackbox : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayRecording()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}