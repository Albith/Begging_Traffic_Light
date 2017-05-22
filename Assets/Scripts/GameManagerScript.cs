using UnityEngine;
using System.Collections;

public class GameManagerScript : MonoBehaviour {

	//This class manages the game's logic, checks for ending and losing conditions.
		//It manages the one traffic light in the street.
		//It delegates a lot of player functions and car management to other classes.
	public static GameManagerScript gameManager;

//-------Game Values.------

	//Game state variables.
	bool isGameOver=false;
		public static bool isDebug=true;
		public static bool isPrompted=false, choiceMade=false;

	//Game constants.
	public static int turnsPerDay=20;

		//Dumpster variables.
		int dumpsterUpdateFreq;
		int dumpsterFoodLifeCount=0;
		public static int dumpsterState=0;   //1 of 4: 
		// 0 is empty, 1 is 1 bag, 2 is 2 bags, 3 is 3 bags, 
		public static bool isTrashCollected=false;

		//Store Hours stuff.
		public static bool isStoreOpen=true;
		int turnsStoreOpen=20;
		int turnsStoreClosed=10;
		int currentTurnsStore=0;

		//Traffic Light enumeration.
		public enum TrafficState
		{
			RED,
			GREEN,
			YELLOW

		}

		public static TrafficState currentTrafficLight;

		//Variables storing the duration of the Traffic Lights in turns.	
		int redDuration,greenDuration, yellowDuration;
		int trafficTurnCount=0;
		public GameObject topLight, bottomLight;

	//Player state counters.
	public static int currentTurn=0;
	int turnsGoneHungry=0;
	int turnsGoneFull=0;

		//Variables used for selecting items while in the restaurant.
		public static int currentRestaurantSelection=0;
		public static int maxRestaurantSelections=3;
		float restaurantMsgDelay=1f;

	//Player attributes. 
		//(movement, display and grid-related variables are located in the playerScript)
		public static int maxHunger=15;
		public static int maxLife=5;
		int turnsBeforeStarving=2;	
		int turnsBeforeRegen=5;

		public static int currentHunger;  //starts at maxHunger.
		public static int currentLife;	//starts at maxLife.
		public static float currentMoney;


//--------Sounds. The gameManager plays the sounds in the game.
	public AudioClip[] soundArray;
	public enum soundIndexes
	{

		WALL_HIT,
		WALK,
		FOOD_BUY,
		FOOD_BUY_FAIL,
		EATING,
		WALK_HURT

	}

// ----Initializing this class's variables.
	void Awake () {
	
		gameManager= this;

		dumpsterUpdateFreq= turnsPerDay/4;
			//Traffic Light variable and display setup.
				currentTrafficLight= (TrafficState)Random.Range (0,3);
				
					if(currentTrafficLight==TrafficState.RED)
						{
							topLight.GetComponent<SpriteRenderer>().sprite=
							Resources.Load<Sprite>("redOn");

							bottomLight.GetComponent<SpriteRenderer>().sprite=
							Resources.Load<Sprite>("redOn");

						}

					else if(currentTrafficLight==TrafficState.GREEN)
					{
						topLight.GetComponent<SpriteRenderer>().sprite=
							Resources.Load<Sprite>("greenOn");
						
						bottomLight.GetComponent<SpriteRenderer>().sprite=
							Resources.Load<Sprite>("greenOn");
						
					}

					else if(currentTrafficLight==TrafficState.YELLOW)
					{
						topLight.GetComponent<SpriteRenderer>().sprite=
							Resources.Load<Sprite>("yellowOn");
						
						bottomLight.GetComponent<SpriteRenderer>().sprite=
							Resources.Load<Sprite>("yellowOn");
						
					}

				redDuration=10;
				greenDuration=redDuration; 
				yellowDuration=4;


		currentHunger=maxHunger;
		currentLife=maxLife;
		currentMoney=0.05f;	//this will be formatted in the UI updateScript.
	


		//Setting game over options.
		isGameOver=false;

	}

