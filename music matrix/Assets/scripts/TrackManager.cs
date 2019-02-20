using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrackManager : MonoBehaviour {
    //How many seconds to wait inbetween moving the read head.
    float waitForSeconds = 0.15f;

    //How many octaves notes should be transposed up from the bottom octave.
    int octaveOffsetMultiplier = 3;

    //How many notes in an octave.
    int octaveOffset = 12;
    //Where the read head should start/where it currently is.
    int timingCount = -1;

    //===Start: variables for tiles bases and objects.===
    [SerializeField]
    private GameObject readHeadMapObject;
    private Tilemap readHeadMap;
    [SerializeField]
    private  TileBase readMapTileBase;
    [SerializeField]
    private GameObject tileMapObject;
    private Tilemap tileMap;
    [SerializeField]
    private GameObject markovTileObject;
    private Tilemap markovChainMap;
    [SerializeField]
    private  TileBase markovTileBase;
    //==End: Variables for tiles bases and objects.===

    //Variable to hold the markovManager script.
    [SerializeField]
    private MarkovManager markovManager;

    //variable to hold the keyManager script.
    [SerializeField]
    private KeyManager keyManager;

    //Key selected by user. (NOTE: This will be changed to the predicted key in future version.)
    public NoteManager.Notes key;

    //List object to hold the scale for a given key.
    //private List<NoteManager.Notes> scale = new List<NoteManager.Notes>();
    //^^remove?
    
    //Static array to hold the semi-tone-steps of a major scale.
    private static int[] majorScale = new int[7] { 0, 2, 4, 5, 7, 9, 11 };

    void Start () {
        //scale = generateScale(key, octaveOffsetMultiplier);
        //^^remove?
        
        //Setting up 
        readHeadMap = readHeadMapObject.GetComponent<Tilemap>();
        tileMap = tileMapObject.GetComponent<Tilemap>();
        markovChainMap = markovTileObject.GetComponent<Tilemap>();

        //Start coroutine that represents the read head moving from bar to bar.
        StartCoroutine(Loop());
    }

    /// <summary>
    /// Method used to get Tilemap variable
    /// </summary>
    /// <returns>the tilemap containg the notes from the user</returns>
    public Tilemap getTilemap() {
        return tileMap;
    }

    /// <summary>
    /// Get current key of system.
    /// </summary>
    /// <returns>NoteManager enum of current key.</returns>
    public NoteManager.Notes getKey() {
        return key;
    }

    /// <summary>
    /// Given a key, generates all of the notes in that key.
    /// </summary>
    /// <param name="key">The key of the scale to generate</param>
    /// <param name="octaveMultiplier">What octave the scale should start at.</param>
    /// <returns>A list of NoteManager enums of the scale in the given key, Does multiple octaves according to gridheight(possible)</returns>
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

    /// <summary>
    /// Move read head along by 1.
    /// </summary>
    private void updateReadHead() {
        readHeadMap.ClearAllTiles();
        Vector3Int readHeadPos = new Vector3Int(timingCount, -1, 0);
        readHeadMap.SetTile(readHeadPos, readMapTileBase);
    }

    /// <summary>
    /// For a given list of tilemaps, get all of the "selected" notes from those tilemap.
    /// Mainly used to get all of the notes that should be played for a certain "beat".
    /// </summary>
    /// <param name="timing">The column to index of the tilemaps.</param>
    /// <param name="mapsToSelectFrom">A list of maps to get notes from.</param>
    /// <returns>A list of NoteManager enums of all of the selected notes in the given tilemaps. (Note: Can have multiple occurences of notes.)</returns>
    private List<NoteManager.Notes> selectedTilesAtTiming(int timing, List<Tilemap> mapsToSelectFrom) {
        Vector3Int tilePos;
        List<NoteManager.Notes> selectedNotes = new List<NoteManager.Notes>();
        //Starting from the bottom of the grid and working upwards:
        for (int i = 0; i < TileManager.gridHeight; i++){
            tilePos = new Vector3Int(timing, i, 0);
            foreach (Tilemap tilemap in mapsToSelectFrom){
                if (tilemap.HasTile(tilePos)) {
                    NoteManager.Notes note = getNoteFromInt(i+(12*2));//adding to i to move it up some octaves as its a bit too low to hear;
                    selectedNotes.Add(note);
                }
            }
        }
        return selectedNotes;
    }

    /// <summary>
    /// By disregarding rhythm completely, this function generates a list of lists where each list represents all of the notes that are played after all of the previous list of notes.
    /// </summary>
    /// <param name="tilemap">The tilemap to get the melody from</param>
    /// <returns>A list of list of NoteManager enums. Representing the melody without timing of the tilemap.</returns>
    public List<List<NoteManager.Notes>> getMelodyFromTilemap(Tilemap tilemap) {
        List<List<NoteManager.Notes>> melody = new List<List<NoteManager.Notes>>();
        for (int i = 0; i < TileManager.gridWidth; i++){
            melody.Add(selectedTilesAtTiming(i, new List<Tilemap>() {tilemap}));
        }

        return melody;
    }

    /// <summary>
    /// Given an int, get the NoteManager enum for that int.
    /// </summary>
    /// <param name="y">The passed int, must +1 to this int to take into account there is a dummy enum at the beginning of the Enum list for NoteManager.</param>
    /// <returns>NoteManager enum for the given int.</returns>
    private NoteManager.Notes getNoteFromInt(int y) {
        y += 1;
        return((NoteManager.Notes)y);
    }

    //private NoteManager.Notes getScaleNoteFromInt(int y){
    //    return (scale[y]);
    //}


    /// <summary>
    /// Main purpose of the class, a loop method that simulates the read head going round and round the music.
    /// </summary>
    /// <returns>Waits for however many seconds specified in the variable "waitForSeconds"</returns>
    IEnumerator Loop(){
        //How many bars have been played since the last reset of bars (For example: Every 4 bars do something and reset bars.)
        int bars = 0;
        List<NoteManager.Notes> selectedNotes = new List<NoteManager.Notes>();
        //Get all of the game objects with tag notemap, these notemaps represent the music to be played.
        GameObject[] noteMapObjects = GameObject.FindGameObjectsWithTag("NoteMap");
        List<Tilemap> allNoteMaps = new List<Tilemap>();
        //Get all of the tilemap components from the gameobjects in noteMapObjects.
        foreach (GameObject gameObject in noteMapObjects){
            allNoteMaps.Add(gameObject.GetComponent<Tilemap>());
        }

        while (true) {
            //variable to keet track of read head.
            timingCount += 1;
            
            //enter if statement if readhead has gone over width of grid.
            if ((timingCount % TileManager.gridWidth == 0) && timingCount>0) {
                bars+=1;
                //read head reset
                timingCount %= TileManager.gridWidth;
                //Given the users tilemap, influence the current markov chain and markovRhythm chain.
                markovManager.influenceChain(tileMap);
                markovManager.influenceRhythmChain(tileMap);

                //Check which phase system is in to do correct action.
                if(markovManager.getPhase()==0){
                    //Learning Phase
                    if(bars%4==0){
                        markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars=0;
                    }
                }else if(markovManager.getPhase()==1){
                    //Breeding Phase
                    if(bars%1==0){
                        markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars=0;
                    }
                }
                //update KeyManager with user inputted notes from bar just played.
                List<NoteManager.Notes> predictedKeys = keyManager.adaptkey(getMelodyFromTilemap(tileMap));
                markovManager.weightInKeyNotes(predictedKeys);

            }

            //once read head and variables have been set correctly, get all of the tiles at current timing, and play them.
            selectedNotes = selectedTilesAtTiming(timingCount, allNoteMaps);
            foreach (NoteManager.Notes note in selectedNotes){
                contactSC(note);
            }

           
            yield return new WaitForSeconds(waitForSeconds);

        }
        
    }

    /// <summary>
    /// Method used to contact SuperCollider with a specific message to play a note.
    /// </summary>
    /// <param name="note"></param>
    public static void contactSC(NoteManager.Notes note)
    {
        List<string> args = new List<string>();
        args.Add("0.3f");
        args.Add(NoteManager.noteToFreq[note].ToString());
        args.Add("0.3f");

        //OSC Send
        OSCHandler.Instance.SendMessageToClient("SuperCollider", "/play" + "VoiceA", args);
      

    }


}
