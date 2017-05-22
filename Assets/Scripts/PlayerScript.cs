using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	//This Script manages mostly  movement, and state changes based on movement.
		//Attribute changes are handled in another script (game manager script in this case).

		//The script could be rewritten to interact with the other game objects more formally
			//(As it is now, it manages a lot of game logic)

	//Sprite map movement and graphics logic variables.
		float moveXGap= 1.863f;
		float moveYGap= 1.264f;
		bool isFacingRight=true;
		

	//Enum flags for player movement.
	enum playerMove 
	{
		UP,
		DOWN,
		LEFT,
		RIGHT
	};


	//Grid logic.
		public static bool canMove=true;

		//Enum that tracks what is contained in the current square on the grid.
		public enum UnitState 
		{
			
			EMPTY,
			HAS_PERSON,
			HAS_CAR,
			BLOCKED,
			RESTAURANT,
			DUMPSTER,
			REST_AREA,

			//These are other States for Message Passing.
			START,
			OUT_OF_BOUNDS,
			SPECIAL_END,
			BOUGHT_FOOD,
			BUY_FAIL,
			BACK_IN_STREET,
			GO_TO_RESTAURANT,
			RESTAURANT_CLOSED,
			DUMPSTER_FOUND_GOOD,
			DUMPSTER_FOUND_BAD,
			DUMPSTER_EMPTY,
			CANT_BEG,
			CAN_BEG,
			BEG_BROKE,
			BEG_FAIL,
			BEG_WIN

		};

		//This enumeration keeps track of the results of a player movement.
		enum moveResult 
		{
			MOVE_DEFAULT,
			MOVE_NEXT_TO_CAR,
			MOVE_TO_SPECIAL,
			OUT_OF_BOUNDS,
			BLOCKED_SPACE
			
		};

		int MAX_COLS=6;
		int MAX_ROWS=4;

		//The player's position in the grid.
		public static Vector2 playerPosition_InGrid;

		//This 2d array contains the state for all spaces on the grid.
		public static UnitState[][] mapGrid;	//Logic map for game locations.

		//A flag to determine if there's a driver that the player can beg money to.
		public static bool canBeg=false;	


	//The game's start function.
	void Start () {
	
		//Setting starting Position in Grid.
		playerPosition_InGrid= new Vector2(3,2);  //Y=0 from the top of the screen.

		//Initializing mapGrid.
			//The first 3 rows are for: Top lane, Second Lane, Sidewalk.
			//The last row is for the Special locations: where the restaurant, dumpster and tree are located.
		mapGrid = new UnitState[][]{ 
					
					new UnitState[] {UnitState.EMPTY, UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY}, 
					new UnitState[] {UnitState.EMPTY, UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY}, 
					new UnitState[] {UnitState.EMPTY, UnitState.EMPTY,UnitState.EMPTY,UnitState.EMPTY,UnitState.BLOCKED,UnitState.EMPTY}, 
					new UnitState[] {UnitState.RESTAURANT, UnitState.RESTAURANT,UnitState.RESTAURANT,
									 UnitState.DUMPSTER,UnitState.REST_AREA, UnitState.REST_AREA} 
								
				  };

	}
	
	// The update function checks for user Input within different modes 
		//(standard movement, inside the store, or next to a car, begging)
	void Update () {
	

		if(canMove)
			checkInput();

		else if(GameManagerScript.isPrompted && !GameManagerScript.choiceMade)
			checkInput_StoreMode();

	
		if(canBeg)
			if(Input.GetKeyDown(KeyCode.Space))
				attemptToBeg();


	}  //end of Update()


	//Checking user choices once inside the restaurant.
	void checkInput_StoreMode()
	{

		//Move the cursor between choices.
		if(Input.GetKeyDown(KeyCode.LeftArrow) )
			GameManagerScript.gameManager.updateRestaurantSelection(KeyCode.LeftArrow);
			
		else if(Input.GetKeyDown(KeyCode.RightArrow) )
		   GameManagerScript.gameManager.updateRestaurantSelection(KeyCode.RightArrow);

        else if(Input.GetKeyDown(KeyCode.UpArrow) )
	        GameManagerScript.gameManager.updateRestaurantSelection(KeyCode.UpArrow);

        else if(Input.GetKeyDown(KeyCode.DownArrow) )
	        GameManagerScript.gameManager.updateRestaurantSelection(KeyCode.DownArrow);


		//Check if the player attempts to purchase something. 
			//The space key is the action button in this case.
		else if(Input.GetKeyDown(KeyCode.Space))
		{
			if( GameManagerScript.currentRestaurantSelection!= 
			   (GameManagerScript.maxRestaurantSelections)  )
				GameManagerScript.gameManager.attemptPurchase();
			
			else GameManagerScript.gameManager.leaveStore();
			
		}

	}

	//Checking the player's choices of movement while out on the street.
	void checkInput()
	{


				if(Input.GetKeyUp(KeyCode.LeftArrow))
				{
					
						if(isFacingRight)
						{
							//Flip the character sprite, if the player changes horizontal direction.
							transform.localScale = new Vector3
										(transform.localScale.x *-1, transform.localScale.y, transform.localScale.z);
							
							isFacingRight=false;
						}

						//Checking if the player's move request is acceptable,
							// by checking with our state grid and game logic.
						moveResult moveCheckResult= updatePlayerInGridAndReturnStatus(playerMove.LEFT);	
						
						//if the player is able to move, update the player's position in the display.
						if(moveCheckResult== moveResult.MOVE_DEFAULT)
							moveCharacter(playerMove.LEFT);
						
						//else if the move request would go out of bounds:
							 //logic and state is not updated, plus a sound (a bump) is played.
						else if(moveCheckResult== moveResult.OUT_OF_BOUNDS)
							GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.WALL_HIT);	
					
				}
				
				else if(Input.GetKeyUp(KeyCode.RightArrow))
				{
					

						if(!isFacingRight)
						{
							//Flip the character sprite, if the player changes horizontal direction.
							transform.localScale = new Vector3
								(transform.localScale.x *-1, transform.localScale.y, transform.localScale.z);
							
							isFacingRight=true;
						}

						moveResult moveCheckResult= updatePlayerInGridAndReturnStatus(playerMove.RIGHT);	
						
						if(moveCheckResult== moveResult.MOVE_DEFAULT)
							moveCharacter(playerMove.RIGHT);
						
						//else //do nothing, out of bounds.
						else if(moveCheckResult== moveResult.OUT_OF_BOUNDS)
							GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.WALL_HIT);	

					
				}
				
				
				else if(Input.GetKeyUp(KeyCode.UpArrow))
				{
					
					moveResult moveCheckResult= updatePlayerInGridAndReturnStatus(playerMove.UP);	
					
					if(moveCheckResult== moveResult.MOVE_DEFAULT)
						moveCharacter(playerMove.UP);
					
					//else //do nothing, out of bounds.
					else if(moveCheckResult== moveResult.OUT_OF_BOUNDS)
						GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.WALL_HIT);	

				}
				
				//Now checking for movement into special regions.
					//If the player moves down, there's the possibility that
					//he/she may be entering the restaurant, dumpster, or rest area.
						//If this happens, the move request will return a special state.
				else if(Input.GetKeyUp(KeyCode.DownArrow))
				{
					
					moveResult moveCheckResult= updatePlayerInGridAndReturnStatus(playerMove.DOWN);	
					
					if(moveCheckResult== moveResult.MOVE_DEFAULT)
						moveCharacter(playerMove.DOWN);
					
					else if(moveCheckResult== moveResult.MOVE_TO_SPECIAL)
					{
						//Moving to the special area the player is currently in.
						moveCharacter_Special(
							mapGrid[(int)playerPosition_InGrid.y][(int)playerPosition_InGrid.x]
					                      );	

					}

					//else //do nothing, out of bounds.
					else if(moveCheckResult== moveResult.OUT_OF_BOUNDS) 
						GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.WALL_HIT);	

					
				}

	}


	//This function attemps to update the player's state, as well as its position on the grid.
		//After attempting this change in the game logic and state, a result of the request will be returned,
		//in the form of a moveResult enumeration.
	moveResult updatePlayerInGridAndReturnStatus(playerMove whereTo)
	{
		//Creating our results variable.
		moveResult currentMoveResult= moveResult.MOVE_DEFAULT;

	//A. First, check if the move is out of bounds. 

		if(whereTo==playerMove.LEFT)		//Left bound check.
		{
			if (playerPosition_InGrid.x<1)
				{	
					currentMoveResult= moveResult.OUT_OF_BOUNDS;
					UIupdateScript.myUI.updateMessage(UnitState.OUT_OF_BOUNDS);

				}
			
			else if(mapGrid[(int)playerPosition_InGrid.y][(int)(playerPosition_InGrid.x-1)] == UnitState.BLOCKED )
				{
					currentMoveResult= moveResult.OUT_OF_BOUNDS;
					UIupdateScript.myUI.updateMessage(UnitState.BLOCKED);
				}

			else 							//update grid to go Left.
				{	
					playerPosition_InGrid= new Vector2(playerPosition_InGrid.x-1, 
				                                   playerPosition_InGrid.y);

					//Updating game state.
					//print ("updatePlayerInGrid():left calls next Turn");
						GameManagerScript.gameManager.nextTurn(false);

				}

		}

		else if(whereTo==playerMove.RIGHT)	//Right bound check.
		{	
			if ( playerPosition_InGrid.x > (MAX_COLS-2) )
				{	
					currentMoveResult= moveResult.OUT_OF_BOUNDS;
					UIupdateScript.myUI.updateMessage(UnitState.OUT_OF_BOUNDS);
					
				}

			else if (mapGrid[(int)playerPosition_InGrid.y][(int)(playerPosition_InGrid.x+1)] == UnitState.BLOCKED )
				{
					currentMoveResult= moveResult.OUT_OF_BOUNDS;
					UIupdateScript.myUI.updateMessage(UnitState.BLOCKED);
				}
		
			else 							//update grid to go Right.
				{
					playerPosition_InGrid= new Vector2(playerPosition_InGrid.x+1, 
				                                   playerPosition_InGrid.y);
		
					//Updating game state.
					//print ("updatePlayerInGrid():right calls next Turn");

						GameManagerScript.gameManager.nextTurn(false);


				}
		}

		else if(whereTo==playerMove.UP)
		{	
			if (playerPosition_InGrid.y <1 )
				{	
					currentMoveResult= moveResult.OUT_OF_BOUNDS;
					UIupdateScript.myUI.updateMessage(UnitState.OUT_OF_BOUNDS);
					
				}

			else 							//update grid state to go Up.
				{
					playerPosition_InGrid= new Vector2(playerPosition_InGrid.x, 
				                                   playerPosition_InGrid.y-1);
			
					//Updating game state.
					//print ("updatePlayerInGrid():up calls next Turn");

						GameManagerScript.gameManager.nextTurn(false);

				}
		}

		//B. Next, check if the move is to a special area.
		else if(whereTo==playerMove.DOWN)
		{	

			if (mapGrid[(int)(playerPosition_InGrid.y+1)][(int)playerPosition_InGrid.x] == UnitState.BLOCKED )
			{
				currentMoveResult= moveResult.OUT_OF_BOUNDS;
				UIupdateScript.myUI.updateMessage(UnitState.BLOCKED);
	
			}

			else{
					//update grid state to go Down.
					playerPosition_InGrid= new Vector2(playerPosition_InGrid.x, 
				                                   playerPosition_InGrid.y+1);
			
					if (playerPosition_InGrid.y >2 )
						currentMoveResult= moveResult.MOVE_TO_SPECIAL;	//going to one of 3 special Areas.

					else 	//Updating game state.
						{
							GameManagerScript.gameManager.nextTurn(false);
							//print ("updatePlayerInGrid():down calls next Turn");

						}
			}

		}

		//End of updatePlayerInGridAndReturnStatus().
		return currentMoveResult;

	}


	//This function moves the character within the grid,
		//once the movement request has been accepted.
	void moveCharacter(playerMove whereTo)
	{
		//If the player is moving to or moving in the street lanes,
			//Check if the player is inside a grid space that has a car.
			//(The player can then be able to beg)
		if(playerPosition_InGrid.y<2)
			checkForCar();

		Vector3 newPosition;

		if(whereTo==playerMove.LEFT)
		{
			newPosition= new Vector3(
				transform.position.x-moveXGap,
				transform.position.y, 
				transform.position.z	);

		}

		else if(whereTo==playerMove.RIGHT)
		{
			newPosition= new Vector3(
				transform.position.x+moveXGap,
				transform.position.y, 
				transform.position.z	);
			
		}

		else if(whereTo==playerMove.UP)
		{
			newPosition= new Vector3(
				transform.position.x,
				transform.position.y+moveYGap, 
				transform.position.z	);
			
		}

		else  //Going down.
		{
			newPosition= new Vector3(
				transform.position.x,
				transform.position.y-moveYGap, 
				transform.position.z	);

		}

		transform.position=newPosition;

	}  //end of Move Character.

	//This method updates the game logic and the display of the character,
		//so as to appear in the special area.
	void moveCharacter_Special(UnitState whatSpecialArea)
	{
		canMove=false;

		//Playing sound here for now. Will change later.
		GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.WALK);	


		if(whatSpecialArea== UnitState.REST_AREA)
		{
			//Updating UI.
			UIupdateScript.myUI.updateMessage(UnitState.REST_AREA);

			//Testing this.
			StartCoroutine(restSequence());

		}

		else if(whatSpecialArea== UnitState.DUMPSTER)
		{
			//Updating UI.
			UIupdateScript.myUI.updateMessage(UnitState.DUMPSTER);

			//running the special Sequence.
			StartCoroutine(dumpsterSequence());

		}

		else if(whatSpecialArea== UnitState.RESTAURANT)
		{

			//Updating UI.
			UIupdateScript.myUI.updateMessage(UnitState.GO_TO_RESTAURANT);

			//start the openStoreDoorSequence.
			StartCoroutine(beginStoreSequence());

		}



	}


