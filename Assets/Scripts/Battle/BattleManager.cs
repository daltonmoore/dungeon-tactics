using System;
using System.Collections;
using System.Collections.Generic;
using Grid;
using Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            { 
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void StartBattle(BattleStartArgs args)
        {
            StartCoroutine(LoadIntoBattleScene(args));
        }

        private IEnumerator LoadIntoBattleScene(BattleStartArgs args)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("BattleScene");

            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            
            BattleGrid grid = FindFirstObjectByType<BattleGrid>();
            grid.ClearGrid();
            grid.SetupGrid();
            grid.SetupParties(args);
        }
    }
}
