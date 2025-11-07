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
    public int health;

    private Vector3 initialPosition;

    private Queue<ActionType> actionQueue;

    private void Start()
    {
        actionQueue = new Queue<ActionType>(10);
    }
    public void TakeDamage()
    {
        health--;
        if(health <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log(unitName + " took damage! Remaining health: " + health);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }


    public void InitializeTransform()
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
                Debug.Log(unitName + " performs ATTACK action.");
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
    public void ClearAction()
    {
        actionQueue.Clear();
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
}
