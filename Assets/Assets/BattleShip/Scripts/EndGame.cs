using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public TextMeshProUGUI resultText; // Assign this in the editor

    void Start()
    {
        string result = PlayerPrefs.GetString("endGameResult", "YOU WON!");
        resultText.text = result;
    }
    public void OnRestartButtonClicked()
    {
        SceneManager.LoadScene("BattleShipScene"); // Replace "GameScene" with your game scene's name
    }
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player has quit game");
    }
}
