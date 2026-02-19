using System;
using System.Collections.Generic;
using Data;
using Events;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Events;
using TacticsCore.Save;
using TacticsCore.Units;
using UnityEngine;

namespace Map
{
    public class OverworldManager : MonoBehaviour
    {
        [SerializeField] private GameObject leaderUnitPrefab;
        
        private void Awake()
        {
            Bus<InitialSceneLoadedEvent>.OnEvent[Owner.Player1] += OnInitialSceneLoaded;
            Bus<SceneLoadedEvent>.OnEvent[Owner.Player1] += OnSceneLoaded;
            Bus<ExitBattleEvent>.OnEvent[Owner.Player1] += OnExitBattle;
        }

        private void OnDestroy()
        {
            Bus<InitialSceneLoadedEvent>.OnEvent[Owner.Player1] -= OnInitialSceneLoaded;
            Bus<SceneLoadedEvent>.OnEvent[Owner.Player1] -= OnSceneLoaded;
            Bus<ExitBattleEvent>.OnEvent[Owner.Player1] -= OnExitBattle;
        }

        private void OnInitialSceneLoaded(InitialSceneLoadedEvent _)
        {
            // I will probably just immediately save the leaders when we load into the map instead of maintaining a list.
            // LeaderUnit[] initialLeaders = FindObjectsByType<LeaderUnit>(FindObjectsSortMode.None);
            // _leaders.AddRange(initialLeaders);
        }

        private void OnSceneLoaded(SceneLoadedEvent _)
        {
            LeaderUnit[] initialLeaders = FindObjectsByType<LeaderUnit>(FindObjectsSortMode.None);
            foreach (var leader in initialLeaders)
            {
                Destroy(leader.gameObject);
            }
            
        }

        private void OnExitBattle(ExitBattleEvent _)
        {
            TDSaveData saveData = SaveManager.Load<TDSaveData>();

            foreach (LeaderSaveData leaderSaveData in saveData.leaders)
            {
                GameObject leaderInstance = Instantiate(leaderUnitPrefab);
                var leaderUnit = leaderInstance.GetComponent<LeaderUnit>();
                
                leaderUnit.Owner = leaderSaveData.owner;
                
                leaderInstance.transform.position = leaderSaveData.position;
                leaderInstance.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Character sprites/"+leaderSaveData.spriteName);
                leaderInstance.GetComponent<LeaderUnit>().PartyList = leaderSaveData.party;
            }
        }
    }
}