using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

/// <summary>
/// Class used for handling tiles in the unity project.
/// </summary>
public class TileManager : MonoBehaviour {

    [SerializeField]
    private GameObject tilemapObject;
    [SerializeField]
    private TileBase baseTile;
    private Tilemap tilemap;

    //width and height of grid
    public static int gridWidth = 16;
    public static int gridHeight = 14;


	void Start () {
        tilemap = tilemapObject.GetComponent<Tilemap>();
	}
	
	void Update () {
        //if mouse is down, call getTilePosition, and flip the state of the tile position.
        if (Input.GetMouseButtonDown(0)){
            Vector3 tilePosition = getTilePosition();
            flipTile(tilePosition);
        }
    }

    /// <summary>
    /// Get the current vector of the mouse, and translate that vector into a cell position for the tilemap.
    /// </summary>
    /// <returns>The cell position of the mouse.</returns>
    private Vector3 getTilePosition() {
        Vector3 mousePos = Interaction.getMousePosition();
        return tilemap.WorldToCell(mousePos);
    }

    /// <summary>
    /// Flip the state of a tile for a given position.
    /// </summary>
    /// <param name="floatTilePos">position of tile to be flipped</param>
    private void flipTile(Vector3 floatTilePos) {
        Vector3Int tilePos = Vector3Int.RoundToInt(floatTilePos);

        //if there is an activated tile in tile position, set it to null. Else activate it.
        if (tilemap.HasTile(tilePos)){
            tilemap.SetTile(tilePos, null);
        }else {
            if (tilePos.x >= 0 && tilePos.x <= (gridWidth-1) && tilePos.y<=(gridWidth-1) && tilePos.y>=0) {
                tilemap.SetTile(tilePos, baseTile);
            }
        }
    }

}
