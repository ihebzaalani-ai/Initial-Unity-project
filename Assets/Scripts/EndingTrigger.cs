using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingTrigger : MonoBehaviour
{
    public string sceneName = "Ending";
    public float delay = 1f;
    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(LoadEnding());
    }

    IEnumerator LoadEnding()
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
