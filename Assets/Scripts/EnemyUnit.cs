using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    private PlayerUnit targetPlayerUnit;

    [SerializeField]private AttackRangeIndicator EnemyAttackRangeIndicator;

    protected override void Start()
    {
        base.Start();
        EnemyAttackRangeIndicator.Initialize(this);
    }

    public void SetTargetPlayerUnit(PlayerUnit playerUnit)
    {
        targetPlayerUnit = playerUnit;
    }

    // 적 AI 행동 결정
    // 사용할 액션 포인트를 받아 행동 결정
    public void DecideAction(int points)
    {
        if(targetPlayerUnit == null) return;

        for(int i = 0; i < points; i++)
        {
            Vector2 directionToPlayer = GetToTargetDirection();
            Vector3 expectedLoc = GetExpectedLoc(transform.position);

            // 랜덤으로 대기 행동 추가
            if (Random.Range(0f, 1f) < 0.3f && GetActionQueue().Count + (points - i) < 10) 
                EnQueueAction(ActionType.WAIT);

            // 타겟 플레이어 유닛이 가까우면 공격 행동
            float distance = Vector3.Distance(expectedLoc, targetPlayerUnit.transform.position);
            if(distance <= 1.5f && Random.Range(0f, 1f) > 0.5f)
            {
                EnQueueAction(ActionType.ATTACK);
                continue;
            }
            // 이동 행동 (70% 확률로 타켓 방향, 30% 확률로 랜덤)
            else if(Random.Range(0f, 1f) < 0.7f)
            {
                if(Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.y))
                {
                    if(directionToPlayer.x > 0)
                    {
                        EnQueueAction(ActionType.MOVERIGHT);
                    }
                    else
                    {
                        EnQueueAction(ActionType.MOVELEFT);
                    }
                }
                else
                {
                    if(directionToPlayer.y > 0)
                    {
                        EnQueueAction(ActionType.MOVEUP);
                    }
                    else
                    {
                        EnQueueAction(ActionType.MOVEDOWN);
                    }
                }
            }
            else
            {
                MoveRandom();
            }
        }
    }
    private void MoveRandom()
    {
        ActionType nextMove = new ActionType();
        do {             
            int rand = Random.Range(0, 4);
            switch(rand)
            {
                case 0:
                    nextMove = ActionType.MOVEUP;
                    break;
                case 1:
                    nextMove = ActionType.MOVEDOWN;
                    break;
                case 2:
                    nextMove = ActionType.MOVERIGHT;
                    break;
                case 3:
                    nextMove = ActionType.MOVELEFT;
                    break;
                default:
                    break;
            }
        } while (!isValidMove(GetExpectedLoc(transform.position), nextMove));
        EnQueueAction(nextMove);
    }
    private bool isValidMove(Vector3 expectedLoc, ActionType moveAction)
    {
        Vector3 newPos = expectedLoc;
        switch(moveAction)
        {
            case ActionType.MOVEUP:
                newPos += Vector3.up;
                break;
            case ActionType.MOVEDOWN:
                newPos += Vector3.down;
                break;
            case ActionType.MOVERIGHT:
                newPos += Vector3.right;
                break;
            case ActionType.MOVELEFT:
                newPos += Vector3.left;
                break;
            default:
                break;
        }
        RaycastHit hit;
        if (Physics.Raycast(newPos - Vector3.forward, Vector3.forward, out hit, 1f, LayerMask.GetMask("Ground")))
            return true;
        return false;
    }

    private Vector2 GetToTargetDirection()
    {
        Vector2 direction = targetPlayerUnit.transform.position - transform.position;
        direction.Normalize();
        return direction;
    }

    private Vector3 GetExpectedLoc(Vector3 currentPos)
    {
        Queue<ActionType> tempQueue = new Queue<ActionType>(GetActionQueue());
        Vector3 expectedPos = currentPos;

        while(tempQueue.Count > 0)
        {
            ActionType action = tempQueue.Dequeue();
            switch(action)
            {
                case ActionType.MOVEUP:
                    expectedPos += Vector3.up;
                    break;
                case ActionType.MOVEDOWN:
                    expectedPos += Vector3.down;
                    break;
                case ActionType.MOVERIGHT:
                    expectedPos += Vector3.right;
                    break;
                case ActionType.MOVELEFT:
                    expectedPos += Vector3.left;
                    break;
                default:
                    break;
            }
        }
        return expectedPos;
    }

    protected override void PerformAttack(int time)
    {
        base.PerformAttack(time);
        StartCoroutine(attackCoroutine(time));
    }

    IEnumerator attackCoroutine(int time)
    {
        EnemyAttackRangeIndicator.Show();
        yield return new WaitForSeconds(time);
        EnemyAttackRangeIndicator.Hide();
    }
}
