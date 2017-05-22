using UnityEngine;
using System.Collections;

public class carProperties : MonoBehaviour {

	//This script is attached to every car object on the scene.

	public bool hasGivenMoney;
	public float money;
	public bool isOnRoad;
	public int willDriverBeg;  //2 means 100%, 1 means 30%, 0 means 0%  probability.


	//Setting the amount of money 
		//this car's driver is willing to donate, if any.
		//Also assigning the car's color. (there's only one car shape, and multiple colors).
	void Awake () {
	
		hasGivenMoney=false;
		isOnRoad=false;

		int chance= Random.Range (0,6);
		if(chance==0)
			willDriverBeg= 0;
		else if(chance<4)
			willDriverBeg= 1;
		else if(chance<6)
			willDriverBeg= 2;
		
		randomCarColor();

	}

	//A function that assigns a random car color to the current car object.
	public void randomCarColor () 
	{

		int chance= Random.Range (0,4);


		if(chance==0)  //Green
			gameObject.GetComponent<SpriteRenderer>().sprite= Resources.Load<Sprite>("carSprite_green_small");
		else if (chance==1)	//Red
			gameObject.GetComponent<SpriteRenderer>().sprite= Resources.Load<Sprite>("carSprite_red_small");
		else if (chance==2)	//Blue
			gameObject.GetComponent<SpriteRenderer>().sprite= Resources.Load<Sprite>("carSprite_blue_small");
		else if (chance==3)	//Sky Blue
			gameObject.GetComponent<SpriteRenderer>().sprite= Resources.Load<Sprite>("carSprite_skyBlue_small");

	}

}
