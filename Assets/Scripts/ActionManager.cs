using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [SerializeField] private UIManager UIManager;

    [SerializeField] private LayerMask playerUnitLayer; // 플레이어 유닛 레이어: 유닛 선택용
    [SerializeField] private LayerMask tileLayer;       // 타일 레이어: 이동할 타일 선택용

    [SerializeField] private GameObject moveIconPrefab;
    [SerializeField] private GameObject attackIconPrefab;

    private List<GameObject> actionIcons;               // 행동 초기화/Progress로 넘길 때 제거하기 위해

    private int currentActionPoint = 10;
    private int maxActionPoint = 10;
    private PlayerUnit currentUnit;

    private List<Unit> playerUnits;

    // 행동: 이동 관련
    private bool isMoveMode = false;
    private List<Tile> highlightedTiles = new List<Tile>();

    // 행동: 공격 관련
    private bool isAttackMode = false;
    private AttackRangeIndicator currentAttackIndicator;

    // 행동 선택 페이즈 시작
    public void StartPhase()
    {
        playerUnits = new List<Unit>(FindObjectsByType<PlayerUnit>(FindObjectsSortMode.None));
        actionIcons = new List<GameObject>();
        currentActionPoint = maxActionPoint;
        currentUnit = null;
        UIManager.UpdateActionPoints(currentActionPoint, maxActionPoint);

        foreach(Unit unit in playerUnits)
        {
            unit.InitializePosition();
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
        currentActionPoint = maxActionPoint;
        UIManager.UpdateActionPoints(currentActionPoint, maxActionPoint);
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
        UIManager.HideUnitInfo();
        UIManager.HideUnitActionOption();
    }
    void Update()
    {
        if(BattleSystemManager.Instance.CurrentState != BattleState.THINKING)
        {
            return;
        }
        SelectUnit();
        SelectAction();

        if (isAttackMode && currentAttackIndicator != null && !currentAttackIndicator.IsLocked)
        {
            UpdateAttackDirection();
        }
    }

    private void SelectUnit()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50, playerUnitLayer))
            {
                PlayerUnit selectedUnit = hit.transform.GetComponent<PlayerUnit>();
                if (selectedUnit != null)
                {
                    currentUnit = selectedUnit;
                    UIManager.ShowUnitInfo(currentUnit);
                    UIManager.ShowUnitActionOption();
                }
            }
        }
    }

    // 행동 선택: m키로 이동 모드 진입/종료, 이동 타일 선택
    // w키로 대기 선택
    // a키로 공격 모드 진입/종료, 공격 범위 선택
    private void SelectAction()
    {
        if(currentUnit == null)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!isMoveMode && currentActionPoint > 0)
            {
                EnterMoveMode();
            }
            else
            {
                ExitMoveMode();
            }
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            if(!isAttackMode && currentActionPoint > 0)
            { 
                EnterAttackMode();
            }
            else
            {
                ExitAttackMode();
            }
        }

        if (isMoveMode && Input.GetMouseButton(0))
        {
            SelectMoveTile();
        }

        if (isAttackMode && Input.GetMouseButtonDown(0))
        {
            ConfirmAttackDirection();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            EnterWait();
        }
    }

    public void EnterWait()
    {
        if(currentUnit == null)
        {
            return;
        }
        currentUnit.EnQueueAction(ActionType.WAIT);
        UIManager.HideUnitActionOption();
    }

    // Move Mode: m키 누를 시 이동 가능한 타일 하이라이트
    public void EnterMoveMode()
    {
        if(currentActionPoint <= 0)
        {
            return;
        }
        UIManager.HideUnitActionOption();
        Debug.Log("Enter Move Mode");
        isMoveMode = true;
        Tile currentTile = currentUnit.GetCurrentTile();
        if (currentTile != null)
        {
            HighlightMoveableTiles(currentTile);
        }
    }

    // Attack Mode: a키 누를 시 공격 방향 지정 인디케이터 표시
    public void EnterAttackMode()
    {
        if (currentActionPoint <= 0)
        {
            return;
        }
        UIManager.HideUnitActionOption();
        Debug.Log("Enter Attack Mode");
        currentAttackIndicator = currentUnit.ShowAttackRangeIndicator();
        isAttackMode = true;
        Debug.Log(currentAttackIndicator);
    }

    // 공격 방향 업데이트 (마우스 위치로 부채꼴 회전)
    private void UpdateAttackDirection()
    {
        // 마우스 위치를 월드 좌표로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.back, currentUnit.transform.position);
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            currentAttackIndicator.UpdateDirection(worldPoint);
        }
    }
    // 공격 방향 확정 (마우스 클릭 시)
    private void ConfirmAttackDirection()
    {
        if (currentAttackIndicator == null)
            return;
        if(currentAttackIndicator.IsLocked)
            return;

        currentAttackIndicator.Lock();
        currentUnit.EnQueueAction(ActionType.ATTACK);
        currentActionPoint--;
        UIManager.UpdateActionPoints(currentActionPoint, maxActionPoint);
        ExitAttackMode();
    }

    // Move Mode
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
                currentActionPoint--;
                UIManager.UpdateActionPoints(currentActionPoint, maxActionPoint);

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
    private void ExitAttackMode()
    {
        if(currentAttackIndicator != null && !currentAttackIndicator.IsLocked)
        {
            Destroy(currentAttackIndicator.gameObject);
        }
        isAttackMode = false;
        currentUnit = null;
        currentAttackIndicator = null;
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
