using System;
using System.Collections;
using Battle;
using Events;
using Grid;
using TacticsCore.Data;
using TacticsCore.EventBus;
using Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Util
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        
        [SerializeField] private float artificialLoadingDuration = 3f;
        
        private ProgressBar progressBar;
        private Label progressLabel;
        private VisualElement _root;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Bus<EngageInBattleEvent>.OnEvent[Owner.Player1] += OnStartBattle;
        }

        private void Start()
        {
            var uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;
            _root.visible = false;
            
            progressBar = _root.Q<ProgressBar>("progressBar");
            progressLabel = _root.Q<Label>("progressLabel");
        }

        private void OnStartBattle(EngageInBattleEvent args)
        {
            LoadScene(DTConstants.SceneNames.Battle, () =>
            {
                BattleManager.Instance.StartBattle(args);
            });
        }

        public void LoadScene(string sceneToLoad, Action onFinishedLoading)
        {
            _root.visible = true;
            StartCoroutine(LoadAsyncOperation(sceneToLoad, onFinishedLoading));
        }

        private IEnumerator LoadAsyncOperation(string sceneName, Action onFinishedLoading)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            progressBar.value = 0f;
            progressLabel.text = "Loading...0%";
            
            var startTimeStamp = Time.time;
            var minLoadingDuration = artificialLoadingDuration;
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
            onFinishedLoading();
        }
    }
}