using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    START,
    THINKING,
    PROGRESS,
    WON,
    LOST
}

public class BattleSystemManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> playerUnitPrefabs;
    [SerializeField] private List<GameObject> enemyUnitPrefabs;
    [SerializeField] private List<Transform> playerUnitSpawnPoints;
    [SerializeField] private List<Transform> enemyUnitSpawnPoints;

    [SerializeField] private ActionManager actionManager;
    [SerializeField] private TurnProgress turnProgress;

    private BattleState currentState;
    public BattleState CurrentState { get { return currentState; } }


    ActionManager actionEnterManager;

    void Start()
    {
        currentState = BattleState.START;
        StartCoroutine(BattleStart());
    }

    private void SpawnUnits(List<GameObject> unitPrefabs, List<Transform> spawnPoints)
    {
        for(int i = 0; i < unitPrefabs.Count; i++)
        {
            Vector3 spawnPosition = spawnPoints[i].position;
            GameObject unitObject = Instantiate(unitPrefabs[i], spawnPosition, Quaternion.identity);
            Unit unit = unitObject.GetComponent<Unit>();
            unit.unitName = unitPrefabs[i].name + i.ToString();
        }
    }

    IEnumerator BattleStart()
    {
        SpawnUnits(playerUnitPrefabs, playerUnitSpawnPoints);
        SpawnUnits(enemyUnitPrefabs, enemyUnitSpawnPoints);

        yield return new WaitForSeconds(1f);
        currentState = BattleState.THINKING;
        actionManager.StartPhase();
        Debug.Log("Battle Started! State changed to THINKING.");
        StopCoroutine(BattleStart());
    }

    public void OnTurnStartButton()
    {
        if(currentState != BattleState.THINKING)
        {
            Debug.LogWarning("Cannot start turn progress. Current state is not THINKING.");
            return;
        }
        actionManager.EndPhase();
        currentState = BattleState.PROGRESS;
        turnProgress.StartPhase();
        Debug.Log("Turn started! State changed to PROGRESS.");
    }
    public void OnTurnEnd()
    {
        StopCoroutine(turnProgress.ProgressTurn());
        currentState = BattleState.THINKING;
        actionManager.StartPhase();
        Debug.Log("Turn ended! State changed to THINKING.");
    }
}
