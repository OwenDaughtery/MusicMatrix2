using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewHandler : MonoBehaviour {
	public MarkovManager markovManager;

	public void approveMarkov(){
		markovManager.approveMarkovChain();
	}

	public void disapproveMarkov(){
		print("no!");
	}
}
