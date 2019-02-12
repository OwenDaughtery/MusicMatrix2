using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TileManager : MonoBehaviour {

    [SerializeField]
    private GameObject tilemapObject;
    [SerializeField]
    private TileBase baseTile;
    private Tilemap tilemap;
    //
    public static int gridWidth = 16;
    //
    public static int gridHeight = 14;


	void Start () {
        tilemap = tilemapObject.GetComponent<Tilemap>();
	}
	
	void Update () {
        if (Input.GetMouseButtonDown(0)){
            Vector3 tilePosition = getTilePosition();
            flipTile(tilePosition);
        }
    }

    private Vector3 getTilePosition() {
        Vector3 mousePos = Interaction.getMousePosition();
        return tilemap.WorldToCell(mousePos);
    }

    private void flipTile(Vector3 floatTilePos) {
        Vector3Int tilePos = Vector3Int.RoundToInt(floatTilePos);
        //TileBase tile = tilemap.GetTile(Vector3Int.RoundToInt(tilePos));
        
        if (tilemap.HasTile(tilePos)){
            tilemap.SetTile(tilePos, null);
        }else {
            if (tilePos.x >= 0 && tilePos.x <= (gridWidth-1) && tilePos.y<=(gridWidth-1) && tilePos.y>=0) {
                tilemap.SetTile(tilePos, baseTile);
            }
        }
    }

}
