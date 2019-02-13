using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MarkovManager : MonoBehaviour {

    public List<MarkovChain> approvedChains = new List<MarkovChain>();
    public List<RhythmMarkovChain> approvedRhythmChains = new List<RhythmMarkovChain>();
    public MarkovChain tempChain = null;
    private int phase = 0;
    private int numberOfChainsToStore = 3;

    public MarkovChain chain;
    public RhythmMarkovChain rhythmChain;
    [SerializeField]
    public TrackManager trackManager;
    public static float incrementAmount = 0.01f; //how much a weight should be affected.
    public static int maximumRest = 15;
    //public static int maximumRest = 16;

    // Use this for initialization
    void Start() {
        chain = new MarkovChain(trackManager.getKey(), 0);
        rhythmChain = new RhythmMarkovChain();
        //rhythmChain.asString();
        //chain.asString();
    }

    // Update is called once per frame
    void Update() {

    }

    public MarkovChain getMarkovChain() {
        return chain;
    }

    public int getPhase(){
        return phase;
    }

    private void advancePhase(){
        print("ADVANCING PHASE");
        phase+=1;
    }

    public void approveMarkovChain(){
        if(phase==0){
            if(tempChain!=null){
                int currentID = chain.getID();
                approvedChains.Add(tempChain);
                tempChain=null;
                chain = new MarkovChain(trackManager.getKey(), currentID+1);
                if(approvedChains.Count==numberOfChainsToStore){
                    advancePhase();
                    chain = getNextChain(null);
                    List<MarkovChain> newApprovedStates = new List<MarkovChain>();
                    for (int i = 0; i < numberOfChainsToStore; i++){
                        newApprovedStates.Add(breedChains());
                    }
                    approvedChains = newApprovedStates;
                }

            }
        }else if(phase==1){
            
        }
        
        print("number of approved markov chains: " + approvedChains.Count);
    }

    public void disapproveMarkovChain(){
        if(phase==0){
            if(tempChain!=null){
                int currentID = chain.getID();
                tempChain=null;
                chain = new MarkovChain(trackManager.getKey(), currentID+1);
            }
        }
    }

    private MarkovChain getNextChain(MarkovChain currentChain){
        int currentIndex;
        if(currentChain == null){
            currentIndex = 0;
        }else{
            currentIndex = approvedChains.IndexOf(currentChain);
            currentIndex = (currentIndex+1) % approvedChains.Count;
        }
        print("getting markov chain at index " + currentIndex + " which has ID of " + approvedChains[currentIndex].getID());
        return approvedChains[currentIndex];
    }


    //
    public void influenceRhythmChain(Tilemap tilemap) {
        List<List<NoteManager.Notes>> melody = trackManager.getMelodyFromTilemap(tilemap);
        List<NoteManager.Notes> lastColumn = null; 
        int tempRest = 1;
        int lastRest =1;
        foreach (List<NoteManager.Notes> column in melody){
            if (column.Count != 0) {
                if (lastColumn != null)
                {
                    //print("Rhythm: incrementning weight from: " + lastRest + " to " + tempRest);
                   rhythmChain.incrementWeight(lastRest, tempRest);
 
                }
                lastColumn = column;
                lastRest = tempRest;
                tempRest=1;
            }else{
                tempRest+=1;
            }
            
        }
    }

        //
    public void influenceChain(Tilemap tilemap) {
        List<List<NoteManager.Notes>> melody = trackManager.getMelodyFromTilemap(tilemap);
        List<NoteManager.Notes> lastColumn = null; 
        foreach (List<NoteManager.Notes> column in melody){
            if (column.Count != 0) {
                if (lastColumn != null)
                {
                    foreach (NoteManager.Notes stateNote in lastColumn)
                    {
                        foreach (NoteManager.Notes transitionNote in column)
                        {
                            chain.incrementWeight(stateNote, transitionNote);
                        }
                    }
                }
                lastColumn = column;
            }
            
        }
    }

    //
    public void populateTrack(Tilemap tileMap, TileBase tileBase) {
        tileMap.ClearAllTiles();
        List<List<NoteManager.Notes>> melody = trackManager.getMelodyFromTilemap(trackManager.getTilemap());
        NoteManager.Notes mostCommonNote = calculateMostCommonNote(melody);
        if (mostCommonNote == NoteManager.Notes.none) {
            mostCommonNote = NoteManager.Notes.C2;
        }
        NoteManager.Notes nextNote = chain.getNextNote(mostCommonNote);
        
        int timing = rhythmChain.getNextRest(1); //arbritary 1, take care not to pass a number that isn't a key of the rhythm chain.
        int totalSoFar =0;
        
        do
        {
            totalSoFar += timing;
            int noteToHeight = (int)nextNote;
            Vector3Int posToAddTo = new Vector3Int(totalSoFar, noteToHeight - 1, 0);
            tileMap.SetTile(posToAddTo, tileBase);
            
            nextNote = chain.getNextNote(nextNote);
            timing = rhythmChain.getNextRest(timing);
            

        } while (totalSoFar+timing < TileManager.gridWidth-1);
        
        tempChain=chain;
        if(phase==1){
            chain = getNextChain(chain);
        }
    }

    private static NoteManager.Notes clampToBottomOctave(NoteManager.Notes note) {
        int noteAsInt = (int)note;
        noteAsInt -= 1;
        noteAsInt %= 12;
        noteAsInt += 1;
        
        return (NoteManager.Notes)noteAsInt;
    }

    private NoteManager.Notes calculateMostCommonNote(List<List<NoteManager.Notes>> melody) {
        Dictionary<NoteManager.Notes, int> mostCommonNotes = new Dictionary<NoteManager.Notes, int>();
        foreach (List<NoteManager.Notes> column in melody){
            foreach (NoteManager.Notes note in column){
                NoteManager.Notes clampedNote = clampToBottomOctave(note);
                if (!mostCommonNotes.ContainsKey(clampedNote)){
                    mostCommonNotes.Add(clampedNote, 1);
                }else {
                    mostCommonNotes[clampedNote] += 1;
                }
            }
        }
        NoteManager.Notes mostCommonNote = NoteManager.Notes.none;
        int numberOfOccurences = 0;
        foreach (KeyValuePair<NoteManager.Notes, int> pair in mostCommonNotes){
            if (pair.Value > numberOfOccurences) {
                mostCommonNote = pair.Key;
                numberOfOccurences = pair.Value;
            }
        }
        
        //print("most common note was: " + mostCommonNote);
        return mostCommonNote;
    }

    private MarkovChain breedChains(){
        List<MarkovChain> approvedChainsCopy = new List<MarkovChain>(approvedChains);

        int index = Random.Range(0,approvedChainsCopy.Count);
        MarkovChain chain1 = approvedChainsCopy[index];
        approvedChainsCopy.Remove(chain1);

        index = Random.Range(0,approvedChainsCopy.Count);
        MarkovChain chain2 = approvedChainsCopy[index];
        approvedChainsCopy.Remove(chain2);

        MarkovChain bredChain = new MarkovChain(trackManager.getKey(), -1);
        for (int i = 1; i <= 12; i++) {
            NoteManager.Notes note = (NoteManager.Notes)i;
            if(Random.Range(0,2) == 0){
                bredChain.replaceState(note, chain1.getState(note));
            }else{
                bredChain.replaceState(note, chain2.getState(note));
            }
        }
        if(Random.Range(0,10)!=0){//10% chance to mutate
            mutateChain(bredChain);
        }
        return bredChain;
        
    }

    private void mutateChain(MarkovChain chain){
        int indexForNote1 = Random.Range(1, 13);
        int indexForNote2 = Random.Range(1,13);
        print("mutating: " + (NoteManager.Notes)indexForNote1 + " to " + (NoteManager.Notes)indexForNote2);
        chain.incrementWeight((NoteManager.Notes)indexForNote1,(NoteManager.Notes)indexForNote2);
    }

    //markov chain class
    public class MarkovChain {
        int ID;
        int fitnessScore = 5;
        NoteManager.Notes key;
        Dictionary<NoteManager.Notes, MarkovState> chain = new Dictionary<NoteManager.Notes, MarkovState>();

        public MarkovChain(NoteManager.Notes newKey, int ID) {
            this.ID=ID;
            key = newKey;
            setUpChain();
        }

        private void setUpChain() {
            List<NoteManager.Notes> inScaleKeys = TrackManager.generateScale(key, 0);
            for (int i = 1; i <= 12; i++) {
                NoteManager.Notes noteToAdd = (NoteManager.Notes)i;
                chain.Add(noteToAdd, new MarkovState(noteToAdd, inScaleKeys));
            }
        }

        public MarkovState getState(NoteManager.Notes note){
            return chain[note];
        }

        public void replaceState(NoteManager.Notes note, MarkovState newState){
            chain[note]=newState;
        }

        public NoteManager.Notes getKey(){
            return key;
        }

        public int getID(){
            return ID;
        }

        public void resetChain(){
            chain = new Dictionary<NoteManager.Notes, MarkovState>();
            setUpChain();

        }

        public void incrementFitnessScore(){
            if(fitnessScore<10){
                fitnessScore++;
            }
        }

        public void decrementFitnessScore(){
            if(fitnessScore>0){
                fitnessScore--;
            }
        }

        public NoteManager.Notes getNextNote(NoteManager.Notes currentNote) {
            return chain[currentNote].getNextNote();
        }

        public void asString() {
            foreach (KeyValuePair<NoteManager.Notes, MarkovState> pair in chain) {
                pair.Value.showTransitions();
                pair.Value.sumProbs();
            }
        }

        private void wipeChain() {
            foreach (KeyValuePair<NoteManager.Notes, MarkovState> pair in chain){
                pair.Value.setUpTransitions();
            }
            
        }

        public void incrementWeight(NoteManager.Notes stateNote, NoteManager.Notes transitionNote) {
            float percentage = MarkovManager.incrementAmount;
            //print("incrementing transition from " + stateNote + " to " + transitionNote + " by a percentage of " + percentage);
            stateNote = MarkovManager.clampToBottomOctave(stateNote);
            transitionNote = MarkovManager.clampToBottomOctave(transitionNote);
            MarkovState state = chain[stateNote];
            float changedBy = state.incrementTransition(transitionNote, percentage);
            state.decrementAllTransitions(transitionNote, changedBy);
            print("=========");
            //state.asString();
            
        }

    }

    //Markov State Class:
    public class MarkovState {
        List<NoteManager.Notes> inScaleKeys;
        [SerializeField]
        private NoteManager.Notes state;
        private Dictionary<NoteManager.Notes, float> transitions;
        private float maximumWeight;
        private float minimumWeight;

        public MarkovState(NoteManager.Notes newState, List<NoteManager.Notes> newInScaleKeys) {
            maximumWeight= 0.56f;
            minimumWeight =  (1f-maximumWeight)/11f;
            inScaleKeys = newInScaleKeys;
            state = newState;
            transitions = setUpTransitions();
            
        }

        public float getTransition(NoteManager.Notes note){
            return transitions[note];
        }

        public void updateTransition(NoteManager.Notes note, float newValue) {
            transitions[note] = newValue;
        }

        public NoteManager.Notes getNextNote() {
            float randomValue = Random.value;
            float totalSoFar = 0.0f;
            NoteManager.Notes noteToReturn = NoteManager.Notes.none;

            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitions) {
                totalSoFar += pair.Value;
                if (randomValue <= totalSoFar)
                {
                    noteToReturn = pair.Key;
                    break;
                }

            }
            return noteToReturn;
        }

        //returns the increase from the old value to the new value.
        public float incrementTransition(NoteManager.Notes note, float percentage) {
            float changeBy = percentage;
            float oldValue = transitions[note];
            float newValue = oldValue + changeBy;
            if(newValue>maximumWeight){
                newValue=maximumWeight;
                changeBy = newValue-oldValue;
            }
            updateTransition(note, newValue);
            return changeBy;
        }

        public void decrementAllTransitions(NoteManager.Notes noteToIgnore, float changedBy){
            float leftOver = 0f;
            List<NoteManager.Notes> notesToIgnore = new List<NoteManager.Notes>();
            notesToIgnore.Add(noteToIgnore);
            float minusBy = changedBy / 11f;
            //print("change by after being split 11 ways: " + minusBy);
            Dictionary<NoteManager.Notes, float> transitionsCopy = new Dictionary<NoteManager.Notes, float>(transitions);
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitionsCopy){
                
                if (pair.Key != noteToIgnore) {
                    float oldValue = pair.Value;
                    float newValue = oldValue-minusBy;
                    if(newValue<minimumWeight){
                        notesToIgnore.Add(pair.Key);
                        newValue=minimumWeight;
                        leftOver += minusBy-(oldValue-newValue);
                    }
                    //print("going from " + oldValue + " to " + newValue);
                    updateTransition(pair.Key, newValue);
                }
            }
            if(leftOver>0){
                deductLeftovers(notesToIgnore, leftOver);
            }
        }

        public void deductLeftovers(List<NoteManager.Notes> exclude, float passedLeftOver){
            float leftOver = 0;
            int numOfRemainingNotes =0;
            Dictionary<NoteManager.Notes, float> transitionsCopy = new Dictionary<NoteManager.Notes, float>(transitions);
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitionsCopy){
                if(!exclude.Contains(pair.Key)){
                    numOfRemainingNotes+=1;
                }
            }
            if(numOfRemainingNotes!=0){
        
                float minusBy = passedLeftOver / numOfRemainingNotes;

                foreach (KeyValuePair<NoteManager.Notes, float> pair in transitionsCopy){
                    
                    if (!exclude.Contains(pair.Key)) {
                        float oldValue = pair.Value;
                        float newValue = oldValue-minusBy;
                        if(newValue<minimumWeight){
                            exclude.Add(pair.Key);
                            newValue=minimumWeight;
                            leftOver += minusBy-(oldValue-newValue);
                        }
                        //print("going from " + oldValue + " to " + newValue);
                        updateTransition(pair.Key, newValue);
                    }
                }
                if(leftOver>0){
                    deductLeftovers(exclude, leftOver);
                }
              }
        }

        public Dictionary<NoteManager.Notes, float> setUpTransitions() {
            transitions = new Dictionary<NoteManager.Notes, float>();
            float inScaleWeight = 0.125f;
            float notInScaleWeight = (1 - (7 * 0.125f)) / 5;

            for (int i = 1; i <= 12; i++) {
                NoteManager.Notes noteToAdd = (NoteManager.Notes)i;
                if (inScaleKeys.Contains(noteToAdd)) {
                    transitions.Add(noteToAdd, 0.125f);
                } else {
                    transitions.Add(noteToAdd, notInScaleWeight);
                }
            }

            return transitions;
        }

        public void showTransitions() {
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitions) {
                print("from " + state + " to " + pair.Key + " has a prob of: " + pair.Value);
            }
        }

        public void sumProbs() {
            float total = 0;
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitions) {
                total += pair.Value;
            }
            print("total weighting from state " + state + " is: " + total);
        }

    }

    //=====RHYTHM STARTS HERE=====//

    public class RhythmMarkovChain{
        Dictionary<int, RhythmMarkovState> chain = new Dictionary<int, RhythmMarkovState>();

        public RhythmMarkovChain(){
            setUpChain();
        }

        public void incrementWeight(int stateRest, int transitionRest) {
            bool debug = false;
            float percentage = MarkovManager.incrementAmount;
         
            
            //print("incrementing transition from " + stateRest + " to " + transitionRest + " by a percentage of " + percentage);
            RhythmMarkovState state = chain[stateRest];
            if(debug){
                print("before:");
                state.sumProbs();
                state.showTransitions();
            }
            float changedBy = state.incrementTransition(transitionRest, percentage);
            state.decrementAllTransitions(transitionRest, changedBy);
            if(debug){
                print("=!=!=!=!=!=!=!=!=");
                state.showTransitions();
                state.sumProbs();
            }
            
            
        }

        private void setUpChain(){
            for (int i = 0; i <= MarkovManager.maximumRest; i++){//SPECIFIES RANGE OF POSSIBLE RESTS, MAKE SURE IT REFLECTS SAME IN RHYTHM STATE
                chain.Add(i, new RhythmMarkovState(i));
            }
        }

        public int getNextRest(int currentRest){
            return chain[currentRest].getNextRest();
        }

        

        public void asString()
        {
            foreach (KeyValuePair<int, RhythmMarkovState> pair in chain)
            {
                pair.Value.sumProbs();
            }
        }

    }

    public class RhythmMarkovState{
        [SerializeField]
        private int state;
        private Dictionary<int, float> transitions = new Dictionary<int, float>();
        private float maximumWeight;
        private float minimumWeight;
        int numberOfLowerRests = Mathf.FloorToInt((maximumRest+1)/2);
        int numberOfHigherRests = Mathf.CeilToInt((maximumRest+1)/2);
        float lowerRhythmWeight;
        float higherRhythmWeight;
        


        public RhythmMarkovState(int newState){
            lowerRhythmWeight = (0.75f)/numberOfLowerRests;
            higherRhythmWeight = (0.25f)/numberOfHigherRests;
            //print((1 - (Mathf.FloorToInt((maximumRest+1)/2) * 0.125f)) / (Mathf.CeilToInt((maximumRest+1)/2)));
            maximumWeight = 0.75f;
            minimumWeight = (1-maximumWeight)/maximumRest;
            state = newState;
            setUpTransitions();
        }

        public int getNextRest()
        {
            float randomValue = Random.value;
            float totalSoFar = 0.0f;
            int intToReturn = -1;

            foreach (KeyValuePair<int, float> pair in transitions)
            {
                totalSoFar += pair.Value;
                if (randomValue <= totalSoFar)
                {
                    intToReturn = pair.Key;
                    break;
                }

            }
            return intToReturn;
        }

        //returns the increase from the old value to the new value.
        public float incrementTransition(int rest, float percentage) {
            float changeBy = percentage;
            float oldValue = transitions[rest];
            float newValue = oldValue + changeBy;
            if(newValue>maximumWeight){
                newValue=maximumWeight;
                changeBy = newValue-oldValue;
            }
            updateTransition(rest, newValue);
            return changeBy;
        }

        public void decrementAllTransitions(int restToIgnore, float changedBy){
            float leftOver = 0f;
            List<int> restsToIgnore = new List<int>();
            restsToIgnore.Add(restToIgnore);
            float minusBy = changedBy / (maximumRest);//
            //print("Rhythm: change by after being split " + (maximumRest) + " ways: " + minusBy);
            Dictionary<int, float> transitionsCopy = new Dictionary<int, float>(transitions);
            foreach (KeyValuePair<int, float> pair in transitionsCopy){
                
                if (pair.Key != restToIgnore) {
                    float oldValue = pair.Value;
                    float newValue = oldValue-minusBy;
                    if(newValue<minimumWeight){
                        restsToIgnore.Add(pair.Key);
                        newValue=minimumWeight;
                        leftOver += minusBy-(oldValue-newValue);
                    }
                    //print("going from " + oldValue + " to " + newValue);
                    updateTransition(pair.Key, newValue);
                }
            }
            if(leftOver>0){
                deductLeftovers(restsToIgnore, leftOver);
            }
        }

        public void deductLeftovers(List<int> exclude, float passedLeftOver){
            float leftOver = 0;
            int numOfRemainingRests =0;
            Dictionary<int, float> transitionsCopy = new Dictionary<int, float>(transitions);
            foreach (KeyValuePair<int, float> pair in transitionsCopy){
                if(!exclude.Contains(pair.Key)){
                    numOfRemainingRests+=1;
                }
            }
            if(numOfRemainingRests!=0){
        
                float minusBy = passedLeftOver / numOfRemainingRests;

                foreach (KeyValuePair<int, float> pair in transitionsCopy){
                    
                    if (!exclude.Contains(pair.Key)) {
                        float oldValue = pair.Value;
                        float newValue = oldValue-minusBy;
                        if(newValue<minimumWeight){
                            exclude.Add(pair.Key);
                            newValue=minimumWeight;
                            leftOver += minusBy-(oldValue-newValue);
                        }
                        //print("going from " + oldValue + " to " + newValue);
                        updateTransition(pair.Key, newValue);
                    }
                }
                if(leftOver>0){
                    deductLeftovers(exclude, leftOver);
                }
              }
        }        

        public float getTransition(int rest){
            return transitions[rest];
        }

        public void updateTransition(int rest, float newValue) {
            transitions[rest] = newValue;
        }

        private void setUpTransitions(){
            float weight = 1f / (float)(maximumRest+1);//+1
            for (int i = 0; i <= MarkovManager.maximumRest; i++){//MUST REFLECT SAME IN RHYTHM MARKOV CHAIN
                if(i<numberOfLowerRests){
                    transitions.Add(i, lowerRhythmWeight);
                }else{
                    transitions.Add(i, higherRhythmWeight);
                }
            }
        }

        public void showTransitions() {
            foreach (KeyValuePair<int, float> pair in transitions) {
                print("Rhythm: from " + state + " to " + pair.Key + " has a prob of: " + pair.Value);
            }
        }

        public void sumProbs()
        {
            float total = 0;
            foreach (KeyValuePair<int, float> pair in transitions)
            {
                total += pair.Value;
            }
            print("Rhythm: total weighting from state " + state + " is: " + total);
        }
    }
}