	void Start()
	{
		//Updating the UI in the Start() in case other scripts start after our Awake() method.
		UIupdateScript.myUI.updateAttributesInUI();

		//Randomize the dumpster conditions.
		updateDumpster();

	}


	//This method updates the game state after a player action, marking the end of the turn.
	public void nextTurn(bool isLeavingSpecial)
	{
		currentTurn++;

		//updateStoreHours.  The store may open or close, depending on the turns elapsed.
			//after X turns, close store for Y turns, then open again.
			updateStoreHours();

		//update the Dumpster contents.  
			//There may be trash (food, spoiled food or nothing edible), or the may not be anything.
			//updating dumpster values.
			if(currentTurn%dumpsterUpdateFreq == 0)
				updateDumpster();


		//update the Traffic Light conditions.
			updateTrafficLight();
	

		//updateCarsManager.
			//It will update the status and movement of all the cars.
			gameObject.GetComponent<CarsManagerScript>().updateCarsManager();

		//updating Player Conditions. 
			//Here, I'm -not- updating player state if they have just left one of the special areas.
			if(!isLeavingSpecial)
				update_PlayerState();

	}


	void Update()
	{
		//We are only checking for the space key to be pressed at the end of the game.
		if(isGameOver)
			if( Input.GetKeyUp(KeyCode.Space) )
			resetGame();

	}


//-------------Update methods.-------->

//------Traffic Light Management methods.---


	void updateTrafficLight()
	{

		//If the light is yellow and has been for some time, change the light.
		if(currentTrafficLight==TrafficState.YELLOW && trafficTurnCount >= yellowDuration )
		{
			//Turning Light to Red.
			currentTrafficLight=TrafficState.RED;
			//resetting Counter.
			trafficTurnCount=0;

			topLight.GetComponent<SpriteRenderer>().sprite=
				Resources.Load<Sprite>("redOn");
			
			bottomLight.GetComponent<SpriteRenderer>().sprite=
				Resources.Load<Sprite>("redOn");		
		}

		//If the light is red and has been for some time, change the light.		
		else if(currentTrafficLight==TrafficState.RED && trafficTurnCount >= redDuration )
		{
			//Turning Light to Green.
			currentTrafficLight=TrafficState.GREEN;
			//resetting Counter.
			trafficTurnCount=0;

			topLight.GetComponent<SpriteRenderer>().sprite=
				Resources.Load<Sprite>("greenOn");
			
			bottomLight.GetComponent<SpriteRenderer>().sprite=
				Resources.Load<Sprite>("greenOn");
			
		}

		//If the light is green and has been for some time, change the light.		
		else if(currentTrafficLight==TrafficState.GREEN && trafficTurnCount >= greenDuration )
		{
			//Turning Light to Yellow.
			currentTrafficLight=TrafficState.YELLOW;
			//resetting Counter.
			trafficTurnCount=0;

			topLight.GetComponent<SpriteRenderer>().sprite=
				Resources.Load<Sprite>("yellowOn");
			
			bottomLight.GetComponent<SpriteRenderer>().sprite=
				Resources.Load<Sprite>("yellowOn");
			
		}
	
		//increase the traffic turn counter.
		trafficTurnCount++;

	}


//----Store Management methods.
	void updateStoreHours()
	{
		//If the store has been open for enough turns, close the store.
		if ( currentTurnsStore > turnsStoreOpen && isStoreOpen ) 
		{
			//Store is now closed.
			isStoreOpen=false;

			//Turn on the restaurant closed sprite.
			GameObject.FindGameObjectWithTag("rest_Closed").GetComponent<SpriteRenderer>().enabled=true;

			//reset counter.
			currentTurnsStore=0;

		}
		//If the store was closed and enough time has passed, open the store.
		else if ( currentTurnsStore > turnsStoreClosed && !isStoreOpen ) 
		{
			//Store is now open.
			isStoreOpen=true;
			
			//Turn off the restaurant closed sprite.
			GameObject.FindGameObjectWithTag("rest_Closed").GetComponent<SpriteRenderer>().enabled=false;
			
			//reset counter.
			currentTurnsStore=0;
			
		}

		//Updating counter for storeTurns at the bottom.
		   currentTurnsStore++;
	}

//----Dumpster Management methods.
	void updateDumpster()
	{

		//Randomize the contents in the dumpster.
		int chance;

		if(currentTurn<turnsPerDay)
			chance= Random.Range(0,9); 
		else 
			chance= Random.Range(0,15);	
			//After 20 turns, there is less of a chance of finding useful trash.

		//Depending on the chance,
			//update the dumpster graphic and content.
			if ( chance < 2 )
				dumpsterState=3;
			else if(chance <4 )
				dumpsterState=2;
			else if(chance <7 )
				dumpsterState=1;
			else
				dumpsterState=0;

		//Updating the Dumpster Graphic.
			updateDumpsterGraphic();
			isTrashCollected=false;

	}

