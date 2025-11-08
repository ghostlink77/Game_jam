using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TurnProgress : MonoBehaviour
{
    [SerializeField] private UIManager UIManager;

    private List<Unit> allUnits;
    private List<EnemyUnit> enemyUnits;
    private List<PlayerUnit> playerUnits;

    private int currentTime;
    private int turnDuration = 10;

    private int enemysActionPoint = 12;

    public void StartPhase()
    {
        enemyUnits = new List<EnemyUnit>();
        playerUnits = new List<PlayerUnit>();
        allUnits = new List<Unit>(FindObjectsByType<Unit>(FindObjectsSortMode.None));
        foreach (Unit unit in allUnits)
        {
            if (unit is EnemyUnit)
            {
                enemyUnits.Add((EnemyUnit)unit);
            }
            else if (unit is PlayerUnit)
            {
                playerUnits.Add((PlayerUnit)unit);
            }
        }

        foreach (PlayerUnit unit in playerUnits)
        {
            unit.HideAllAttackIndicators();
        }

        DecideEnemysAction();
        currentTime = 0;
        StartCoroutine(ProgressTurn());
        UIManager.HideUnitInfo();
    }
    public void EndPhase()
    {
        StopAllCoroutines();
        foreach(Unit unit in allUnits)
        {
            unit.ClearAction();
        }
    }

    void Update()
    {
        //if (battleSystemManager.CurrentState != BattleState.PROGRESS) return;


    }

    public IEnumerator ProgressTurn()
    {
        yield return new WaitForSeconds(1f);


        while (currentTime < turnDuration)
        {

            currentTime++;
            Debug.Log("Turn Progress: " + currentTime + "/" + turnDuration);
            UIManager.UpdateTurnProgress(currentTime, turnDuration);

            DoAllUnitsAction();

            yield return new WaitForSeconds(1f);
        }
        BattleSystemManager.Instance.OnTurnEnd();
    }

    private void DoAllUnitsAction()
    {
        foreach(Unit unit in allUnits)
        {
            if (unit != null)
            {
                unit.DequeueAction();
            }
        }
    }

    // 적 유닛들의 행동 결정
    private void DecideEnemysAction()
    {
        int currentEnemysActionPoint = enemysActionPoint;

        List<EnemyUnit> aliveEnemies = enemyUnits.FindAll(e => e.IsAlive);
        List<PlayerUnit> alivePlayers = playerUnits.FindAll(p => p.IsAlive);

        while(currentEnemysActionPoint > 0)
        {
            EnemyUnit enemy = aliveEnemies[Random.Range(0, aliveEnemies.Count)];

            // 타겟 플레이어 유닛 선정 (70% 확률로 가장 가까운 유닛, 30% 확률로 랜덤 유닛)
            PlayerUnit targetPlayer = null;
            if (Random.Range(0f, 1f) < 0.7f)
            {
                targetPlayer = FindClosestPlayerUnit(alivePlayers, enemy);
            }
            else
            {
                targetPlayer = alivePlayers[Random.Range(0, alivePlayers.Count)];
            }
            enemy.SetTargetPlayerUnit(targetPlayer);

            // 액션 포인트 할당
            int maxPointsForThisEnemy = Mathf.Min(4, currentEnemysActionPoint);
            int points = Random.Range(0, maxPointsForThisEnemy + 1);

            enemy.DecideAction(points);

            currentEnemysActionPoint -= points;
            if (currentEnemysActionPoint <= 0) break;

            //ShuffleEnemyList(aliveEnemies);
        }
    }

    private PlayerUnit FindClosestPlayerUnit(List<PlayerUnit> alivePlayers, EnemyUnit enemy)
    {
        PlayerUnit closestPlayer = null;
        float closestDistance = Mathf.Infinity;
        foreach (PlayerUnit player in alivePlayers)
        {
            float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        return closestPlayer;
    }

    private void ShuffleEnemyList(List<EnemyUnit> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            var temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }
}
