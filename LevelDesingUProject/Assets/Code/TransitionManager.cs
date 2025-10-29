using UnityEngine;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("UI Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.8f;

    private bool isTransitioning = false;

    // 🧠 Keep track of current and previous environments
    public GameObject currentEnvironment { get; private set; }
    public GameObject previousEnvironment { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (fadeCanvasGroup != null) {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets the starting environment on scene load.
    /// Call this once from Start() in a setup script.
    /// </summary>
    public void SetStartingEnvironment(GameObject startEnv) {
        currentEnvironment = startEnv;
        previousEnvironment = null;
    }

    public void RequestTransition(GameObject targetEnv, Transform spawnPoint, AudioClip sound = null) {
        if (isTransitioning) {
            Debug.LogWarning("Transition in progress.");
            return;
        }

        // Save where we’re coming from
        previousEnvironment = currentEnvironment;
        StartCoroutine(DoTransition(targetEnv, spawnPoint, sound));
    }

    // 🔁 Go back to previous environment
    public void GoBack(Transform spawnPoint, AudioClip sound = null) {
        if (previousEnvironment == null) {
            Debug.LogWarning("No previous environment stored.");
            return;
        }

        var target = previousEnvironment;
        previousEnvironment = currentEnvironment;
        StartCoroutine(DoTransition(target, spawnPoint, sound));
    }

    private IEnumerator DoTransition(GameObject targetEnv, Transform spawnPoint, AudioClip sound) {
        isTransitioning = true;

        if (sound)
            AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position);

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = true;

        yield return FadeTo(1f);

        // Swap environments
        if (currentEnvironment != null)
            currentEnvironment.SetActive(false);

        targetEnv.SetActive(true);
        currentEnvironment = targetEnv;

        // Move player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player && spawnPoint) {
            var rb = player.GetComponent<Rigidbody>();
            var cc = player.GetComponent<CharacterController>();
            if (rb) rb.isKinematic = true;
            if (cc) cc.enabled = false;

            player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            if (rb) rb.isKinematic = false;
            if (cc) cc.enabled = true;
        }

        yield return new WaitForSeconds(0.05f);
        yield return FadeTo(0f);

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);
        fadeCanvasGroup.blocksRaycasts = false;

        isTransitioning = false;
    }

    private IEnumerator FadeTo(float target) {
        float start = fadeCanvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = target;
    }
}
