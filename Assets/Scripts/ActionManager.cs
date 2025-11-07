using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [SerializeField] private BattleSystemManager battleSystemManager;
    [SerializeField] private UIManager UIManager;

    [SerializeField] private LayerMask playerUnitLayer; // 플레이어 유닛 레이어: 유닛 선택용
    [SerializeField] private LayerMask tileLayer;       // 타일 레이어: 이동할 타일 선택용

    [SerializeField] private GameObject moveIconPrefab;
    [SerializeField] private GameObject attackIconPrefab;

    private List<GameObject> actionIcons;

    private int actionCount = 10;
    private Unit currentUnit;

    private List<Unit> playerUnits;

    // 행동: 이동 관련
    private bool isMoveMode = false;
    private List<Tile> highlightedTiles = new List<Tile>();

    // 행동 선택 페이즈 시작
    public void StartPhase()
    {
        playerUnits = new List<Unit>();
        actionIcons = new List<GameObject>();
        actionCount = 10;
        currentUnit = null;
        UIManager.UpdateActionPoints(actionCount, 10);

        GameObject[] playerUnit = GameObject.FindGameObjectsWithTag("PlayerUnit");
        foreach(GameObject unitObj in playerUnit)
        {
            Unit unit = unitObj.GetComponent<Unit>();
            playerUnits.Add(unit);
            unit.InitializeTransform();
        }
    }

    public void EndPhase()
    {
        ResetField();
        foreach (Unit unit in playerUnits)
        {
            unit.ResetPosition();
        }
    }

    public void ResetAction()
    {
        actionCount = 10;
        UIManager.UpdateActionPoints(actionCount, 10);
        ResetField();
        foreach (Unit unit in playerUnits)
        {
            unit.ResetPosition();
            unit.ClearAction();
        }
    }

    private void ResetField()
    {
        ClearHighlightedTiles();
        currentUnit = null;
        isMoveMode = false;
        foreach (GameObject icon in actionIcons)
        {
            Destroy(icon);
        }
        actionIcons.Clear();
    }
    void Update()
    {
        if(battleSystemManager.CurrentState != BattleState.THINKING)
        {
            return;
        }
        SelectUnit();
        SelectAction();
    }

    private void SelectUnit()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50, playerUnitLayer))
            {
                Unit selectedUnit = hit.transform.GetComponent<Unit>();
                if (selectedUnit != null)
                {
                    currentUnit = selectedUnit;
                    Debug.Log("Selected unit: " + currentUnit.unitName);
                }
            }
        }
    }

    // 행동 선택: m키로 이동 모드 진입/종료, 이동 타일 선택
    private void SelectAction()
    {
        if(currentUnit == null)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!isMoveMode && actionCount > 0)
            {
                EnterMoveMode();
            }
            else
            {
                ExitMoveMode();
            }
        }

        if(isMoveMode && Input.GetMouseButton(0))
        {
            SelectMoveTile();
        }
    }

    // Move Mode: m키 누를 시 이동 가능한 타일 하이라이트
    private void EnterMoveMode()
    {
        Debug.Log("Enter Move Mode");
        isMoveMode = true;
        Tile currentTile = currentUnit.GetCurrentTile();
        if (currentTile != null)
        {
            HighlightMoveableTiles(currentTile);
        }
    }

    // 하이라이트된 타일 중 하나 선택 시 이동 액션 큐에 추가
    private void SelectMoveTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 50, tileLayer))
        {
            Tile tile = hit.transform.GetComponent<Tile>();

            if (tile != null && highlightedTiles.Contains(tile))
            {
                Tile currentTile = currentUnit.GetCurrentTile();
                Debug.Log("Selected move tile");
                ActionType movingDirection = GetMoveActionType(currentTile, tile);
                currentUnit.EnQueueAction(movingDirection);
                actionCount--;
                UIManager.UpdateActionPoints(actionCount, 10);

                CreateMoveIcon(movingDirection, currentTile);

                currentUnit.transform.position = tile.transform.position;

                ExitMoveMode();
            }
        }
    }
    private ActionType GetMoveActionType(Tile fromTile, Tile toTile)
    {
        if (toTile == fromTile.GetNearTile(Direction.UP))
        {
            return ActionType.MOVEUP;
        }
        else if (toTile == fromTile.GetNearTile(Direction.DOWN))
        {
            return ActionType.MOVEDOWN;
        }
        else if (toTile == fromTile.GetNearTile(Direction.LEFT))
        {
            return ActionType.MOVELEFT;
        }
        else if (toTile == fromTile.GetNearTile(Direction.RIGHT))
        {
            return ActionType.MOVERIGHT;
        }
        return ActionType.WAIT; // 기본값
    }

    private void CreateMoveIcon(ActionType actionType, Tile currentTile)
    {
        Vector3 iconPosition = currentTile.transform.position;

        GameObject iconOb = Instantiate(moveIconPrefab, iconPosition, Quaternion.identity);
        switch (actionType)
        {
            case ActionType.MOVEUP:
                iconOb.transform.Rotate(0, 0, 90);
                break;
            case ActionType.MOVEDOWN:
                iconOb.transform.Rotate(0, 0, -90);
                break;
            case ActionType.MOVELEFT:
                iconOb.transform.Rotate(0, 0, 180);
                break;
            case ActionType.MOVERIGHT:
                iconOb.transform.Rotate(0, 0, 0);
                break;
        }
        actionIcons.Add(iconOb);
    }

    private void ExitMoveMode()
    {
        isMoveMode = false;
        ClearHighlightedTiles();
        currentUnit = null;
    }

    // 현재 타일 기준으로 상하좌우 인접 타일 하이라이트
    private void HighlightMoveableTiles(Tile centerTile)
    {
        Direction[] directions = { Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT };
        foreach (Direction direction in directions)
        {
            Tile nearTile = centerTile.GetNearTile(direction);
            if (nearTile != null)
            {
                nearTile.SetHighlight(true);
                highlightedTiles.Add(nearTile);
            }
        }
    }
    private void ClearHighlightedTiles()
    {
        foreach (Tile tile in highlightedTiles)
        {
            if(tile != null)
            {
                tile.SetHighlight(false);
            }
        }
        highlightedTiles.Clear();
    }

}
