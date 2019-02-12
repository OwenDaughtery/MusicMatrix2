using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;

public class Test1 {
    
    
    private MarkovManager markovManager;
    //private TrackManager trackManager;


    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator Testing1() {
        
        SceneManager.LoadScene(0);
        yield return new WaitForSeconds(5);
        GameObject hello = (GameObject.FindGameObjectWithTag("MarkovManager"));
        Debug.Log(hello);
        //trackManager = GameObject.FindGameObjectWithTag("TrackManager").GetComponent<TrackManager>();
        //trackManager.generateScale(NoteManager.Notes.C2, 3);

        
        //Assert.AreEqual(predictedScale, actualScale);
        Assert.AreEqual(1,1);
        yield return null;
    }
}
