using UnityEngine;
using System.Collections;
using System.Collections.Generic;	//for using Dictionaries.

using UnityEngine.UI;	//library included to modify the UI

public class UIupdateScript : MonoBehaviour {

	//This class updates the user interface 
		//located in the two bottom panels in the display.

	//Instance of the class.
	public static UIupdateScript myUI;

	//Text values.
	public Text hungerAmount, lifeAmount, moneyAmount; 
	public Text statusMsg;

	public Text choiceSelector;  //For selecting items in the restaurant.
		float selectorBlink=0.3f;	
		Vector3[] positionArray; 
		
	//Color values.
	Color myGreenRGB;
	Color myOrangeRGB;
	Color currentHungerColor, currentLifeColor;

	//Defining Dictionary array.
	Dictionary<PlayerScript.UnitState, string> statusMsgDict = 
					new Dictionary<PlayerScript.UnitState, string>();

	public static string itemFound="x";
	public static float moneyReceived=0f;

	//Game Over Check var and Screen.
	public GameObject gameOverScreenHolder;

	// Use this for initialization
	void Awake () {

		myUI=this;

		//Turn off gameOverScreen.
		gameOverScreenHolder.SetActive(false);
		//Turn off restaurant selector.
		choiceSelector.canvasRenderer.SetAlpha(0f);

		//Setting up choice Selector point array.
		positionArray = new [] { new Vector3(115.77f,-361.8f,0f), 
								  new Vector3(331f,-361.8f,0f),
								  new Vector3(115.77f,-389.03f,0f),
								  new Vector3(331f,-389.03f,0f)	//The 'Leave' option is last.
																	};

		//Setting up.
		setupMessageStrings();

		//Set up Color values to default.
			myGreenRGB= new Color32(0,200,0,255);
			myOrangeRGB= new Color32(255,142,0,255);

			currentHungerColor=Color.yellow;
			currentLifeColor=myGreenRGB;

	}
	
	// Update is called once per frame
	void Update () {
	


	}

//-------1. UI setup and update methods.

	void setupMessageStrings()
	{

			//Initializing message strings.
				//setting up statusMsg array.
			
			//I'm doing this by adding strings to my dictionary.
			statusMsgDict.Add(PlayerScript.UnitState.BLOCKED, 
			                  "There's a traffic light post ahead.\nYou'll have to go around it.");
			
			statusMsgDict.Add(PlayerScript.UnitState.OUT_OF_BOUNDS, 
			                  "You have to go back.\nYou're in the edge of the map.");
			
			statusMsgDict.Add(PlayerScript.UnitState.REST_AREA, 
			                  "You decide to go for a rest.");
			
			statusMsgDict.Add(PlayerScript.UnitState.GO_TO_RESTAURANT, 
		                  	  "You go to the restaurant.");

			statusMsgDict.Add(PlayerScript.UnitState.RESTAURANT_CLOSED, 
		      	              "The restaurant is closed right now.\nCome back after a few turns.");

			statusMsgDict.Add(PlayerScript.UnitState.RESTAURANT, 
			                  "Welcome to Chicken King.\nWhat would you like to buy?\n"+
		                  	  "Ch. Leg $2.50      Soda $0.75    Fries $0.60          Leave");
			
			statusMsgDict.Add(PlayerScript.UnitState.DUMPSTER, 
			                  "You check out the nearest dumpster.\nWonder if there's food?");
			
			statusMsgDict.Add(PlayerScript.UnitState.DUMPSTER_FOUND_GOOD, 
		                  "You found "+itemFound+"!");

			statusMsgDict.Add(PlayerScript.UnitState.DUMPSTER_FOUND_BAD, 
		                  "You found an old "+itemFound+".\nBleh, you don't feel so good...");

			statusMsgDict.Add(PlayerScript.UnitState.DUMPSTER_EMPTY, 
		                  "You check out the nearest dumpster...\n nothing edible there.");

			statusMsgDict.Add(PlayerScript.UnitState.SPECIAL_END, 
			                  "You're back in the street.");
			
			statusMsgDict.Add(PlayerScript.UnitState.START, 
			                  "Another day on the streets...\nTime to beg at the traffic stop.");

			statusMsgDict.Add(PlayerScript.UnitState.BOUGHT_FOOD, 
		                  "Thank you for your purchase!");

			statusMsgDict.Add(PlayerScript.UnitState.BUY_FAIL, 
		                  "You don't have enough funds!");

			statusMsgDict.Add(PlayerScript.UnitState.BACK_IN_STREET, 
		                  "You're back in the street.");	

			statusMsgDict.Add(PlayerScript.UnitState.CANT_BEG, 
			                  "You can't beg in moving traffic!\nThe traffic light must be red.");
			
			statusMsgDict.Add(PlayerScript.UnitState.CAN_BEG, 
		                  "You can beg this driver for change.\nPress Space to beg.");

			statusMsgDict.Add(PlayerScript.UnitState.BEG_FAIL, 
			                  "The driver won't give you any money!");	

			statusMsgDict.Add(PlayerScript.UnitState.BEG_WIN, 
		                  "You received $"+moneyReceived.ToString("F2")+"!");	

			statusMsgDict.Add(PlayerScript.UnitState.BEG_BROKE, 
		                  "The driver has no money!");	
	}