//This method manages the player's interaction with the fast-food restaurant. 
	//(Visually speaking. However, the game logic is delegated to the gameManager)
	IEnumerator beginStoreSequence()
	{
		//First, we hide the player sprite visible in the street.
		gameObject.GetComponent<SpriteRenderer>().enabled=false;
		
		//Then, we show a different player sprite, one in the restaurant area.
		GameObject.FindGameObjectWithTag("restSprite").GetComponent<SpriteRenderer>().enabled=true;

		//Third, we check if the store is open.	
			if(!GameManagerScript.isStoreOpen)
			{
					//Store is closed, turn back.

					//Displaying message.		
					UIupdateScript.myUI.updateMessage(UnitState.RESTAURANT_CLOSED);

					//Playing error sound.
					GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.FOOD_BUY_FAIL);	

					//Perform a delay.
					yield return new WaitForSeconds(1.2f);

					//Hiding the restaurant sprite.
					GameObject.FindGameObjectWithTag("restSprite").GetComponent<SpriteRenderer>().enabled=false;

					//Showing the normal sprite.
					gameObject.GetComponent<SpriteRenderer>().enabled=true;
					
					//Showing the end message.
					UIupdateScript.myUI.updateMessage(UnitState.SPECIAL_END);			
					
					//Update player to go Up, back to the street area.
					playerPosition_InGrid= new Vector2(playerPosition_InGrid.x, 
					                                   playerPosition_InGrid.y-1);
					
					//Updating game state.
					GameManagerScript.gameManager.nextTurn(true);
					
					//The sequence ends, and the player can now move again.
					canMove=true;

			}

			else
			{

				//The store is open, so let's enter.
				yield return new WaitForSeconds(0.6f);

				//The player opens the door.
				//This is performed by changing the sprite of the restaurant door,
				
				//Resulting in a crude animation.

					//Showing the half-open door sprite.
					GameObject.FindGameObjectWithTag("half_open").GetComponent<SpriteRenderer>().enabled=true;

						yield return new WaitForSeconds(0.4f);
					
					//Showing the open door sprite.
					GameObject.FindGameObjectWithTag("open").GetComponent<SpriteRenderer>().enabled=true;

						yield return new WaitForSeconds(0.4f);

					//Finally, the player enters the restaurant through the open door.
						//To perform this, we hide the door and player sprites.
					GameObject.FindGameObjectWithTag("restSprite").GetComponent<SpriteRenderer>().enabled=false;
					GameObject.FindGameObjectWithTag("half_open").GetComponent<SpriteRenderer>().enabled=false;
					GameObject.FindGameObjectWithTag("open").GetComponent<SpriteRenderer>().enabled=false;


				//Updating the UI.
				UIupdateScript.myUI.updateMessage(UnitState.RESTAURANT);

				//Now going into the store game mode.
				GameManagerScript.gameManager.testShop();

			}
	
	}	

