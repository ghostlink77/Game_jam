using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Action Points UI")]
    [SerializeField] private Slider actionPointSlider;
    [SerializeField] private TextMeshProUGUI actionPointText;

    [Header("Turn Progress UI")]
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI timeText;


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
}