	public void updateMessage (PlayerScript.UnitState whatState) {


		//This method updates the status message portion of the UI,
			//depending on what gameState is sent as an argument.

		//		string resultString= "_";
		//
		//		if(whatState==PlayerScript.UnitState.BLOCKED)
		//			statusMsg.text= statusMsgDict[PlayerScript.UnitState.BLOCKED];
		//
		//		else if(whatState==PlayerScript.UnitState.OUT_OF_BOUNDS)
		//			statusMsg.text= statusMsgDict[PlayerScript.UnitState.OUT_OF_BOUNDS];

		if(whatState==PlayerScript.UnitState.DUMPSTER_FOUND_GOOD)
			statusMsgDict[PlayerScript.UnitState.DUMPSTER_FOUND_GOOD]=
				"You found "+itemFound+"!";
	
		if(whatState==PlayerScript.UnitState.DUMPSTER_FOUND_BAD)
			statusMsgDict[PlayerScript.UnitState.DUMPSTER_FOUND_BAD]=
				"You found an old "+itemFound+".\nBleh, you don't feel so good...";

		if(whatState==PlayerScript.UnitState.BEG_WIN)
			statusMsgDict[PlayerScript.UnitState.BEG_WIN]=
				"You received $"+moneyReceived.ToString("F2")+"!";	

		statusMsg.text= statusMsgDict[whatState];


	}



	public void updateAttributesInUI () {

		//Update color for hunger and life.
		updateAttributeColor();

		hungerAmount.text= 
			GameManagerScript.currentHunger.ToString()+ " / "+ GameManagerScript.maxHunger.ToString() ; 

		lifeAmount.text=
			GameManagerScript.currentLife.ToString()+ " / "+ GameManagerScript.maxLife.ToString() ;; 

		moneyAmount.text="$"+GameManagerScript.currentMoney.ToString("F2"); 

	}

	void updateAttributeColor()
	{

		//This method checks the value of some player parameters,
			//and changes the color of the interface to reflect this.

		//First, hunger.
			if(GameManagerScript.currentHunger > 10)
				currentHungerColor=Color.yellow;

			else 
				currentHungerColor=myOrangeRGB;

		//Second, life.
			if(GameManagerScript.currentLife == GameManagerScript.maxLife)
				currentLifeColor=myGreenRGB;
			
			else if(GameManagerScript.currentLife > 2)
				currentLifeColor=Color.yellow;

			else 
				currentLifeColor=Color.red;

		//Finally, update.
			hungerAmount.color=currentHungerColor;
			lifeAmount.color=currentLifeColor;

	}


//----2. Restaurant Selection Mode Methods.---
	//These methods are activated when the player enters the fast food restaurant.
	public void startRestaurantSelectorUI()
	{
		//Showing the selector.
		choiceSelector.canvasRenderer.SetAlpha(255f);

		//Setting the selector to blink.
		InvokeRepeating("selectorBlinkSequence", 0f, selectorBlink);
	}

	void selectorBlinkSequence()
	{

		//turns the cursor on or off.
		if(choiceSelector.canvasRenderer.GetAlpha()>254f)
			choiceSelector.canvasRenderer.SetAlpha(0);
		else choiceSelector.canvasRenderer.SetAlpha(255);

	}

	public void stopRestaurantSelectorUI()
	{

		//Hiding the selector.
		choiceSelector.canvasRenderer.SetAlpha(0f);

		//Stopping the Coroutine.
		CancelInvoke();

	}

	public void updateRestaurantSelectorUI()
	{
		//sets new position for restaurantSelector
		if(GameManagerScript.currentRestaurantSelection==0)
			choiceSelector.rectTransform.localPosition= positionArray[0]; 

		else if(GameManagerScript.currentRestaurantSelection==1)
			choiceSelector.rectTransform.localPosition= positionArray[1];

		else if(GameManagerScript.currentRestaurantSelection==2)
			choiceSelector.rectTransform.localPosition= positionArray[2]; 

		else if(GameManagerScript.currentRestaurantSelection==3)
			choiceSelector.rectTransform.localPosition= positionArray[3]; 

	}

//-----------

}
