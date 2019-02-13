using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackManager : MonoBehaviour {
    float waitForSeconds = 0.15f; //time in between each note 0.25
    int octaveOffsetMultiplier = 3;
    int octaveOffset = 12;
    int timingCount = -1;


    [SerializeField]
    private GameObject readHeadMapObject;
    private Tilemap readHeadMap;
    [SerializeField]
    private TileBase readMapTileBase;
    [SerializeField]
    private GameObject tileMapObject;
    private Tilemap tileMap;
    [SerializeField]
    private GameObject markovTileObject;
    private Tilemap markovChainMap;
    [SerializeField]
    private TileBase markovTileBase;

    [SerializeField]
    private MarkovManager markovManager;

    public NoteManager.Notes key;
    private List<NoteManager.Notes> scale = new List<NoteManager.Notes>();
    private static int[] majorScale = new int[7] {0, 2, 4, 5, 7, 9, 11};

    void Start () {
        scale = generateScale(key, octaveOffsetMultiplier);
        readHeadMap = readHeadMapObject.GetComponent<Tilemap>();
        tileMap = tileMapObject.GetComponent<Tilemap>();
        markovChainMap = markovTileObject.GetComponent<Tilemap>();
        StartCoroutine(Loop());
    }

    //
    public Tilemap getTilemap() {
        return tileMap;
    }

    //
    public NoteManager.Notes getKey() {
        return key;
    }

    //
    public static List<NoteManager.Notes> generateScale(NoteManager.Notes key, int octaveMultiplier){
        int scaleOffset = ((int)key);
        List<NoteManager.Notes> scale = new List<NoteManager.Notes>();
        for (int i = 0; i < Mathf.RoundToInt(TileManager.gridHeight/7); i++) {
            foreach (int offset in majorScale){
                scale.Add((NoteManager.Notes)(offset + scaleOffset + (12 * (i + octaveMultiplier))));
            }
        }
        return scale;
    }

    void Update () {
        updateReadHead();
	}

    private void updateReadHead() {
        readHeadMap.ClearAllTiles();
        Vector3Int readHeadPos = new Vector3Int(timingCount, -1, 0);
        readHeadMap.SetTile(readHeadPos, readMapTileBase);
    }

    private List<NoteManager.Notes> selectedTilesAtTiming(int timing, List<Tilemap> mapsToSelectFrom) {
        Vector3Int tilePos;
        List<NoteManager.Notes> selectedNotes = new List<NoteManager.Notes>();
        for (int i = 0; i < TileManager.gridHeight; i++){
            tilePos = new Vector3Int(timing, i, 0);
            foreach (Tilemap tilemap in mapsToSelectFrom){
                if (tilemap.HasTile(tilePos)) {
                    //NoteManager.Notes note = getScaleNoteFromInt(i);
                    NoteManager.Notes note = getNoteFromInt(i+(12*2));//adding to i to move it up some octaves as its a bit too low to hear;
                    selectedNotes.Add(note);
                }
            }
        }
        return selectedNotes;
    }

    //
    public List<List<NoteManager.Notes>> getMelodyFromTilemap(Tilemap tilemap) {
        List<List<NoteManager.Notes>> melody = new List<List<NoteManager.Notes>>();
        for (int i = 0; i < TileManager.gridWidth; i++){
            melody.Add(selectedTilesAtTiming(i, new List<Tilemap>() {tilemap}));
        }

        return melody;
    }

    private NoteManager.Notes getNoteFromInt(int y) {
        y += 1;
        return((NoteManager.Notes)y);
    }

    private NoteManager.Notes getScaleNoteFromInt(int y){
        return (scale[y]);
    }


    //main looping method:
    IEnumerator Loop(){
        int bars = 0;
        List<NoteManager.Notes> selectedNotes = new List<NoteManager.Notes>();
        GameObject[] noteMapObjects = GameObject.FindGameObjectsWithTag("NoteMap");
        List<Tilemap> allNoteMaps = new List<Tilemap>();
        foreach (GameObject gameObject in noteMapObjects){
            allNoteMaps.Add(gameObject.GetComponent<Tilemap>());
        }
        while (true) {
            
            timingCount += 1;
            
            if ((timingCount % TileManager.gridWidth == 0) && timingCount>0) {
                bars+=1;
                //read head has reset
                timingCount %= TileManager.gridWidth;
                //markovManager.getMarkovChain().wipeChain();
                markovManager.influenceChain(tileMap);
                markovManager.influenceRhythmChain(tileMap);
                if(markovManager.getPhase()==0){
                    if(bars%4==0){
                        markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars=0;
                    }
                }else if(markovManager.getPhase()==1){
                    if(bars%2==0){
                        markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars=0;
                    }
                }
                
            }

            
            selectedNotes = selectedTilesAtTiming(timingCount, allNoteMaps);
            foreach (NoteManager.Notes note in selectedNotes){
                contactSC(note);
            }
           
            yield return new WaitForSeconds(waitForSeconds);

        }
        
    }

    public static void contactSC(NoteManager.Notes note)
    {
        //print("sending to play: " + note);
        //OSC Send
        List<string> args = new List<string>();
        args.Add("0.3f");
        args.Add(NoteManager.noteToFreq[note].ToString());
        args.Add("0.3f");
        OSCHandler.Instance.SendMessageToClient("SuperCollider", "/play" + "VoiceA", args);
      

    }


}
