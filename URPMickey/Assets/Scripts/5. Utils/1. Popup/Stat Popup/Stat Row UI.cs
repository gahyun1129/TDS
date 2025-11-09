using UnityEngine;
using UnityEngine.UI;

public class StatRowUI : MonoBehaviour
{
    [Header("스탯 UI 컴포넌트")]
    [SerializeField] private Text statNameText;
    [SerializeField] private Text statValueText;

    public StatType AssignedStatType { get; private set; }

    public void Setup(StatType type, string statName, float statValue)
    {
        AssignedStatType = type;
        statNameText.text = statName;
        statValueText.text = statValue.ToString("F0");
    }
}
