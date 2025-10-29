using UnityEngine;
using System.Collections;

    [RequireComponent(typeof(Collider))]

public class Portal : MonoBehaviour {

    [Header("Environment References")]
    public GameObject environmentA;
    public GameObject environmentB;

    [Header("Spawn Points")]
    public Transform spawnPointA;
    public Transform spawnPointB;

    [Header("Transition Settings")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;
    public AudioClip portalSound;

    private bool isInEnvironmentA = true;
    private bool isTransitioning = false;

    private void Start() {
        // Start in environment A
        environmentA.SetActive(true);
        environmentB.SetActive(false);

        if (fadeCanvas) {
            fadeCanvas.alpha = 0f;
            fadeCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (isTransitioning || !other.CompareTag("Player")) return;
        StartCoroutine(DoTransition(other.transform));
    }

    private IEnumerator DoTransition(Transform player) {
        isTransitioning = true;

        if (portalSound)
            AudioSource.PlayClipAtPoint(portalSound, transform.position);

        fadeCanvas.gameObject.SetActive(true);
        yield return Fade(1f); // fade to black

        // Switch environments
        if (isInEnvironmentA) {
            environmentA.SetActive(false);
            environmentB.SetActive(true);

            if (spawnPointB)
                player.position = spawnPointB.position;
        } else {
            environmentB.SetActive(false);
            environmentA.SetActive(true);

            if (spawnPointA)
                player.position = spawnPointA.position;
        }

        isInEnvironmentA = !isInEnvironmentA;

        yield return new WaitForSeconds(0.1f);
        yield return Fade(0f); // fade back in

        fadeCanvas.gameObject.SetActive(false);
        isTransitioning = false;
    }

    private IEnumerator Fade(float target) {
        float start = fadeCanvas.alpha;
        float t = 0f;

        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = target;
    }
}

