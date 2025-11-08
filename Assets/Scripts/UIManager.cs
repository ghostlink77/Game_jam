using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Action Points UI")]
    [SerializeField] private Slider actionPointSlider;
    [SerializeField] private TextMeshProUGUI actionPointText;

    [Header("Turn Progress UI")]
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Unit Info UI")]
    [SerializeField] private GameObject unitInfoPanel;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI unitInfoText;

    [Header("Unit Action UI")]
    [SerializeField] private GameObject unitActionPanel;

    [Header("Result Text")]
    [SerializeField] private TextMeshProUGUI resultText;

    public void UpdateActionPoints(int currentPoints, int maxPoints)
    {
        actionPointSlider.maxValue = maxPoints;
        actionPointSlider.value = currentPoints;
        actionPointText.text = $"Action Point: {currentPoints} / {maxPoints}";
    }

    public void UpdateTurnProgress(int currentTime, int maxTime)
    {
        timeSlider.maxValue = maxTime;
        timeSlider.value = currentTime;
        timeText.text = $"{currentTime} / {maxTime}";
    }

    public void ShowUnitInfo(Unit unit)
    {
        string timeTableText = "HP: "+ unit.Hp.ToString() + "\n" + "Action Queue:\n";
        unitInfoPanel.SetActive(true);
        unitNameText.text = unit.unitName;
        Queue<ActionType> actionQueue = unit.GetActionQueue();
        for (int i = 0; i < actionQueue.Count; i++)
        {
            timeTableText += $"{i + 1}. {actionQueue.ToArray()[i]}\n";
        }
        unitInfoText.text = timeTableText;
    }
    public void ShowUnitActionOption()
    {
        unitActionPanel.SetActive(true);
    }
    public void HideUnitInfo()
    {
        unitInfoPanel.SetActive(false);
    }
    public void HideUnitActionOption()
    {
        unitActionPanel.SetActive(false);
    }

    public void Win()
    {
        resultText.gameObject.SetActive(true);
        resultText.text = "YOU WIN!!";
    }
    public void Lose()
    {
        resultText.gameObject.SetActive(true);
        resultText.text = "YOU LOSE..";
    }

    private void LateUpdate()
    {
        if(BattleSystemManager.Instance.CurrentState != BattleState.THINKING) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("PlayerUnit")))
        {
            ShowUnitInfo(hit.transform.GetComponent<Unit>());
        }
        else
        {
            HideUnitInfo();
        }
    }
}
