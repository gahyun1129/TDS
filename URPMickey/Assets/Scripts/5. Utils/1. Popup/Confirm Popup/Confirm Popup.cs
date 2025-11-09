using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

public class ConfirmPopup : PopupBase
{
    [SerializeField] private Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    protected override void Awake()
    {
        base.Awake(); 
        cancelButton.onClick.AddListener(Close); 
    }

    public void Setup(string message, UnityAction onConfirmAction)
    {
        messageText.text = message;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(onConfirmAction);
        confirmButton.onClick.AddListener(Close);
    }
}