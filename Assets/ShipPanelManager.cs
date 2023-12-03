using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipPanelManager : MonoBehaviour
{
    // Dictionary to hold the ship button GameObjects
    private Dictionary<string, GameObject> shipButtons;

    private void Awake()
    {
        // Initialize the dictionary
        shipButtons = new Dictionary<string, GameObject>();

        // Assign ship buttons to the dictionary with the ship names
        shipButtons.Add("Ship0", GameObject.Find("Ship0Button"));
        shipButtons.Add("Ship1", GameObject.Find("Ship1Button"));
        shipButtons.Add("Ship2", GameObject.Find("Ship2Button"));
        shipButtons.Add("Ship3", GameObject.Find("Ship3Button"));
        shipButtons.Add("Ship4", GameObject.Find("Ship4Button"));

        // Add listeners or initialization code here if necessary
    }

    public void UpdateShipPanel(string shipName, bool isSunk)
    {
        // Check if the dictionary contains the ship
        if (shipButtons.ContainsKey(shipName))
        {
            // Get the button GameObject
            GameObject shipButton = shipButtons[shipName];

            // Update the button's appearance based on whether the ship is sunk
            if (isSunk)
            {
                // Here you can set the button to inactive or change its appearance
                shipButton.SetActive(false);
            }
            else
            {
                // If for some reason the ship is not sunk anymore, you can re-enable it
                shipButton.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Ship name not recognized: " + shipName);
        }
    }

    // Call this method when a ship is sunk to update the UI
    public void OnShipSunk(string shipName)
    {
        UpdateShipPanel(shipName, true);
    }

    // If you have a method to repair ships or bring them back, call this method
    public void OnShipRepaired(string shipName)
    {
        UpdateShipPanel(shipName, false);
    }

    // You can add more methods to manage the ship panel state as needed
}
