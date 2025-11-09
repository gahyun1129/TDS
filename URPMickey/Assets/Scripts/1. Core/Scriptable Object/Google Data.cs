using UnityEngine;

[CreateAssetMenu(fileName = "Rune_", menuName = "GameData/Rune Definition")]
public class RuneDefinition : ScriptableObject
{
    public string RuneID;
    public string DisplayName;
    public string IconName;
    public StatData TargetStatType; // StatType SO를 직접 연결
    public StatModType ModType;
    public float Value;
}