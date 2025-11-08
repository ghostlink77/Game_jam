using UnityEngine;

public class AttackRangeIndicator : MonoBehaviour
{
    private Unit unit;

    private bool isLocked = false;
    public bool IsLocked { get { return isLocked; } }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Unit unit)
    {
        this.unit = unit;
        transform.position = unit.transform.position;
        isLocked = false;
    }
    public void UpdateDirection(Vector3 targetPosition)
    {
        if(isLocked) return;

        Vector3 direction = targetPosition - transform.position;
        direction.z = 0;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle-45);
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(BattleSystemManager.Instance.CurrentState != BattleState.PROGRESS) return;

        if((unit is PlayerUnit && other.gameObject.GetComponent<EnemyUnit>() != null) ||
           (unit is EnemyUnit && other.gameObject.GetComponent<PlayerUnit>() != null) )
        {
            other.gameObject.GetComponent<Unit>().TakeDamage();
        }
    }
}
