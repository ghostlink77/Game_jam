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
    public static BattleSystemManager instance;
    public static BattleSystemManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [SerializeField] private List<GameObject> playerUnitPrefabs;
    [SerializeField] private List<GameObject> enemyUnitPrefabs;
    [SerializeField] private List<Transform> playerUnitSpawnPoints;
    [SerializeField] private List<Transform> enemyUnitSpawnPoints;

    [SerializeField] private ActionManager actionManager;
    [SerializeField] private TurnProgress turnProgress;
    [SerializeField] private UIManager UIManager;

    private List<Unit> playerUnits;
    private List<Unit> enemyUnits;

    private BattleState currentState;
    public BattleState CurrentState { get { return currentState; } }


    ActionManager actionEnterManager;

    void Start()
    {
        currentState = BattleState.START;
        StartCoroutine(BattleStart());
    }

    private List<Unit> SpawnUnits(List<GameObject> unitPrefabs, List<Transform> spawnPoints)
    {
        List<Unit> spawnedUnits = new List<Unit>();
        for (int i = 0; i < unitPrefabs.Count; i++)
        {
            Vector3 spawnPosition = spawnPoints[i].position;
            GameObject unitObject = Instantiate(unitPrefabs[i], spawnPosition, Quaternion.identity);
            Unit unit = unitObject.GetComponent<Unit>();
            unit.unitName = unitPrefabs[i].name + i.ToString();

            spawnedUnits.Add(unit);
        }

        return spawnedUnits;
    }

    IEnumerator BattleStart()
    {
        playerUnits = SpawnUnits(playerUnitPrefabs, playerUnitSpawnPoints);
        enemyUnits = SpawnUnits(enemyUnitPrefabs, enemyUnitSpawnPoints);

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
        turnProgress.EndPhase();

        if(JudgeWin())
        {
            currentState = BattleState.WON;
            Debug.Log("All enemies defeated! You won the battle!");
            UIManager.Win();
            return;
        }
        if(JudgeLose())
        {
            currentState = BattleState.LOST;
            Debug.Log("All player units defeated! You lost the battle!");
            UIManager.Lose();
            return;
        }

        currentState = BattleState.THINKING;
        actionManager.StartPhase();
        Debug.Log("Turn ended! State changed to THINKING.");
    }

    private bool JudgeWin()
    {
        foreach(Unit enemy in enemyUnits)
        {
            if(enemy.IsAlive)
                return false;
        }
        return true;
    }
    private bool JudgeLose()
    {
        foreach(Unit player in playerUnits)
        {
            if(player.IsAlive)
                return false;
        }
        return true;
    }
}
