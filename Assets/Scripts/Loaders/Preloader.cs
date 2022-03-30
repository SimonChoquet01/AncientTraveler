using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class Preloader : MonoBehaviour
{
    private AsyncOperation _async;
    void Start()
    {
        _async = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        _async.allowSceneActivation = false;
    }
    void Update()
    {
        if (_async.progress >= 0.9f && SplashScreen.isFinished)
            _async.allowSceneActivation = true;
    }
}
