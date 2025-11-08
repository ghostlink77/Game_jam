using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public enum ActionType
{
    ATTACK,
    MOVEUP,
    MOVERIGHT,
    MOVELEFT,
    MOVEDOWN,
    WAIT
}

public class Unit : MonoBehaviour
{
    public string unitName;
    [SerializeField] private int MaxHp = 3;
    [SerializeField] private int attackTime = 1;
    private int hp;
    public int Hp
    {
        get { return hp; }
        set { if(value >= 0 && value <= MaxHp) hp = value; }
    }

    private bool isAlive = true;
    public bool IsAlive { get { return isAlive; } }

    private Vector3 initialPosition;

    private Queue<ActionType> actionQueue;
    

    protected virtual void Start()
    {
        actionQueue = new Queue<ActionType>(10);
        Hp = MaxHp;
    }
    public void TakeDamage()
    {
        hp--;
        if(hp <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log(unitName + " took damage! Remaining health: " + hp);
        }
    }

    private void Die()
    {
        ClearAction();
        isAlive = false;
        gameObject.SetActive(false);
    }


    public void InitializePosition()
    {
        initialPosition = transform.position;
    }
    public void ResetPosition()
    {
        if(initialPosition != null)
        {
            transform.position = initialPosition;
        }
    }
    public void EnQueueAction(ActionType action)
    {
        if(actionQueue.Count >= 10)
        {
            Debug.Log("Action queue is full!");
            return;
        }
        Debug.Log(unitName + " enqueued action: " + action.ToString());
        actionQueue.Enqueue(action);
    }

    public Tile GetCurrentTile()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position - Vector3.forward, Vector3.forward, out hit, 1f, LayerMask.GetMask("Ground")))
        {
            Debug.Log("Current Tile: " + hit.transform.name);
            return hit.transform.GetComponent<Tile>();
        }
        return null;
    }

    private void DoAction(ActionType action)
    {
        switch (action)
        {
            case ActionType.ATTACK:
                PerformAttack(attackTime);
                break;
            case ActionType.MOVEUP:
            case ActionType.MOVERIGHT:
            case ActionType.MOVELEFT:
            case ActionType.MOVEDOWN:
                Move(action);
                Debug.Log(unitName + " moves " + action.ToString().Substring(4) + ".");
                break;
            case ActionType.WAIT:
                Debug.Log(unitName + " waits.");
                break;
        }
    }

    public void DequeueAction()
    {
        if (actionQueue.Count == 0)
        {
            Debug.Log(unitName + " has no actions to perform.");
            DoAction(ActionType.WAIT);
            return;
        }
        ActionType action = actionQueue.Dequeue();
        Debug.Log(unitName + " dequeued action: " + action.ToString());
        DoAction(action);
    }
    public virtual void ClearAction()
    {
        actionQueue.Clear();
    }

    protected virtual void PerformAttack(int time)
    {
        Debug.Log(unitName + " performs an attack!");
    }
    private void Move(ActionType action)
    {
        Tile tile = GetCurrentTile();
        Tile destTile = null;
        if (action == ActionType.MOVEUP)
        {
            destTile = tile.GetNearTile(Direction.UP);
        }
        else if(action == ActionType.MOVEDOWN)
        {
            destTile = tile.GetNearTile(Direction.DOWN);
        }
        else if(action == ActionType.MOVELEFT)
        {
            destTile = tile.GetNearTile(Direction.LEFT);
        }
        else if(action == ActionType.MOVERIGHT)
        {
            destTile = tile.GetNearTile(Direction.RIGHT);
        }

        if (destTile != null)
        {
            transform.position = destTile.transform.position;
        }
    }

    public Queue<ActionType> GetActionQueue()
    {
        return actionQueue;
    }
}
