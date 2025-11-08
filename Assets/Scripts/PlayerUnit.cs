using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    private AttackRangeIndicator currentAttackRangeIndicator;
    private Queue<AttackRangeIndicator> attackRangeIndicators;
    [SerializeField] private GameObject attackRangeIndicatorPrefab;
    protected override void Start()
    {
        base.Start();
        attackRangeIndicators = new Queue<AttackRangeIndicator>();
    }

    public override void ClearAction()
    {
        base.ClearAction();
        RemoveAllAttackIndicators();
    }
    public AttackRangeIndicator ShowAttackRangeIndicator()
    {
        GameObject indicator = Instantiate(attackRangeIndicatorPrefab, transform.position, Quaternion.identity);
        AttackRangeIndicator attackRangeIndicator = indicator.GetComponent<AttackRangeIndicator>();
        attackRangeIndicator.Initialize(this);
        attackRangeIndicators.Enqueue(attackRangeIndicator);

        return attackRangeIndicator;
    }
    public void HideAllAttackIndicators()
    {
        foreach(var indicator in attackRangeIndicators)
        {
            indicator.Hide();
        }
    }

    public void RemoveAllAttackIndicators()
    {
        foreach(var indicator in attackRangeIndicators)
        {
            Destroy(indicator.gameObject);
        }
        attackRangeIndicators.Clear();
        currentAttackRangeIndicator = null;
    }

    protected override void PerformAttack(int time)
    {
        if(BattleSystemManager.Instance.CurrentState != BattleState.PROGRESS)
        {
            return;
        }
        base.PerformAttack(time);
        StartCoroutine(AttackCoroutine(time));
    }

    IEnumerator AttackCoroutine(int time)
    {
        currentAttackRangeIndicator = attackRangeIndicators.Dequeue();
        currentAttackRangeIndicator.Show();
        yield return new WaitForSeconds(time);
        currentAttackRangeIndicator.Hide();
    }
}

