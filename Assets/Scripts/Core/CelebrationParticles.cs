using UnityEngine;
using System.Collections;

public class CelebrationParticles : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem[] particleSystems;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   burstSound;

    [Header("Settings")]
    [SerializeField] private int   burstCount    = 3;
    [SerializeField] private float burstInterval = 0.4f;

    private void Start()
    {
        // Force stop all particle systems on startup regardless of inspector settings
        foreach (var ps in particleSystems)
            if (ps != null) { ps.Stop(); ps.Clear(); }
    }

    public void Celebrate()
    {
        Debug.Log("[CelebrationParticles] Celebrate called!");
        StartCoroutine(FireworkSequence());
    }

    private IEnumerator FireworkSequence()
    {
        for (int i = 0; i < burstCount; i++)
        {
            foreach (var ps in particleSystems)
                if (ps != null) { ps.Stop(); ps.Clear(); ps.Play(); }

            if (audioSource != null && burstSound != null)
                audioSource.PlayOneShot(burstSound);

            yield return new WaitForSeconds(burstInterval);
        }
    }

}