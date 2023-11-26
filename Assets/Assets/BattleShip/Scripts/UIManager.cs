using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public delegate void ChangeShip(int id, int size);
    public static event ChangeShip OnChangeShip;

    public delegate void ChangeOrientation(bool orientation);
    public static event ChangeOrientation OnChangeOrientation;

    [Header("Player Pieces References")]
    public List<Button> collectionOfPlayersPieceButtons;

    [Header("Game Button References")]
    public Button butOrientation;
    public Button butUIReset;
    public Button butExit;

    [Header("Sprite References")]
    public Sprite textureHorizontal;
    public Sprite textureVertical;

    [Header("Basic Flag Settings..")]
    public bool DisplayInstructions;
    public bool Orientation = false;

    // This dictionary is a simple data structure to store player's piece id and size of unit
    public Dictionary<int, int> boardPieces = new Dictionary<int, int>
    {
        { 0, 5 }, // aircraft carrier
        { 1, 4 }, // battleship
        { 2, 3 }, // submarine
        { 3, 3 }, // destoryer
        { 4, 2 }, // patrol boat

    };

    private void OnEnable()
    {
        
            BoardUnitManager.OnBoardPiecePlaced += BoardVer1_OnBoardPiecePlaced;
        
    }

    private void OnDisable()
    {
        BoardUnitManager.OnBoardPiecePlaced -= BoardVer1_OnBoardPiecePlaced;
    }

    private void BoardVer1_OnBoardPiecePlaced(int id)
    {

        if (id >= 0 && id < collectionOfPlayersPieceButtons.Count)
        {
            // disable button representing the piece
            Debug.Log($"id = {id}");
            collectionOfPlayersPieceButtons[id].gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Invalid id: {id}");
        }
        // disable button representing the piece
       // Debug.Log($"id = {id}");
     //   collectionOfPlayersPieceButtons[id].gameObject.SetActive(false);
    }

    private void Start()
    {
        UpdateOrientationUI();
    }

    public void butBoardPieceSelected(int shipID)
    {
        int size = boardPieces.ContainsKey(shipID) ? boardPieces[shipID] : -1;

        if (size == -1)
            Debug.LogWarning($"{shipID} does not exist in the collection");
        else
        {
            //pass the data
            OnChangeShip?.Invoke(shipID, size);
        }
    }

    public void butChangeOrientation()
    {
        Orientation = !Orientation;
        UpdateOrientationUI();
    }

    void UpdateOrientationUI()
    {
        if (Orientation)
        {
            butOrientation.image.sprite = textureVertical;

        }
        else
        {
            butOrientation.image.sprite = textureHorizontal;
        }

        OnChangeOrientation?.Invoke(Orientation);
    }
    
}
