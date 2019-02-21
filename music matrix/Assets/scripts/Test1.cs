using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests{


    public class Test1{

        GameObject mmObject;
        GameObject kmObject;
        GameObject tmObject;

        MarkovManager markovManager;
        KeyManager keyManager;
        TrackManager trackManager;

        [TestFixtureSetUp]
        public void Setup(){
            mmObject = new GameObject();
            markovManager = mmObject.AddComponent(typeof(MarkovManager)) as MarkovManager;
            
            kmObject = new GameObject();
            keyManager = kmObject.AddComponent(typeof(KeyManager)) as KeyManager;
            
            
            /*tmObject = new GameObject();
            trackManager = tmObject.AddComponent(typeof(TrackManager)) as TrackManager;
            Assert.NotNull(tmObject);
            Assert.NotNull(trackManager);*/
        }

        [Test]
        public void TestSetUpCorrectly() {
            Assert.NotNull(mmObject);
            Assert.NotNull(kmObject);
            Assert.NotNull(markovManager);
            Assert.NotNull(keyManager);

        }
        
        [Test]
        public void TestMarkovManager(){
            //trackManager.key = NoteManager.Notes.C2;

            //markovManager.trackManager = trackManager;

            //markovManager.startMarkovManaging();
            //MarkovManager.MarkovPair markovPair = markovManager.getMarkovPair();
            //Assert.NotNull(markovPair);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator Test1WithEnumeratorPasses()
        {
            Assert.AreEqual(2, 2);
            yield return null;
        }
    }
}
