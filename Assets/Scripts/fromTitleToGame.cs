using UnityEngine;
using System.Collections;

public class fromTitleToGame : MonoBehaviour {

	//This script is used to progress from the Title and Introductory Screen to the game mode.

	//This variable keeps track of the number of button presses.  
	//There are only 2 screens to go through before entering the play screen.
	int howManyPauses=0;

	// Update is called once per frame
	void Update () {
	
			if(Input.GetKeyDown(KeyCode.Space) )
			{
				if(howManyPauses==0)
				GameObject.FindGameObjectWithTag("Title").GetComponent<SpriteRenderer>().enabled=false;

				else Application.LoadLevel("MapTest1");
		
				howManyPauses++;
			}

	}
}