//This method manages the player's interaction with the dumpster. 
	//(Visually speaking. However, the game logic is delegated to the gameManager)
	IEnumerator dumpsterSequence()
	{

		//Checking if there is any trash to collect in the dumpster.
		if(GameManagerScript.isTrashCollected || GameManagerScript.dumpsterState==0)
		{

			//Do a short sequence, no trash to Collect.

			//Play sound.
			GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.FOOD_BUY_FAIL);	


			//Hiding the player sprite on the street.
			gameObject.GetComponent<SpriteRenderer>().enabled=false;
			
			//showing the player's sprite in the dumpster.
			GameObject.FindGameObjectWithTag("noTrash").GetComponent<SpriteRenderer>().enabled=true;

			//showing the status message.
			UIupdateScript.myUI.updateMessage(UnitState.DUMPSTER_EMPTY);

			//updating the player attributes in the game logic and on the screen.
			UIupdateScript.myUI.updateAttributesInUI();
			
			//delay.
			yield return new WaitForSeconds(1.2f);

		
			//hiding the restSprite.
			GameObject.FindGameObjectWithTag("noTrash").GetComponent<SpriteRenderer>().enabled=false;


		}

		else{

				//There is trash to collect at the dumpster sprite.
					//We will now perform an animation, and update the game logic + variables.

				//Hiding the player sprite on the street.
				gameObject.GetComponent<SpriteRenderer>().enabled=false;
				
				//showing the player sprite at the dumpster.
				GameObject dumpSprite= GameObject.FindGameObjectWithTag("forage");
				dumpSprite.GetComponent<SpriteRenderer>().enabled=true;
				
				//Preparing to loop the player sprite at the dumpster.
				int flipSpriteHowManyTimes= 6;
				int flipSpriteCount=0;


				while(flipSpriteCount < flipSpriteHowManyTimes)
				{

					//flip the sprite.
					dumpSprite.transform.localScale = 
						new Vector3( dumpSprite.transform.localScale.x*-1, 1, 1);

						//delay.
						yield return new WaitForSeconds(0.3f);

					//count sprite.
					flipSpriteCount++;

				}


				//End of animation sequence.

				//Update the game logic, attempt to salvage something from the dumpster.
				GameManagerScript.gameManager.attemptToCollectDumpster();

					yield return new WaitForSeconds(1.2f);


				//hiding the player sprite at the dumpster.
				dumpSprite.GetComponent<SpriteRenderer>().enabled=false;
					
		}		//end of else loop.


		//After this dumpster forage sequence, 
			//show the normal player sprite and update the logic.

			//Showing the player sprite at the street.
			gameObject.GetComponent<SpriteRenderer>().enabled=true;

			//Updating the status message at the end of this sequence.
			UIupdateScript.myUI.updateMessage(UnitState.SPECIAL_END);
			
			
			//Update the player's position to go Up.
			playerPosition_InGrid= new Vector2(playerPosition_InGrid.x, 
			                                   playerPosition_InGrid.y-1);
			
			
			//Update the game state.
				GameManagerScript.gameManager.nextTurn(true);

			//Return control of the player sprite to the player.
			canMove=true;
		
	}	

