using System.Collections;
using Game_HoldGrounds.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace General.Utilities
{
    /// <summary>
    /// Handles loading scene in the game.
    /// It should only has one of this in the scene.
    /// At the moment this script is instantiated, it will start to fade and will async load a new scene
    /// if there is a name.
    /// </summary>
    public class LoadingSceneHandler : MonoBehaviour
    {
        public static LoadingSceneHandler Instance { get; private set; }
        
        /// <summary>
        /// Scene name to load.
        /// </summary>
        [SerializeField] private string sceneToLoadName;
        
        [Tooltip("Fade speed of the screen.")]
        [SerializeField] [Range(0.1f, 2.0f)]private float fadeScreenSpeed = 0.6f;
        private float fadeTimer;

        [Tooltip("BG image for fading in and out")]
        [SerializeField] private Image uiBgProgress;
        [Tooltip("Object that contains the loading bar, it will not be faded, so it needs to setup as game object.")]
        [SerializeField] private GameObject uiLoadingDataObj;
        [Tooltip("Progress bar Image to create the progress effect.")]
        [SerializeField] private RectTransform uiProgressBar;
        [Tooltip("GameObject to show when the level finishes loading. Usually a text.")]
        [SerializeField] private GameObject uiPressButton;
        [Tooltip("Text for loading.")]
        [SerializeField] private GameObject loadingText;
        
        //Cached
        private Color originalColorBg;
        private Color colorToFade;
        private AsyncOperation asyncLoad;
        private bool sceneFinishedLoading;
        private bool unloadingHandler;

        // =============================================================================================================
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); //but it will destroy manually later
            //Reset everything for the fade effect
            uiLoadingDataObj.SetActive(false);
            originalColorBg = uiBgProgress.color;
            colorToFade = originalColorBg;
            colorToFade.a = 0; //needs to start transparent
            uiBgProgress.color = colorToFade;
            uiProgressBar.localScale = new Vector3(0, 1, 1);
        }
        // =============================================================================================================
        private void Start()
        {
            StartCoroutine(FadeIn());
        }
        // =============================================================================================================
        private void Update()
        {
            if (!sceneFinishedLoading || unloadingHandler)
                return;
            
            if (Input.anyKeyDown)
            {
                asyncLoad.allowSceneActivation = true;
                unloadingHandler = true;
                StartCoroutine(FadeOut());
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Set the scene to be loaded.
        /// </summary>
        /// <param name="sceneName"></param>
        public void SetSceneName(string sceneName)
        {
            sceneToLoadName = sceneName;
        }
        // =============================================================================================================
        private IEnumerator FadeOut()
        {
            uiLoadingDataObj.SetActive(false);
            uiPressButton.SetActive(false);
            loadingText.SetActive(false);
            while (true)
            {
                colorToFade.a -= (fadeScreenSpeed/2) * Time.deltaTime;
                yield return new WaitForFixedUpdate();
                uiBgProgress.color = colorToFade;
                if (colorToFade.a <= 0)
                {
                    colorToFade.a = 0;
                    uiBgProgress.color = colorToFade;
                    // Debug.Log("Fade out finished.");
                    Destroy(gameObject);
                    yield break;
                }
            }
        }
        // =============================================================================================================
        private IEnumerator FadeIn()
        {
            uiPressButton.SetActive(false);
            loadingText.SetActive(true);
            while (true)
            {
                colorToFade.a += fadeScreenSpeed * Time.deltaTime;
                yield return new WaitForFixedUpdate();
                uiBgProgress.color = colorToFade;
                if (colorToFade.a >= 1)
                {
                    colorToFade.a = 1;
                    uiBgProgress.color = colorToFade;
                    uiLoadingDataObj.SetActive(true);
                    // Debug.Log("Fade finished. Loading scene...");
                    StartCoroutine(LoadAsyncScene());
                    yield break;
                }
            }
        }
        // =============================================================================================================
        /// <summary>
        /// Load a scene async to load with progress and background.
        /// </summary>
        private IEnumerator LoadAsyncScene()
        {
            while (string.IsNullOrEmpty(sceneToLoadName))
            {
                Debug.LogError("Scene loading is empty!");
            }
            yield return new WaitForSeconds(1);
            asyncLoad = SceneManager.LoadSceneAsync(sceneToLoadName); //AsyncOperation
            asyncLoad.allowSceneActivation = false;
            // Wait until the asynchronous scene fully loads
            var doneLoading = false;
            while (!doneLoading)
            {
                // Debug.Log(asyncLoad.progress);
                uiProgressBar.localScale = new Vector3(asyncLoad.progress, 1, 1);
                if (asyncLoad.progress >= 0.9f)
                {
                    doneLoading = true;
                    sceneFinishedLoading = true;
                    uiPressButton.SetActive(true);
                }
                yield return new WaitForFixedUpdate();
            }

            sceneFinishedLoading = true;
            uiProgressBar.localScale = new Vector3(1, 1, 1);
            // Debug.Log("Scene loaded. Press any button to continue.");
            yield return new WaitForSeconds(1);
        }
        // =============================================================================================================
    }
}