	void updateDumpsterGraphic()
	{

		if (dumpsterState==3)	//This state includes a lot of trash.
		{
			//Show Trash Bags.
			GameObject.FindGameObjectWithTag("bagA").GetComponent<SpriteRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag("bagB").GetComponent<SpriteRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag("bagC").GetComponent<SpriteRenderer>().enabled=true;

		}


		else if (dumpsterState==2)	//This state includes a moderate amount of trash.
		{
			//Show Trash Bags.

			int chanceA= Random.Range (0,3);
			int chanceB= Random.Range (0,3);
			int chanceC= Random.Range (0,3);

			while(chanceB==chanceA)
				chanceB= Random.Range (0,3);
			while(chanceC==chanceA || chanceC==chanceB)
				chanceC= Random.Range (0,3);

			string []whatTrash= {"A","B","C"};
			string trashBagTag= "bag";

			GameObject.FindGameObjectWithTag(trashBagTag+whatTrash[chanceA]).GetComponent<SpriteRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag(trashBagTag+whatTrash[chanceB]).GetComponent<SpriteRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag(trashBagTag+whatTrash[chanceC]).GetComponent<SpriteRenderer>().enabled=false;
			
		}

		else if (dumpsterState==1)	//This state includes a small amount of trash.
		{
			//Show Trash Bags.
			
			int chanceA= Random.Range (0,3);
			int chanceB= Random.Range (0,3);
			int chanceC= Random.Range (0,3);
			
			while(chanceB==chanceA)
				chanceB= Random.Range (0,3);
			while(chanceC==chanceA || chanceC==chanceB)
				chanceC= Random.Range (0,3);
			
			string []whatTrash= {"A","B","C"};
			string trashBagTag= "bag";
			
			GameObject.FindGameObjectWithTag(trashBagTag+whatTrash[chanceA]).GetComponent<SpriteRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag(trashBagTag+whatTrash[chanceB]).GetComponent<SpriteRenderer>().enabled=false;
			GameObject.FindGameObjectWithTag(trashBagTag+whatTrash[chanceC]).GetComponent<SpriteRenderer>().enabled=false;
			
		}

		else{	//There is no trash at the dumpster.

			//Hide Trash Bags.
			GameObject.FindGameObjectWithTag("bagA").GetComponent<SpriteRenderer>().enabled=false;
			GameObject.FindGameObjectWithTag("bagB").GetComponent<SpriteRenderer>().enabled=false;
			GameObject.FindGameObjectWithTag("bagC").GetComponent<SpriteRenderer>().enabled=false;

			//Hide Objects.
			GameObject.FindGameObjectWithTag("objA").GetComponent<SpriteRenderer>().enabled=false;
			GameObject.FindGameObjectWithTag("objB").GetComponent<SpriteRenderer>().enabled=false;

		}

		//A random group of trash sprites will be displayed in the dumpster area. 

		if(dumpsterState!=0)
		{
			//Show objects 


			int chance= Random.Range (0,4);

			string []whatObject= {"A","B"};
			string objectTag= "obj";

			if (chance==0) //show nothing.
			{
				GameObject.FindGameObjectWithTag(objectTag+whatObject[0]).
							GetComponent<SpriteRenderer>().enabled=false;
				GameObject.FindGameObjectWithTag(objectTag+whatObject[1]).
							GetComponent<SpriteRenderer>().enabled=false;
			}

			else if(chance<3)
			{
				GameObject.FindGameObjectWithTag(objectTag+whatObject[chance-1]).
					GetComponent<SpriteRenderer>().enabled=true;

				if(chance==2)  //hide the first obj.
					GameObject.FindGameObjectWithTag(objectTag+whatObject[0]).GetComponent<SpriteRenderer>().enabled=false;
				else
					GameObject.FindGameObjectWithTag(objectTag+whatObject[1]).GetComponent<SpriteRenderer>().enabled=false;

			}

			else{

				GameObject.FindGameObjectWithTag(objectTag+whatObject[0]).
					GetComponent<SpriteRenderer>().enabled=true;
				GameObject.FindGameObjectWithTag(objectTag+whatObject[1]).
					GetComponent<SpriteRenderer>().enabled=true;

			}

		}
					

	}


//----Player State Methods.------->

