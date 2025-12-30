using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grid;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        public Queue<AbstractBattleUnit> TurnOrder { get; set; } = new();

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

            var allUnits = new List<BattleUnitData>();
            allUnits.AddRange(args.Party);
            allUnits.AddRange(args.EnemyParty);
            // var turnOrder = allUnits.OrderByDescending(u => u.unitPrefab.battleUnitSO.initiative).ToList();
            // for (int index = 0; index < turnOrder.Count; index++)
            // {
            //     BattleUnitData battleUnitData = turnOrder[index];
            //     Debug.Log($"{index}: {battleUnitData.name} initiative {battleUnitData.unitPrefab.battleUnitSO.initiative}");
            //     
            // }
        }
    }
}
