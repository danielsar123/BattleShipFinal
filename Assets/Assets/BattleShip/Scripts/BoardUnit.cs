using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardUnit : MonoBehaviour
{
    public TMP_Text tmpBoardUnitLabel;
    public int row;
    public int col;
    public Ship AssignedShip { get; private set; }

    public bool occupied = false;
    public bool hit = false;

    // Colors for different states
    public Color hitColor = Color.red;
    public Color missColor = Color.blue;

    private GameObject cubeInstance; // The instantiated cube GameObject

    // Start is called before the first frame update
    void Start()
    {
        tmpBoardUnitLabel.text = $"B{row},{col}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetShip(Ship ship)
    {
        AssignedShip = ship;
    }

    // Method to process hit
    public void ProcessHit()
    {
        // Set the hit flag depending on whether the unit was occupied
        hit = occupied;
        tmpBoardUnitLabel.text = "X"; // Set the text to "X"
        tmpBoardUnitLabel.color = Color.red; // Set the text color to red
        tmpBoardUnitLabel.fontSize = 36; // Adjust the font size as needed

        // UpdateColor(); // Update its color based on whether it's a hit or miss
    }

    // Method to create a cube with the specified color
    private void CreateCube()
    {
        // Create a new cube GameObject
        cubeInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeInstance.transform.position = new Vector3(transform.position.x, transform.position.y - 10, transform.position.z);
    }

    // Method to update the cube's color
    private void UpdateColor()
    {
        if (cubeInstance != null)
        {
            cubeInstance.GetComponent<Renderer>().material.color = hit ? hitColor : missColor;
        }
    }
}