	public void update_PlayerState()
	{

		//Updating the player state: hungry, not hungry or dead.
			//Walking(moving 1 space) consumes 1 hunger.

		//If the player is not hungry:
		if (currentHunger>0)
			{
				currentHunger--;
				
				//resets to 0;
				turnsGoneHungry=0;
				turnsGoneFull++;
				
				if(turnsGoneFull%turnsBeforeRegen==0)
					if(currentLife<maxLife)
						currentLife++;
			}

		//Else if the player's hunger is 0.
		else{

			turnsGoneHungry++;
			turnsGoneFull=0;

			//Check if the player has died.
			if(currentLife==0)
				{
					runGameOver();
					print ("Game Over.");
					
				}
			
			//Player loses life if he/she's hungry for too long.
			if(turnsGoneHungry>turnsBeforeStarving)
				currentLife--;

			}

		if(currentLife==0)
			playSound(soundIndexes.WALL_HIT); //playing death Sound.	
		else
			{
				//The player is still alive, play the walking sounds after the player moves.

				if(turnsGoneHungry<=turnsBeforeStarving)
					playSound(soundIndexes.WALK);
				else 
					playSound(soundIndexes.WALK_HURT);

			}

		//update Attributes shown on the user interface.
		UIupdateScript.myUI.updateAttributesInUI();

	}

//-----End of update Methods, now to Game State Management.
	void runGameOver()
	{
		isGameOver=true;
		UIupdateScript.myUI.gameOverScreenHolder.SetActive(true);
		//disabling player controls.
		PlayerScript.canMove=false;
	}

	void resetGame()
	{
		//resetting cars.
		gameObject.GetComponent<CarsManagerScript>().resetAllCars();

		//re-initialize values.
		currentHunger=maxHunger;
		currentLife=maxLife;
		currentMoney=0.05f;	//this will be formatted in the UI updateScript.

		UIupdateScript.myUI.gameOverScreenHolder.SetActive(false);
		UIupdateScript.myUI.updateAttributesInUI();

		//Resetting the status message.
		UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.START);

		//Play a Sound.
		GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.EATING);	
		
		//Enabling player controls.
		PlayerScript.canMove=true;

		isGameOver=false;
		
	}

	public void playSound(soundIndexes whichSound)
	{
		
		audio.clip=soundArray[(int)whichSound];
		audio.Play();
		
	}

