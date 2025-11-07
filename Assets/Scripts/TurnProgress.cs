using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TurnProgress : MonoBehaviour
{
    [SerializeField] private BattleSystemManager battleSystemManager;
    [SerializeField] private UIManager UIManager;

    private List<Unit> allUnits;

    private List<Unit> EnemyUnits;

    private int currentTime;
    private int turnDuration = 10;

    private int enemysActionPoint = 10;

    public void StartPhase()
    {
        allUnits = new List<Unit>(FindObjectsByType<Unit>(FindObjectsSortMode.None));
        GameObject[] playerUnit = GameObject.FindGameObjectsWithTag("EnemyUnit");
        foreach (GameObject unitObj in playerUnit)
        {
            Unit unit = unitObj.GetComponent<Unit>();
            EnemyUnits.Add(unit);
        }
        DecideEnemysAction();
        currentTime = 0;
        StartCoroutine(ProgressTurn());
        UIManager.HideUnitInfo();
    }

    // Update is called once per frame
    void Update()
    {
        if (battleSystemManager.CurrentState != BattleState.PROGRESS) return;


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
        battleSystemManager.OnTurnEnd();
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

    private void DecideEnemysAction()
    {
        enemysActionPoint = 10;

    }
}
