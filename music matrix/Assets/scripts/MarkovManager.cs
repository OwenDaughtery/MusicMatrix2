using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MarkovManager : MonoBehaviour {

    //A list of all the approved rhythm and markov chains as pairs.
    public List<MarkovPair> approvedPairs = new List<MarkovPair>();
    //variable to temporally hold a markov pair.
    public MarkovPair tempPair = null;

    //what phase the system is currently in.
    //-1 is key detection, 0 is learning, 1 is breeding,
    private int phase = -1;

    //The number of pairs to store before the system enters the breeding phase.
    private int numberOfPairsToStore = 4;

    //The current looked at markov pair.
    public MarkovPair markovPair;

    [SerializeField]
    public TrackManager trackManager;

    //how much a weight should be affected. (NOTE: THIS WILL BE UPDATED TO BE DYNAMIC IN FUTURE VERSION)
    public static float incrementAmount = 0.1f;
    
    //The maximum amount of rests between 2 notes that can happen.
    public static int maximumRest = 15;

    void Start() {
    }

    void Update() {
    }

    public void startMarkovManaging() {
        markovPair = new MarkovPair(new MarkovChain(trackManager.getKey(), 0), new RhythmMarkovChain());
    }

    /// <summary>
    /// Getter to get current markov chain from markov pair.
    /// </summary>
    /// <returns>Current markov chain</returns>
    public MarkovChain getMarkovChain() {
        return markovPair.getMarkovChain();
    }

    /// <summary>
    /// Getter to get current rhythm markov chain from markov pair.
    /// </summary>
    /// <returns>Current rhythm markov chain</returns>
    public RhythmMarkovChain getRhythmMarkovChain(){
        return markovPair.getRhythmMarkovChain();
    }

    /// <summary>
    /// Function that returns what phase the system is currently in.
    /// </summary>
    /// <returns>int of what system is currently in.</returns>
    public int getPhase(){
        return phase;
    }

    /// <summary>
    /// Increment phase variable.
    /// </summary>
    public void advancePhase(){
        print("ADVANCING PHASE");
        phase+=1;
        if (phase == 0) {
            print("Advance Phase: Entering Learning Phase.");
            startMarkovManaging();
        } else if (phase == 1) {
            print("Advance Phase: Entering Breeding Phase");
        }
    }

    /// <summary>
    /// Function that is called from a button press, that approves the current markov pair.
    /// </summary>
    public void approveMarkovPair(){
        //if in learning phase:
        if (phase == -1) {

        }else if(phase==0){
            //If there is a markov pair to approve.
            if(tempPair!=null){
                //Get id from markov chain.
                int currentID = markovPair.getMarkovChain().getID();
                //add markov pair to list of approved pairs.
                approvedPairs.Add(tempPair);
                //reset the tempPair variable to protect against user approving the same markov pair twice.
                tempPair=null;
                //create a new markov pair with a higher id than the previous one just saved.
                markovPair = new MarkovPair(new MarkovChain(trackManager.getKey(), currentID+1), new RhythmMarkovChain());

                //If statement entered if enough markovPairs have been stored.
                if(approvedPairs.Count>=numberOfPairsToStore){
                    advancePhase();
                    //the markov pair should now be set to the first markov pair that was saved using the getNextPair method.
                    markovPair = getNextPair(null);

                    //create a new list of approved pairs, and populate it with breeding the pairs together.
                    List<MarkovPair> newApprovedPairs = new List<MarkovPair>();
                    for (int i = 0; i < numberOfPairsToStore; i++){
                        newApprovedPairs.Add(breedPairs());
                    }
                    approvedPairs = newApprovedPairs;
                }

            }
        //if in breeding phase:
        }else if(phase==1){
            
        }
        
        print("number of approved markov chains: " + approvedPairs.Count);
    }

    /// <summary>
    /// Method used to disapprove of a markov pair. Simply creates a brand new markov chain and trashes the current one.
    /// </summary>
    public void disapproveMarkovPair(){
        if (phase == -1){

        }else if (phase == 0){
            if (tempPair != null)
            {
                int currentID = markovPair.getMarkovChain().getID();
                tempPair = null;
                markovPair = new MarkovPair(new MarkovChain(trackManager.getKey(), currentID + 1), new RhythmMarkovChain());
            }
        }else if (phase == 1) {

        }
    }

    /// <summary>
    /// Given a markov pair, get the next in the list of approved pairs.
    /// </summary>
    /// <param name="currentPair">The currently used markovpair, if null is passed then the method will return the first markov pair in the list.</param>
    /// <returns></returns>
    private MarkovPair getNextPair(MarkovPair currentPair){
        int currentIndex;
        if(currentPair == null){
            currentIndex = 0;
        }else{
            currentIndex = approvedPairs.IndexOf(currentPair);
            currentIndex = (currentIndex+1) % approvedPairs.Count;
        }
        print("getting markov chain at index " + currentIndex + " which has ID of " + approvedPairs[currentIndex].getMarkovChain().getID());
        return approvedPairs[currentIndex];
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="predictedKeys"></param>
    public void weightInKeyNotes(List<NoteManager.Notes> predictedKeys) {

    }

    /// <summary>
    /// Change the weights of the transitions for the current rhythm markov chain.
    /// </summary>
    /// <param name="tilemap">The tilemap that the rhythm markov chain will be influenced by.</param>
    public void influenceRhythmChain(Tilemap tilemap) {
        RhythmMarkovChain rhythmChain = markovPair.getRhythmMarkovChain();
        List<List<NoteManager.Notes>> melody = trackManager.getMelodyFromTilemap(tilemap);
        List<NoteManager.Notes> lastColumn = null; 
        int tempRest = 1;
        int lastRest =1;
        foreach (List<NoteManager.Notes> column in melody){
            //if there are notes in the current column
            if (column.Count != 0) {
                //and a column with notes has already been encountered.
                if (lastColumn != null){
                    //print("Rhythm: incrementning weight from: " + lastRest + " to " + tempRest);
                    //increment the weight from state of lastrest to the temprest.
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

    /// <summary>
    /// Change the weights of transitions for the current note markov chain.
    /// </summary>
    /// <param name="tilemap">The tilemap that the markov chain will be influenced by.</param>
    public void influenceChain(Tilemap tilemap) {
        MarkovChain chain = markovPair.getMarkovChain();
        List<List<NoteManager.Notes>> melody = trackManager.getMelodyFromTilemap(tilemap);
        List<NoteManager.Notes> lastColumn = null; 
        foreach (List<NoteManager.Notes> column in melody){
            if (column.Count != 0) {
                if (lastColumn != null){
                    //for each note in the last encountered column to the currenyl looked at column, increment the weight of the transition.
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

    /// <summary>
    /// Using the current markov pair, add new notes to the track by activating tiles.
    /// </summary>
    /// <param name="tileMap">the tilemap that the method should populate</param>
    /// <param name="tileBase"></param>
    public void populateTrack(Tilemap tileMap, TileBase tileBase) {
        print("debug id going in");
        //erase previous notes from tilemap
        tileMap.ClearAllTiles();
        List<List<NoteManager.Notes>> melody = trackManager.getMelodyFromTilemap(trackManager.getTilemap());
        //get the most common note to start with.
        NoteManager.Notes mostCommonNote = calculateMostCommonNote(melody);
        if (mostCommonNote == NoteManager.Notes.none) {
            mostCommonNote = NoteManager.Notes.C2;
        }

        //get the current markov chain from markov pair, and get the next note from the most common note.
        NoteManager.Notes nextNote = markovPair.getMarkovChain().getNextNote(mostCommonNote);
        
        //get the rhythm markov chain from markov pair, and get the next timing for an artbritary rest value (NOTE: FUTURE VERSIONS WILL USE THE FIRST AMOUNT OF RESTS IN THE TRACKMANAGER.TILEMAP)
        int timing = markovPair.getRhythmMarkovChain().getNextRest(1); //arbritary 1, take care not to pass a number that isn't a key of the rhythm chain.
        //variable to ensure no tiles get activated over the edge of the tilemap.
        int totalSoFar =0;
        do{
            totalSoFar += timing;
            //get the height of the current note. (Note: must be reduced by 1 due to the dummy enum in NoteManager.Notes)
            int noteToHeight = ((int)nextNote) - 1;
            Vector3Int posToAddTo = new Vector3Int(totalSoFar, noteToHeight, 0);
            //activate tile at certain position.
            tileMap.SetTile(posToAddTo, tileBase);
            
            //get next note and timing.
            nextNote = markovPair.getMarkovChain().getNextNote(nextNote);
            timing = markovPair.getRhythmMarkovChain().getNextRest(timing);
            
            //as long as the next x position a tile will be activated at is within the gridwith, do loop again.
        } while (totalSoFar+timing < TileManager.gridWidth-1);
        
        //after populating, save the markpov pair responsible for populating in temppair.
        tempPair=markovPair;
        if(phase==1){
            markovPair = getNextPair(markovPair);
        }
        print("debug id coming out");
    }

    /// <summary>
    /// Given a note, get the lowest octave that note can be.
    /// </summary>
    /// <param name="note">a NoteManager enum to clamp</param>
    /// <returns>The given note reduced to X2, where X is the given note and 2 represents the lowest octave that note can go in the given NoteManager enums.</returns>
    public static NoteManager.Notes clampToBottomOctave(NoteManager.Notes note) {
        int noteAsInt = (int)note;
        noteAsInt -= 1;
        noteAsInt %= 12;
        noteAsInt += 1;
        
        return (NoteManager.Notes)noteAsInt;
    }


    /// <summary>
    /// Given a melody, caluclate the most common note.
    /// </summary>
    /// <param name="melody">List of list of NoteManager enums representing a melody of notes.</param>
    /// <returns>NoteManager enum of the most common note in the melody.</returns>
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


    /// <summary>
    /// Function that uses 2 randomly selected markovpairs from the list of approved pairs, breeds them together, mutates the child (possibly), and returns it.
    /// </summary>
    /// <returns>A new markov pair which is a child of 2 markov pairs in the approvedpairs list.</returns>
    private MarkovPair breedPairs(){
        List<MarkovPair> approvedPairsCopy = new List<MarkovPair>(approvedPairs);

        //get random markov pair from approvedPairsCopy list and remove it to ensure same markov pair isn't chosen twice.
        int index = Random.Range(0,approvedPairsCopy.Count);
        MarkovPair pair1 = approvedPairsCopy[index];
        MarkovChain chain1 = pair1.getMarkovChain();
        RhythmMarkovChain rhythmChain1 = pair1.getRhythmMarkovChain();
        approvedPairsCopy.Remove(pair1);

        //get another random markov pair from approvedPairsCopy list.
        index = Random.Range(0,approvedPairsCopy.Count);
        MarkovPair pair2 = approvedPairsCopy[index];
        MarkovChain chain2 = pair2.getMarkovChain();
        RhythmMarkovChain rhythmChain2 = pair2.getRhythmMarkovChain();
        approvedPairsCopy.Remove(pair2);

        //create a new markov chain whose states will be replaced by states from the chosen markov pairs.
        MarkovChain bredChain = new MarkovChain(trackManager.getKey(), -1);
        for (int i = 1; i <= 12; i++) {
            NoteManager.Notes note = (NoteManager.Notes)i;
            //randomly choose between both chains, and replace the new chains states with one of the selected parent chains'.
            if(Random.Range(0,2) == 0){
                bredChain.replaceState(note, chain1.getState(note));
            }else{
                bredChain.replaceState(note, chain2.getState(note));
            }
        }


        RhythmMarkovChain bredRhythmChain = new RhythmMarkovChain();
        for (int i = 1; i <= maximumRest; i++){
            NoteManager.Notes note = (NoteManager.Notes)i;
            //randomly choose between both chains, and replace the new chains states with one of the selected parent chains'.
            if (Random.Range(0, 2) == 0){
                bredRhythmChain.replaceState(i, rhythmChain1.getState(i));
            }
            else
            {
                bredRhythmChain.replaceState(i, rhythmChain2.getState(i));
            }
        }

        //enter if statement X% of the time and mutate the bred chain for variety.
        if (Random.Range(0,10)!=0){//10% chance to mutate
            mutateChain(bredChain);
        }

        //make a new markov pair with the 2 new created chains, and return it.
        MarkovPair newMarkovPair = new MarkovPair(bredChain, new RhythmMarkovChain());
        return newMarkovPair;
        
    }

    /// <summary>
    /// Method that randomly increments the weight of 1 transition from 1 state to another in a given markov chain.
    /// </summary>
    /// <param name="chain">the markov chain to mutate.</param>
    private void mutateChain(MarkovChain chain){
        int indexForNote1 = Random.Range(1, 13);
        int indexForNote2 = Random.Range(1,13);
        //print("mutating: " + (NoteManager.Notes)indexForNote1 + " to " + (NoteManager.Notes)indexForNote2);
        chain.incrementWeight((NoteManager.Notes)indexForNote1,(NoteManager.Notes)indexForNote2);
    }


    /// <summary>
    /// Subclass of MarkovManager, the purpose of which is to simple hold a markov chain and rhythm markov chain.
    /// </summary>
    public class MarkovPair {
        MarkovChain markovChain;
        RhythmMarkovChain rhythmMarkovChain;


        public MarkovPair(MarkovChain chain, RhythmMarkovChain rhythmChain){
            markovChain = chain;
            rhythmMarkovChain = rhythmChain;
        }

        /// <summary>
        /// Getter of markov chain.
        /// </summary>
        /// <returns>MarkovChain of class.</returns>
        public MarkovChain getMarkovChain(){
            return markovChain;
        }

        /// <summary>
        /// Getter of rhythm markov chain.
        /// </summary>
        /// <returns>RhythmMarkovChain of class</returns>
        public RhythmMarkovChain getRhythmMarkovChain(){
            return rhythmMarkovChain;
        }

        /// <summary>
        /// Set the current markov chain equal to a different markov chain.
        /// </summary>
        /// <param name="chain">The new markov chain to set the class variable too.</param>
        public void setMarkovChain(MarkovChain chain){
            markovChain = chain;
        }

        /// <summary>
        /// Set the current rhythm markov chain equal to a different rhythm markov chain.
        /// </summary>
        /// <param name="rhythmChain">The new rhythm markov chain to set the class variable too.</param>
        public void setRhythmMarkovChain(RhythmMarkovChain rhythmChain){
            rhythmMarkovChain = rhythmChain;
        }

    }

    /// <summary>
    /// Subclass of MarkovManager, the purpose of which is the represent the idea of a markov chain.
    /// The main pair of a MarkovChain is to hold MarkovStates. And to handle the incrementing and decrementing of weights between these states.
    /// </summary>
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

        /// <summary>
        /// Function that creates 12 markov states, and stores them in its dictionary under the Key of the note of that state.
        /// </summary>
        private void setUpChain() {
            List<NoteManager.Notes> inScaleKeys = TrackManager.generateScale(key, 0);
            for (int i = 1; i <= 12; i++) {
                NoteManager.Notes noteToAdd = (NoteManager.Notes)i;
                chain.Add(noteToAdd, new MarkovState(noteToAdd, inScaleKeys));
            }
        }

        /// <summary>
        /// Given a certain note, return the MarkovState of that note.
        /// </summary>
        /// <param name="note">A NoteManager enum of the desired MarkovState.</param>
        /// <returns>A MarkovState which represents the transitions from the given NoteManager enum to all other NoteManager enums.</returns>
        public MarkovState getState(NoteManager.Notes note){
            return chain[note];
        }

        /// <summary>
        /// Replace the state of a chain with a new given MarkovState.
        /// </summary>
        /// <param name="note">The NoteManager enum representing the key of the MarkovState to replace</param>
        /// <param name="newState">The new markov state to be inserted into the markovChain.</param>
        public void replaceState(NoteManager.Notes note, MarkovState newState){
            chain[note]=newState;
        }

        /// <summary>
        /// Getter of the key of this markov chain.
        /// </summary>
        /// <returns>NoteManager enum of the current key of this markov chain.</returns>
        public NoteManager.Notes getKey(){
            return key;
        }

        /// <summary>
        /// Getter of ID of this markov chain.
        /// </summary>
        /// <returns>int of the ID of this markov chain.</returns>
        public int getID(){
            return ID;
        }

        /// <summary>
        /// public method to reset the chain variable and recall setUpChain to initalise it properly.
        /// </summary>
        public void resetChain(){
            chain = new Dictionary<NoteManager.Notes, MarkovState>();
            setUpChain();

        }

        /// <summary>
        /// Function to increment the fitness of the current markov chain, up to but not past 10.
        /// </summary>
        public void incrementFitnessScore(){
            if(fitnessScore<10){
                fitnessScore++;
            }
        }

        /// <summary>
        /// Function to decrement the fitness of the current markov chain,  down to but not past 0.
        /// </summary>
        public void decrementFitnessScore(){
            if(fitnessScore>0){
                fitnessScore--;
            }
        }

        /// <summary>
        /// Given a NoteManager enum, call the getNextNote() method of the MarkovState at that enum.
        /// </summary>
        /// <param name="currentNote">NoteManager enum of required MarkovState.</param>
        /// <returns>NoteManager enum of a note given by the indexed MarkovState.</returns>
        public NoteManager.Notes getNextNote(NoteManager.Notes currentNote) {
            return chain[currentNote].getNextNote();
        }

        /// <summary>
        /// For each state in the chain, print out all of it's transition weights and the sum of those transition weights.
        /// </summary>
        public void asString() {
            foreach (KeyValuePair<NoteManager.Notes, MarkovState> pair in chain) {
                pair.Value.showTransitions();
                pair.Value.sumProbs();
            }
        }

        /// <summary>
        /// Increment the weight of the markov state at a given NoteManager enum index, to a given NoteManager enum.
        /// </summary>
        /// <param name="stateNote">The state of the MarkovState to increment.</param>
        /// <param name="transitionNote">The note of the MarkovState to increment the transition to of.</param>
        /// NOTE: MAY BE REVISED IN LATER VERSIONS TO NOT BE INCREMENTED BY FIXED AMOUNT.
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

    /// <summary>
    /// Subclass of MarkovManager, to represent a single MarkovState that will be held in a MarkovChain.
    /// The purpose of a MarkovState is to hold all of the transitions from itself to all other notes in the system. The sum of those transitions should always be exactly 1.
    /// </summary>
    public class MarkovState {
        //A list of NoteManager enums that represents notes that are in scale from this particular state.
        List<NoteManager.Notes> inScaleKeys;
        //The core state of this MarkovState.
        private NoteManager.Notes state;
        //A dictionary holding all of the transitions from this state to all the other NoteManager enums in the system.
        private Dictionary<NoteManager.Notes, float> transitions;
        //The maximum and minimum a transition can be at any one time.
        private float maximumWeight;
        private float minimumWeight;

        public MarkovState(NoteManager.Notes newState, List<NoteManager.Notes> newInScaleKeys) {
            maximumWeight= 0.56f;
            minimumWeight =  (1f-maximumWeight)/11f;
            inScaleKeys = newInScaleKeys;
            state = newState;
            transitions = setUpTransitions();
        }

        /// <summary>
        /// Given a NoteManager enum, return the appropriate transition weight from the dictionary.
        /// </summary>
        /// <param name="note">A NoteManager enum that will be used to access the dictionary of the MarkovState.</param>
        /// <returns>A float representing the transition weight of the MarkovState to the given NoteManager enum.</returns>
        public float getTransition(NoteManager.Notes note){
            return transitions[note];
        }

        /// <summary>
        /// Function to update a transition value of the markov state.
        /// </summary>
        /// <param name="note">The NoteManager enum whose's transition to will be updated too.</param>
        /// <param name="newValue">The value to change the transition too.</param>
        public void updateTransition(NoteManager.Notes note, float newValue) {
            transitions[note] = newValue;
        }

        /// <summary>
        /// Function that generates a random value between 0-1. And will go through all of the transitions until it finds one
        /// where the value falls between the sum of all the transitions seen so far and the sum of all the transitions seen so far + the next transition.
        /// </summary>
        /// <returns>A NoteManager enum representing the randomly selected note from this MarkovState</returns>
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


        /// <summary>
        /// Function used to increment the transition in the MarkovStates dictionary.
        /// </summary>
        /// <param name="note">The note of the transition to increment.</param>
        /// <param name="percentage">The amount to increment the transition.</param>
        /// <returns>How much the transition was incremented by. (Will always be somewhere between the passed float "percentage", and 0.
        /// Due to the maximumWeight variable to take into account.)</returns>
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

        /// <summary>
        /// Decrement all transitions in the MarkovState by a certain amount, excluding the passed parameter.
        /// </summary>
        /// <param name="noteToIgnore">The NoteManager enum that was just incremented, and so wants to be avoided when decrementing.</param>
        /// <param name="changedBy">The amount that a transition was just incremented by, this will be divded evenly amoungst the other transitions.</param>
        public void decrementAllTransitions(NoteManager.Notes noteToIgnore, float changedBy){
            float leftOver = 0f;
            List<NoteManager.Notes> notesToIgnore = new List<NoteManager.Notes>();
            notesToIgnore.Add(noteToIgnore);
            //divide the amount to change by by 11, to split it evenly between the remaining transitions.
            float minusBy = changedBy / 11f;
            Dictionary<NoteManager.Notes, float> transitionsCopy = new Dictionary<NoteManager.Notes, float>(transitions);
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitionsCopy){
                //If current transition is one to ignore don't enter if statement.
                if (pair.Key != noteToIgnore) {
                    float oldValue = pair.Value;
                    float newValue = oldValue-minusBy;
                    if(newValue<minimumWeight){
                        //if statement that is entered if the current transition has reached minimum.
                        //It is added to the list of notes to ignore,
                        notesToIgnore.Add(pair.Key);
                        //it's value is set to the minimum
                        newValue=minimumWeight;
                        //and the amount leftover from decrementing it as far as possible is recorded.
                        leftOver += minusBy-(oldValue-newValue);
                    }
                    updateTransition(pair.Key, newValue);
                }
            }
            
            //If any transitions hit minimum, the amount that was leftover needs to be divided between the remaining transitions.
            if(leftOver>0){
                deductLeftovers(notesToIgnore, leftOver);
            }
        }

        /// <summary>
        /// Function to deduct any leftover value from the function "decrementAllTransitions".
        /// </summary>
        /// <param name="exclude">NoteManager enums to exclude from decrementing, either because they are already at minimum or were previously incremented.</param>
        /// <param name="passedLeftOver">The amount left over from "decrementAllTransitions"</param>
        /// WARNING: This function is recursive, any changes to it should ensure they don't allow infinite loops to occur.
        public void deductLeftovers(List<NoteManager.Notes> exclude, float passedLeftOver){
            float leftOver = 0;
            int numOfRemainingNotes =0;
            Dictionary<NoteManager.Notes, float> transitionsCopy = new Dictionary<NoteManager.Notes, float>(transitions);
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitionsCopy){
                if(!exclude.Contains(pair.Key)){
                    numOfRemainingNotes+=1;
                }
            }

            //if statment to ensure infinite loop isn't possible, because if all transitions are at a minimum, the function ends and the leftover is simply forgotten about.
            //(This isn't ideal but is better than an infinite loop).
            if(numOfRemainingNotes!=0){
                //dividing the amount to be deducted between the number of remaining transitions that can be deducted.
                float minusBy = passedLeftOver / numOfRemainingNotes;
                foreach (KeyValuePair<NoteManager.Notes, float> pair in transitionsCopy){
                    if (!exclude.Contains(pair.Key)) {
                        float oldValue = pair.Value;
                        float newValue = oldValue-minusBy;
                        //If this transition has also reached minimum, enter if statement.
                        if(newValue<minimumWeight){
                            exclude.Add(pair.Key);
                            newValue=minimumWeight;
                            leftOver += minusBy-(oldValue-newValue);
                        }
                        updateTransition(pair.Key, newValue);
                    }
                }
                //Recursive call if there is still leftover value to be deducted.
                if(leftOver>0){
                    deductLeftovers(exclude, leftOver);
                }
              }
        }

        /// <summary>
        /// Function called to set all of the transitions for the MarkovState correctly.
        /// </summary>
        /// <returns>A new dictionary of transitions for the markov state to assignt to itself.</returns>
        public Dictionary<NoteManager.Notes, float> setUpTransitions() {
            transitions = new Dictionary<NoteManager.Notes, float>();
            //Notes which are in the scale of the key of the MarkovState are assigned a higher value.
            float inScaleWeight = 0.125f;
            float notInScaleWeight = (1 - (7 * 0.125f)) / 5;

            for (int i = 1; i <= 12; i++) {
                NoteManager.Notes noteToAdd = (NoteManager.Notes)i;
                if (inScaleKeys.Contains(noteToAdd)) {
                    transitions.Add(noteToAdd, inScaleWeight);
                } else {
                    transitions.Add(noteToAdd, notInScaleWeight);
                }
            }

            return transitions;
        }

        /// <summary>
        /// Function to print all of the current transitions of the markov state.
        /// </summary>
        public void showTransitions() {
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitions) {
                print("from " + state + " to " + pair.Key + " has a prob of: " + pair.Value);
            }
        }

        /// <summary>
        /// Function to print the sum of all the transitions of the markov state.
        /// </summary>
        public void sumProbs() {
            float total = 0;
            foreach (KeyValuePair<NoteManager.Notes, float> pair in transitions) {
                total += pair.Value;
            }
            print("total weighting from state " + state + " is: " + total);
        }

    }

    /// <summary>
    /// Subclass of MarkovManager, behaves almost identically to the class MarkovChain but the states of the chain represent the amount of rests between notes rather than notes.
    /// </summary>
    public class RhythmMarkovChain{
        Dictionary<int, RhythmMarkovState> chain = new Dictionary<int, RhythmMarkovState>();

        public RhythmMarkovChain(){
            setUpChain();
        }

        /// <summary>
        /// Increment the weight of the rhythm markov state at a given rest index, to a given rest.
        /// </summary>
        /// <param name="stateRest">The state of the RhythmMarkovState to increment</param>
        /// <param name="transitionRest">The rest value of the RhythmMarkovState to increment the transition to of.</param>
        /// NOTE: MAY BE REVISED IN LATER VERSIONS TO NOT BE INCREMENTED BY FIXED AMOUNT.
        public void incrementWeight(int stateRest, int transitionRest) {
            float percentage = MarkovManager.incrementAmount;
            RhythmMarkovState state = chain[stateRest];
            float changedBy = state.incrementTransition(transitionRest, percentage);
            state.decrementAllTransitions(transitionRest, changedBy);
        }

        /// <summary>
        /// Function that creates # of "maximumRest" rhythm markov states, and stores them in its dictionary under the Key of the rest value of that state.
        /// </summary>
        private void setUpChain(){
            for (int i = 0; i <= MarkovManager.maximumRest; i++){//SPECIFIES RANGE OF POSSIBLE RESTS, MAKE SURE REFLECTS SAME IN RHYTHM STATE
                chain.Add(i, new RhythmMarkovState(i));
            }
        }

        /// <summary>
        /// Replace the state of a chain with a new given RhythmMarkovState.
        /// </summary>
        /// <param name="note">The rest value representing the key of the RhythmMarkovState to replace</param>
        /// <param name="newState">The new rhythm markov state to be inserted into the rhythmMarkovChain.</param>
        public void replaceState(int rest, RhythmMarkovState newState)
        {
            chain[rest] = newState;
        }

        /// <summary>
        /// Given a specific rest value, use the RhythmMarkovState at the key of that rest value to call the getNextRest function.
        /// </summary>
        /// <param name="currentRest">The value of the rest value key wanted in the chain.</param>
        /// <returns>an integer representing the next rest value.</returns>
        public int getNextRest(int currentRest){
            return chain[currentRest].getNextRest();
        }

        /// <summary>
        /// Given a certain rest value, return the RhythmMarkovState of that rest value.
        /// </summary>
        /// <param name="note">A rest value of the desired RhythmMarkovState.</param>
        /// <returns>A RhythmMarkovState which represents the transitions from the given rest value to all other rest valuess.</returns>
        public RhythmMarkovState getState(int rest){
            return chain[rest];
        }

        /// <summary>
        /// Function that prints the core details of the RhythmMarkovChain, such as the summed up probabilities of all of it's RhythmMarkovStates.
        /// </summary>
        public void asString(){
            foreach (KeyValuePair<int, RhythmMarkovState> pair in chain){
                pair.Value.sumProbs();
            }
        }

    }

    /// <summary>
    /// Subclass of MarkovManager, used to hold transitions from a certain rest value to all other possible rest values.
    /// </summary>
    public class RhythmMarkovState{
        [SerializeField]
        private int state;
        private Dictionary<int, float> transitions = new Dictionary<int, float>();
        //maximum and minimum weight of any transitions.
        private float maximumWeight;
        private float minimumWeight;
        //the number of lower rest values and higher rest values in the markov state.
        int numberOfLowerRests = Mathf.FloorToInt((maximumRest+1)/2);
        int numberOfHigherRests = Mathf.CeilToInt((maximumRest+1)/2);
        float lowerRhythmWeight;
        float higherRhythmWeight;
        


        public RhythmMarkovState(int newState){
            //lower rest values are given a higher weight to make them more probable.
            lowerRhythmWeight = (0.75f)/numberOfLowerRests;
            higherRhythmWeight = (0.25f)/numberOfHigherRests;
                
            maximumWeight = 0.75f;
            minimumWeight = (1-maximumWeight)/maximumRest;
            state = newState;
            setUpTransitions();
        }

        /// <summary>
        /// Function that generates a random value between 0-1. And will go through all of the transitions until it finds one
        /// where the value falls between the sum of all the transitions seen so far and the sum of all the transitions seen so far + the next transition.
        /// </summary>
        /// <returns>A rest value representing the randomly selected rest value from this RhythmMarkovState.</returns>
        public int getNextRest(){
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

        /// <summary>
        /// Function used to increment the transition in the RhythmMarkovStates dictionary.
        /// </summary>
        /// <param name="rest">The rest value of the transition to increment</param>
        /// <param name="percentage">The amount to increment the transition by.</param>
        /// <returns>How much the transition was incremented by. (Will always be somwhere between the passed float "precentage", and 0.
        /// Due to the maximumWeight variable to take into account.)</returns>
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




        /// <summary>
        /// Decrement all transitions in the RhythmMarkovState by a certain amount, excluding the passed parameter.
        /// </summary>
        /// <param name="restToIgnore">The rest value that was just incremented, and so wants to be avoided when decrementing.</param>
        /// <param name="changedBy">The amount that a transition was just incremented by, this will be divided enely amoungst the other transitions.</param>
        public void decrementAllTransitions(int restToIgnore, float changedBy){
            float leftOver = 0f;
            List<int> restsToIgnore = new List<int>();
            restsToIgnore.Add(restToIgnore);
            //divide the amount to change by by the maximumRest allowed, to split it evenly between the remaining transitions.
            float minusBy = changedBy / (maximumRest);
            Dictionary<int, float> transitionsCopy = new Dictionary<int, float>(transitions);
            foreach (KeyValuePair<int, float> pair in transitionsCopy){
                
                //If current transition is one to ignore don't enter if statement.
                if (pair.Key != restToIgnore) {
                    float oldValue = pair.Value;
                    float newValue = oldValue-minusBy;
                    if(newValue<minimumWeight){
                        //if statement that is entered if the current transition has reached minimum.
                        //It is added to the list of notes to ignore,
                        restsToIgnore.Add(pair.Key);
                        //It's value is set to the minimum.
                        newValue=minimumWeight;
                        //and the amount leftover from decrementing it as far as possible is recorded.
                        leftOver += minusBy-(oldValue-newValue);
                    }
                    updateTransition(pair.Key, newValue);
                }
            }
            //If any transition hit minimum, the amount that was leftover needs to be divided amoungst the remaining transitions.
            if(leftOver>0){
                deductLeftovers(restsToIgnore, leftOver);
            }
        }

        /// <summary>
        /// Function to deduct any leftover value from the function "decrementAllTransitions".
        /// </summary>
        /// <param name="exclude">Rest values to exlcude from decrementing, either because they are already at minimum or were previously incremented.</param>
        /// <param name="passedLeftOver">The amount left over from "decrementAllTransitions"</param>
        /// WARNING: This function is recursive, any changes to it should ensure they don't allow infinite loops to occur.
        public void deductLeftovers(List<int> exclude, float passedLeftOver){
            float leftOver = 0;
            int numOfRemainingRests =0;
            Dictionary<int, float> transitionsCopy = new Dictionary<int, float>(transitions);
            foreach (KeyValuePair<int, float> pair in transitionsCopy){
                if(!exclude.Contains(pair.Key)){
                    numOfRemainingRests+=1;
                }
            }
            //if statment to ensure infinite loop isn't possible, because if all transitions are at a minimum, the function ends and the leftover is simply forgotten about.
            //(This isn't ideal but is better than an infinite loop).
            if (numOfRemainingRests != 0){
                //dividing the amount to be deducted between the number of remaining transitions that can be deducted.
                float minusBy = passedLeftOver / numOfRemainingRests;

                foreach (KeyValuePair<int, float> pair in transitionsCopy){
                    if (!exclude.Contains(pair.Key)) {
                        float oldValue = pair.Value;
                        float newValue = oldValue-minusBy;
                        //If this transition has also reached minimum, enter if statment.
                        if(newValue<minimumWeight){
                            exclude.Add(pair.Key);
                            newValue=minimumWeight;
                            leftOver += minusBy-(oldValue-newValue);
                        }
                        updateTransition(pair.Key, newValue);
                    }
                }
                //Recursive call if there is still leftover value to be deducted.
                if (leftOver>0){
                    deductLeftovers(exclude, leftOver);
                }
              }
        }        

        /// <summary>
        /// Function to get transition weight given a rest value.
        /// </summary>
        /// <param name="rest">Rest value of transition to get from this RhythmMarkovState.</param>
        /// <returns>A float representing the weight of the transition from this RhythmMarkovState to the given rest value.</returns>
        public float getTransition(int rest){
            return transitions[rest];
        }

        /// <summary>
        /// Function to update a transition value of the rhythm markov state.
        /// </summary>
        /// <param name="rest">The rest value whose's transition to will be updated.</param>
        /// <param name="newValue">The value to change the transition too.</param>
        public void updateTransition(int rest, float newValue) {
            transitions[rest] = newValue;
        }

        /// <summary>
        /// Function called to set all of the transitions for the RhythmMarkovState correctly.
        /// </summary>
        /// <returns>A new dictionary of transitions for the rhythm markov state to assignt to itself.</returns>
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

        /// <summary>
        /// Function to print all of the current transitions of the markov state.
        /// </summary>
        public void showTransitions() {
            foreach (KeyValuePair<int, float> pair in transitions) {
                print("Rhythm: from " + state + " to " + pair.Key + " has a prob of: " + pair.Value);
            }
        }
        /// <summary>
        /// Function to print all of the current transitions of the markov state.
        /// </summary>
        public void sumProbs(){
            float total = 0;
            foreach (KeyValuePair<int, float> pair in transitions)
            {
                total += pair.Value;
            }
            print("Rhythm: total weighting from state " + state + " is: " + total);
        }
    }
}