//----Special area-related methods.

	//------Dumpster methods.
	public void attemptToCollectDumpster()
	{
		print ("in attemptToCollectDumpster().");

		//Some items recover player attributes, others reduce them.
		//Note: I'm using randomness and dumpster State to determine trash's effects on the player.
			//I'm -not- using these values below.
		/*
		 * half-empty water bottle.  (5 hunger, 0 life)
		 * leftover rice & beans     (5 hunger, 1 life)
		 * piece of a sandwich.		 (5 hunger, 2 life)
		 * a spoiled apple			 (0 hunger, -1 life)
		 * a full energy drink		 (10 hunger, 0 life)
		 * nothing edible.			 *no attributes.
		 * 
		 * To do: do item updates later...
		 */ 

		//I may be using randomization too much.
		int chance=0;
		int maxChance=0;
		bool isFoodOkay=true;

		if(dumpsterState==1)
			maxChance=5;
		else if(dumpsterState==2)
			maxChance=7;
		else 
			maxChance=9;

		//Rolling the dice...
			chance= Random.Range (0, maxChance);

		//Modify the player attributes, AND assign the item name for display.
			if (chance<3)
			{
				//no status changes.
				UIupdateScript.itemFound="nothing edible";

				//Play sound.
				GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.FOOD_BUY_FAIL);	

			}

			else if (chance<5)
			{
				
				int chanceB= Random.Range(0,2);

				if(chanceB==0)
					{
						UIupdateScript.itemFound="a half-empty water bottle";
						updatePlayerAttributes(5,0);
					}
				else
					{
						UIupdateScript.itemFound="leftover rice and beans";
						updatePlayerAttributes(5,1);

					}
			}

			else if(chance <7)
			{
				int chanceB= Random.Range(0,2);
			
				if(chanceB==0)	
					{
						UIupdateScript.itemFound="a piece of sandwich";
						updatePlayerAttributes(4,2);

					}
				else
					{
						UIupdateScript.itemFound="a full energy drink";
						updatePlayerAttributes(10,0);
					}
			}

			
			else if(chance<9)
			{
				isFoodOkay=false;
				UIupdateScript.itemFound="apple";
				updatePlayerAttributes(0,-1);

			}
			

		//print message and play sound.
			
			if(!isFoodOkay )
				{
					UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.DUMPSTER_FOUND_BAD);
					playSound(soundIndexes.WALL_HIT);
				}
			else 
				{
					UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.DUMPSTER_FOUND_GOOD);
					if(chance>2)
						playSound(soundIndexes.EATING);
				}

		//set IsTrashCollected to true.
			isTrashCollected=true;

			print ("item found is "+UIupdateScript.itemFound);

		//update UI.
		UIupdateScript.myUI.updateAttributesInUI();


	}

	//------Restaurant.

	//Note: this function's name should be changed to a more official name.
		//Such as, enter_restaurantMode().
	public void testShop()
	{
		print ("Now in store mode.");

		//Update UI
		isPrompted=true;

		//Start Restaurant Selector.
		UIupdateScript.myUI.startRestaurantSelectorUI();
	
	}
	
	//This method updates the restaurant mode's logic.
	public void updateRestaurantSelection(KeyCode whichDirection)
	{
		//The arrow buttons under this mode serve to 
			//choose between different meal options to purchase, or exiting the store.

		//Choosing left or right decrements or increments the current selection.
		if(whichDirection==KeyCode.RightArrow)
		{
			if(currentRestaurantSelection == (maxRestaurantSelections) )
				currentRestaurantSelection=0;
			else currentRestaurantSelection++;
		}	

		else if(whichDirection==KeyCode.LeftArrow)
		{
			if(currentRestaurantSelection == 0 )
				currentRestaurantSelection=maxRestaurantSelections;
			else currentRestaurantSelection--;
		}

		//Added up and down functionality. 
			//It allows players to flip between selections faster.

		else if(whichDirection==KeyCode.UpArrow || whichDirection==KeyCode.DownArrow)
		{
			if(currentRestaurantSelection < 2 )
				currentRestaurantSelection+=2;
			else currentRestaurantSelection-=2;
		}

		//Updating the selector UI.
		UIupdateScript.myUI.updateRestaurantSelectorUI();

	}


	public void leaveStore()
	{
		//Exiting out of the store mode.
		isPrompted=false; 
		choiceMade=false;
		PlayerScript.canMove=true;

		//Update player to go Up, to the sidewalk next to the street.
		PlayerScript.playerPosition_InGrid= new Vector2(PlayerScript.playerPosition_InGrid.x, 
		                                   				PlayerScript.playerPosition_InGrid.y-1);


		//Show Sprite.
		GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>().enabled=true;

		//Updating game state.
		GameManagerScript.gameManager.nextTurn(true);

		//Stop Selector.
		UIupdateScript.myUI.stopRestaurantSelectorUI();
		//Show Restaurant Message again.
		UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.BACK_IN_STREET);



	}

	//Function called when the user attempts to purchase an item in the store.
		//(Using the space bar).
	public void attemptPurchase()
	{
		//If you can afford the item,
			//play affirmative sound, substract the money.
			//update attributes and UI according to food eaten.
			//Go to sequence for the rest of the actions:
					//turn off the selector, show an affirmative text message
					//delay for a moment, then repeat menu.

		//If you can't afford the item,
			//play error sound.
			//turn off the selector, show an affirmative text message
			//delay for a moment, then repeat menu.

		//itemPrices are hardcoded for now.
			float[] itemPrices= new float[3] {2.50f, 0.75f, 0.60f};

			choiceMade=true;

			//Check if the player can afford the item.
			if(currentMoney>= itemPrices[currentRestaurantSelection])
		    {
				//Yes, you can buy the item.
		     
				//substract the money.
				currentMoney -= itemPrices[currentRestaurantSelection];
				//play affirmative sound.
				playSound(soundIndexes.FOOD_BUY);
				
			print ("current Restaurant Selection is "+currentRestaurantSelection);

				//update attributes.
				if(currentRestaurantSelection==0)  //Burger
					updatePlayerAttributes(10,1);
				else if(currentRestaurantSelection==1)  //Cola
					updatePlayerAttributes(5,0);
				else if(currentRestaurantSelection==2)  //Fries
					updatePlayerAttributes(4,0);

				//update UI.
				UIupdateScript.myUI.updateAttributesInUI();

				//show Purchase Outcome (succesful) and return to store.
				StartCoroutine(showPurchaseOutcome(true));
		     
			}
			                             
			else	
			{
				//Can't buy the item, you don't have enough money.		
				//play error sound.
				playSound(soundIndexes.FOOD_BUY_FAIL);
				
				//show Purchase Outcome (succesful) and return to store.
				StartCoroutine(showPurchaseOutcome(false));
				
			}                        
		                             
		                             
		                             
	}

	//This method updates the status message after an item purchase attempt.
	IEnumerator showPurchaseOutcome(bool isPurchaseSuccesful)
	{
		//turn off the selector, show an affirmative (or negative) text message
		//delay for a moment, then repeat menu.

		UIupdateScript.myUI.stopRestaurantSelectorUI();

		if(isPurchaseSuccesful)
			UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.BOUGHT_FOOD);
		else
			UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.BUY_FAIL);

		yield return new WaitForSeconds(restaurantMsgDelay);

		//then repeat the menu.
			//Restart Selector.
			UIupdateScript.myUI.startRestaurantSelectorUI();
			//Show Restaurant Message again.
			UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.RESTAURANT);

		choiceMade=false;
	}

//-----One last method.-----

	public void updatePlayerAttributes(int hungerOffset, int healthOffset)
	{
		//You can substract and add hunger/health values with this method.
		//Note:  Hunger is 0-20, Health is 0-5 now.

		currentHunger +=hungerOffset;
		currentLife += healthOffset;

		//Check resulting values are within range.


		//Upper Bounds Check for player variables.
		if(currentHunger > maxHunger)
			currentHunger=maxHunger;
		if(currentLife > maxLife)
			currentLife=maxLife;

		//Bottom Bounds Check for the player variables.
		if(currentHunger<0)
			currentHunger=0;
		if(currentLife<0)
			{		
				currentLife=0;
				runGameOver();
				print ("Special Game Over, you ate something bad.");
			}

	}


}
