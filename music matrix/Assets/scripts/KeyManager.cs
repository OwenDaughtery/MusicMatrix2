using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class KeyManager : MonoBehaviour { 
    //The range of possible keys that the system could be in
    private List<NoteManager.Notes> possibleKeys = new List<NoteManager.Notes>();
    //the circle of fifths that shows the notes in a scale for every key.
    private Dictionary<NoteManager.Notes, List<NoteManager.Notes>> circleOfFifths = new Dictionary<NoteManager.Notes, List<NoteManager.Notes>>();

    void Start(){
        //setting up possible keys and circleOfFifths.
        for (int i = 1; i <= 12; i++) {//c# might not be included here in later versions.
            possibleKeys.Add((NoteManager.Notes)i);
        }
        foreach (NoteManager.Notes key in possibleKeys){
            List<NoteManager.Notes> tempscale = TrackManager.generateScale(key, 0);
            List<NoteManager.Notes> scale = new List<NoteManager.Notes>();
            for (int i = 0; i <= 6; i++){
                scale.Add(MarkovManager.clampToBottomOctave(tempscale[i]));
            }
            circleOfFifths.Add(key, scale);
        }
    }

    void Update(){
    }

    /// <summary>
    /// Given a melody from a tilemap, try to predict what key its in given a already established list of possible keys.
    /// </summary>
    /// <param name="melody">A list of list of NoteManager enums representing all of the notes in a tilemap</param>
    /// <returns>A list of possible keys the user could be playing in.</returns>
    public NoteManager.Notes adaptkey(List<List<NoteManager.Notes>> melody) {
        List<NoteManager.Notes> uniqueNotes = new List<NoteManager.Notes>();
        Dictionary<NoteManager.Notes, int> keyDistances = new Dictionary<NoteManager.Notes, int>();
        foreach (KeyValuePair<NoteManager.Notes, List<NoteManager.Notes>> pair in circleOfFifths) {
            keyDistances.Add(pair.Key, 0);
        }
        keyDistances.Add(NoteManager.Notes.none, 4);//Adding none with an int of X means that keys must have a distance of X or more to be considered "the key"
        //List<NoteManager.Notes> selectionOfKeys = new List<NoteManager.Notes>(possibleKeys);
        //getting every unique note from the melody given by the user.
        foreach (List<NoteManager.Notes> column in melody) {
            foreach (NoteManager.Notes note in column){
                NoteManager.Notes clampedNote = MarkovManager.clampToBottomOctave(note);
                if (!uniqueNotes.Contains(clampedNote)) {
                    uniqueNotes.Add(clampedNote);
                }
            }
        }

        //for each unique note, check if it is in a scale from the possible keys.
        foreach (NoteManager.Notes uniqueNote in uniqueNotes) {
            foreach (KeyValuePair<NoteManager.Notes, List<NoteManager.Notes>> pair in circleOfFifths) {
                if (pair.Value.Contains(uniqueNote)) {
                    keyDistances[pair.Key] += 1;
                }
                
                /*if (possibleKeys.Contains(pair.Key)) {
                    //print("searching for " + uniqueNote + " in key " + pair.Key);
                    if (!pair.Value.Contains(uniqueNote)){
                        if (possibleKeys.Count > 1){
                            //remove the current looked at key from the possible keys.
                            possibleKeys.Remove(pair.Key);
                        }
                        else {
                            print("Only 1 key left. What to do?");
                            throw new System.Exception();
                        } 
                    }  
                }  
                */
            }
        }

        NoteManager.Notes chosenKey = NoteManager.Notes.none;
        foreach (KeyValuePair<NoteManager.Notes, int> pair in keyDistances) {
            if (pair.Value >= keyDistances[chosenKey]) {
                chosenKey = pair.Key;
            }
        }

        //if (possibleKeys.Count == 1){
        //    print("Predicted key is: " + possibleKeys[0]);
        //}
        print("predicted key is: " + chosenKey);
        return chosenKey;
    }
}
