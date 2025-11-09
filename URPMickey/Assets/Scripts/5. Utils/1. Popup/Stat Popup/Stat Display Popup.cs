using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class statDisplayPopup : PopupBase
{
    [SerializeField] private List<StatUIPair> statRows;
    [SerializeField] private Button cancelButton;
    private GameDataDataBase dataBase;
    private PlayerPersistentData playerData;

    protected override void Awake()
    {
        base.Awake();

        dataBase = GameDataManager.Instance.GetDataBase();
        playerData = GameDataManager.Instance.GetPlayerData();

        cancelButton.onClick.AddListener(Close); 
    }

    public void DisplayPopup()
    {
        foreach (var row in statRows)
        {
            var stat = dataBase.GetStat(row.type);
            int currentLevel = playerData.GetStatLevel(row.type);
            float currentValue = dataBase.GetStatValue(row.type, currentLevel);

            row.ui.Setup(row.type, stat.statName, currentValue);
        }
    }

    [System.Serializable]
    public class StatUIPair
    {
        public StatType type;
        public StatRowUI ui;
    }
}