//This method manages the player's resting sequence in the area under the tree. 
	//(Visually speaking. However, the game logic is delegated to the gameManager)
	IEnumerator restSequence()
	{

		//Hiding the player sprite on the street.
		gameObject.GetComponent<SpriteRenderer>().enabled=false;

		//Showing the player Sprite resting under the tree.
		GameObject.FindGameObjectWithTag("sleep").GetComponent<SpriteRenderer>().enabled=true;

			yield return new WaitForSeconds(1.5f);

			GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.EATING);	

			yield return new WaitForSeconds(1f);


		//Showing a status message.
		UIupdateScript.myUI.updateMessage(UnitState.SPECIAL_END);

	
		//Update the player's position to go Up.
		playerPosition_InGrid= new Vector2(playerPosition_InGrid.x, 
		                                   playerPosition_InGrid.y-1);



		//Updating Player values. Recovering the character's attributes.
		GameManagerScript.gameManager.updatePlayerAttributes(0,5);


			//Updating game state.
			GameManagerScript.gameManager.nextTurn(true);


		//Update the UI.
		UIupdateScript.myUI.updateAttributesInUI();


		//Hiding the resting player sprite.
		GameObject.FindGameObjectWithTag("sleep").GetComponent<SpriteRenderer>().enabled=false;


		//Showing the player sprite on the street.
		gameObject.GetComponent<SpriteRenderer>().enabled=true;

		canMove=true;

	}	


//These methods manages the player's interaction with cars on the street. 
	//Checks if the player is on the same grid space as a car, 
	//and thus there's a chance to beg for money.
	void checkForCar()
	{
		GameManagerScript.gameManager.gameObject.GetComponent<CarsManagerScript>().checkIfPlayerCanBeg();

		}

	void attemptToBeg()
	{
		GameManagerScript.gameManager.gameObject.GetComponent<CarsManagerScript>().tryBegging();		
	}

}
