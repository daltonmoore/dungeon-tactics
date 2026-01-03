using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Util
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        
        private ProgressBar progressBar;
        private Label progressLabel;
        private VisualElement _root;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            var uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;
            _root.visible = false;
            
            progressBar = _root.Q<ProgressBar>("progressBar");
            progressLabel = _root.Q<Label>("progressLabel");
        }
        
        public void LoadScene(string sceneToLoad, params object[] args)
        {
            _root.visible = true;
            StartCoroutine(LoadAsyncOperation(sceneToLoad));
        }

        private IEnumerator LoadAsyncOperation(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            progressBar.value = 0f;
            progressLabel.text = "Loading...0%";
            
            var startTimeStamp = Time.time;
            var minLoadingDuration = 3f;
            while (!operation.isDone)
            {
                // too fast to update the progress bar right now so we wait artificially for minLoadingDuration seconds
                // float progress = Mathf.Clamp01(operation.progress / .9f); 
                // progressBar.value = progress;
                // progressLabel.text = $"{(int)(progress * 50)}%";

                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            //get the time it took to finish the actual loading
            var currentDuration = Time.time - startTimeStamp;

            //check it against expected duration
            while (currentDuration < minLoadingDuration)
            {
                currentDuration = Time.time - startTimeStamp;
                float progress = Mathf.Clamp01(currentDuration / minLoadingDuration);
                progressBar.value = progress;
                progressLabel.text = $"Loading...{(int)(progress * 100)}%";
                yield return null;
            }
            
            _root.visible = false;
        }
    }
}