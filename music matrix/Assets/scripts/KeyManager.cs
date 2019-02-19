using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class KeyManager : MonoBehaviour
{
    private NoteManager.Notes key;
    private List<List<NoteManager.Notes>> circleOfFifths;

    // Start is called before the first frame update
    void Start(){
        circleOfFifths = new List<List<NoteManager.Notes>>();
        List<NoteManager.Notes> keys = new List<NoteManager.Notes>();
        for (int i = 1; i <= 12; i++) {//should i only go up to 11 here? should c# be included?
            keys.Add((NoteManager.Notes)i);
        }
        foreach (NoteManager.Notes key in keys){
            List<NoteManager.Notes> tempscale = TrackManager.generateScale(key, 0);
            List<NoteManager.Notes> scale = new List<NoteManager.Notes>();
            for (int i = 0; i <= 6; i++){
                scale.Add(MarkovManager.clampToBottomOctave(tempscale[i]));
            }
            circleOfFifths.Add(scale);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public NoteManager.Notes getKey() {
        return key;
    }

    public NoteManager.Notes adaptkey(List<List<NoteManager.Notes>> melody) {
        print("entering adaptkey");
        List<NoteManager.Notes> uniqueNotes = new List<NoteManager.Notes>();
        List<List<NoteManager.Notes>> possibleKeys = new List<List<NoteManager.Notes>>(circleOfFifths);
        foreach (List<NoteManager.Notes> column in melody) {
            foreach (NoteManager.Notes note in column){
                NoteManager.Notes clampedNote = MarkovManager.clampToBottomOctave(note);
                if (!uniqueNotes.Contains(clampedNote)) {
                    uniqueNotes.Add(clampedNote);
                }
            }
        }
        print("count before entering: " + possibleKeys.Count);
        foreach (NoteManager.Notes uniqueNote in uniqueNotes) {
            foreach (List<NoteManager.Notes> possibleKey in circleOfFifths) {
                print("searching for " + uniqueNote + " in key " + possibleKey[0]);
                if (!possibleKey.Contains(uniqueNote) && possibleKeys.Count > 1)
                {
                    print("not there!");
        
                    possibleKeys.Remove(possibleKey);
                }
                
                print(possibleKeys.Count);
                if (possibleKeys.Count==1) {
                    print("found a key! its: " + possibleKeys[0][0]);
                }
            }
        }

        return NoteManager.Notes.none;
    }
}
