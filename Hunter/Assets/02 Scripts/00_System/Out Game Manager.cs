using System;
using UnityEngine;

public class OutGameManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneChanger.Instance.LoadScene("InGame");
    }
}

