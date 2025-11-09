using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rune_", menuName = "Inventory/Rune Data")]
public class RuneData : ScriptableObject
{
    [Header("룬 정보")]
    public string runeName;
    public Sprite runeIcon;

    [Header("오버라이드(덮어쓰기)")]
    public GameObject overrideProjectilePrefab;
    public string description;
    public Sprite icon;
    public string skillName;

    [Header("추가 효과(SkillEffect)")]
    public SkillEffect additionalEffect;
}
