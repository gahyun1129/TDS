using UnityEngine;

[CreateAssetMenu(fileName = "New Stat", menuName = "GameData/Stat Data")]
public class StatData : ScriptableObject
{
    public StatType type;
    public string statName;
    [TextArea]
    public string Description;
    public string IconName;
}