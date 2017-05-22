using UnityEngine;
using System.Collections;

public class CarsManagerScript : MonoBehaviour {

	//This script manages the movement, attributes and display of car objects in the game.

	//Variables that count the current game turn
		//(Every player movement constitutes a turn).
	public int turnsSinceLastRelease=0;
		//variables that change the timing of the carManager's vehicle release.
		int maxReleaseWait=5;
		public int currentReleaseWait=1;

	//Car management and deployment variables.
	public int howManyCars;  //amount of cars in the game.
	int numberOfLanes=2;
	int typesOfCars=4;
	int carsOut=0;

	//Displaying the car objects with the proper offset.
	float moveXGap= 1.863f;
	float moveYGap= 1.264f;

	//Car spawn positions.
	Vector3 bottomSpawnPos;
	Vector3 topSpawnPos;
	Vector3 leftOfMapPos;


	//This array keeps track of all the car objects.
	public GameObject[] carObjectsArray;

	//This variable keeps track of which spaces in the grid are occupied by cars.
		//These grid coordinates are then translated into onscreen positions,
		//by translating these with offset data and spawnPosition data.
	public Vector2 [] carPositionsInGrid;

	//Managing the amount of money that each car driver may have.
	public float [] moneyAmounts;
	int beggableCar=-1;

//-------CarManager methods.------

	//Initializing the carManager and its variables.
	void Awake () {

		//Setting up the number of cars.
		howManyCars=5;

		//Setting up different amounts of money the drivers may have.
		moneyAmounts= new float[]{ 0f, 0.5f, 0.25f, 0.10f, 0.50f, 1f, 0.75f, 0.31f};

		//Setting Spawn Position data.
		bottomSpawnPos= new Vector3(-4.72f, 2.73f, -0.5f);
		topSpawnPos= new Vector3(-4.72f, 3.994f, -0.5f);

		leftOfMapPos= new Vector3(-8f, 2.73f, -0.5f);

		//Initializing the carObject+carPosition arrays.
		carObjectsArray= new GameObject[howManyCars];
		carPositionsInGrid= new Vector2[howManyCars];

		//Instantiating the car objects.
		for (int i=0; i<carObjectsArray.Length; i++)
			{
				carObjectsArray[i]= Instantiate(Resources.Load("carObject", typeof(GameObject))) as GameObject;
				carObjectsArray[i].transform.position= leftOfMapPos;
				
				carObjectsArray[i].name= "carObject"+i;

				//Initializing the car objects' positions.
					//They are not yet visible in the game scene.
					//-1,-1 means the Car position is off the grid.
				carPositionsInGrid[i]= new Vector2(-1,-1);			

		}

		//Assigning money amounts to each car object.
		for (int i=0; i<carObjectsArray.Length; i++)
		{
			//assign Money amount
			print ("assigning Money amount for "+i);
			carObjectsArray[i].GetComponent<carProperties>().money=getRandomFunds();
			
		}
			
	}

	//Update function that runs at the start of every turn.
	public void updateCarsManager()
	{
		//print ("Updating cars for turn "+GameManagerScript.currentTurn+":");

		//1. Advance all the cars already on the road to the right, by one grid space.
		for (int i=0; i<carObjectsArray.Length; i++)
		{
			if( carObjectsArray[i].GetComponent<carProperties>().isOnRoad )
				moveCar(i);

		}

		//2.Attempting to release a car.
			//If this is the first turn in game, generate release wait.
			if (GameManagerScript.currentTurn<2)
				currentReleaseWait= 2;//Random.Range(1,maxReleaseWait);
			else if(GameManagerScript.currentTurn<20)
				maxReleaseWait= 7;//Random.Range(1,maxReleaseWait);

		turnsSinceLastRelease++;

		//Every 1 to X turns, depending on difficulty,
			//release a Car.
		if( turnsSinceLastRelease >= currentReleaseWait)
		{
			bool isCarReleased=false;

			//Release a car out onto the street, coming from the left side of the screen.
			for (int i=0; i<carObjectsArray.Length; i++)
			{
				if( carObjectsArray[i].GetComponent<carProperties>().isOnRoad == false )
					{ releaseCar(i);
						break;
					}
			}

			print ("updateCarsManager(): Resetting car release values.");

			//Reset counter and generate a new Release Wait.
				turnsSinceLastRelease=0;
				currentReleaseWait= Random.Range(1,maxReleaseWait); 
		
		}


	}

	//Function that implements the car movement once out in the field.
		//Verifies if the car can move 
		//(is it in front of a red traffic light? is there a car directly in front of it?)
	void moveCar(int index)
	{

		//1.Checking if the car is out of the map's bounds.
			//If we are, reset the car. 

		//2.Check if another car is in front of the car, or if the traffic light is stopped.
			//2b.Check if player is in front of car.

		//3.Else, increment X position and X vaule in Grid.

		//4.Update the grid's and car's state.

		bool carCanMove=false;

		if( carPositionsInGrid[index].x== 5 )  //5 is the width of the mapGrid.
			resetCar(index);

		//If the car is behind a red light, or is behind a car behind a red light, 
			//do not update movement, stop.
		else if (GameManagerScript.currentTrafficLight == GameManagerScript.TrafficState.RED &&
			 carPositionsInGrid[index].x == 3 ) 
		{}
		else if	(PlayerScript.mapGrid[(int)carPositionsInGrid[index].y][(int)carPositionsInGrid[index].x+1]==
		 			PlayerScript.UnitState.HAS_CAR )
		{}

		//Else, move one grid space to the right.
		else
		{
			print ("car "+index+" should be moving.");
			carCanMove=true;

			//Grid value.
			carPositionsInGrid[index]= new Vector2(

										carPositionsInGrid[index].x+1,
										carPositionsInGrid[index].y
										
										);

			//Position value.
			carObjectsArray[index].transform.position=
				new Vector3( carObjectsArray[index].transform.position.x + moveXGap,
				            carObjectsArray[index].transform.position.y,
			    	        carObjectsArray[index].transform.position.z );

		}

		//Update the game grid with the position of the cars.
		if(carCanMove)
			updateCarInGrid(index);

	}

	void updateCarInGrid(int index)
	{

		//New Position defined as occupied.
		PlayerScript.mapGrid[(int)carPositionsInGrid[index].y][(int)carPositionsInGrid[index].x]= PlayerScript.UnitState.HAS_CAR;

		//Previous Position defined as free.
		PlayerScript.mapGrid[(int)carPositionsInGrid[index].y][(int)carPositionsInGrid[index].x-1]= PlayerScript.UnitState.EMPTY;


	}

	//This method spawns a car object on the upper left corner of the screen.
		//It will spawn in either the 1st or 2nd lane.
	void releaseCar(int index)
	{

		int stateOfLanes=0;
			//0 means top lane is open (at the spawn point).
			//1 means bottom lane is open (at the spawn point).
			//-1 means both lanes are occupied at the spawn point.

		//choose a Lane to spawn from.
		int whichLane= Random.Range (0,2);
		int otherLane;
			if(whichLane==0) otherLane=1;
			else otherLane=0;

		//check that one or two of the lanes are not occupied.
		if(PlayerScript.mapGrid[whichLane][0]== PlayerScript.UnitState.EMPTY)
		{}
		else if(PlayerScript.mapGrid[otherLane][0]== PlayerScript.UnitState.EMPTY)
			whichLane= otherLane;
		else whichLane=-1;


		//If there is an open lane:
		if (whichLane != -1)
		{

			print ("releasing carObject"+index);

			//Set the car's spawnPosition based on Lane.
			if(whichLane==0)
				carObjectsArray[index].transform.position= topSpawnPos;
			else if(whichLane==1)
				carObjectsArray[index].transform.position= bottomSpawnPos;

			//Set the car's grid Coordinates.
			carPositionsInGrid[index]= new Vector2(0,whichLane);

			//Update the game grid.
			PlayerScript.mapGrid[whichLane][0]= PlayerScript.UnitState.HAS_CAR;

			//Setting true the boolean isInMap
			carObjectsArray[index].GetComponent<carProperties>().isOnRoad=true;
		}

	}

	void resetCar(int index)
	{
		//Resetting Positions.
		carObjectsArray[index].transform.position= leftOfMapPos;

		//Resetting Space in grid.
		if( carPositionsInGrid[index].y!=-1)
			PlayerScript.mapGrid[(int)carPositionsInGrid[index].y][(int)carPositionsInGrid[index].x]= 
				PlayerScript.UnitState.EMPTY;

		//Resetting position in Grid.
		carPositionsInGrid[index]= new Vector2(-1,-1);	

		
		//Resetting hasGivenMoney Value.
		carObjectsArray[index].GetComponent<carProperties>().hasGivenMoney=false;
		//Resetting this car's fund amount.
		carObjectsArray[index].GetComponent<carProperties>().money=getRandomFunds();
		//Resetting this car's boolean isInMap (it's not actively in the map, no).
		carObjectsArray[index].GetComponent<carProperties>().isOnRoad=false;


		//Resetting the car's appearance.
		carObjectsArray[index].GetComponent<carProperties>().randomCarColor();

		//one more thing: resetting this car's driver's probability to give money.
			//This variable should be called willDriver_giveMoney, instead of willDriverBeg.
		int willDriverBeg=0;
		int chance= Random.Range (0,6);
		if(chance==0)
			willDriverBeg= 0;
		else if(chance<4)
			willDriverBeg= 1;
		else if(chance<6)
			willDriverBeg= 2;

		carObjectsArray[index].GetComponent<carProperties>().willDriverBeg= willDriverBeg;

		
	}

	//Reset all the car's properties.
	public void resetAllCars()
	{
		for (int i=0; i<carObjectsArray.Length; i++)
			resetCar(i);
			
	}

	//This function returns a random selection of funds (as stored in the moneyAmounts array.)
		//This is used to assign a random money amount to each car's driver.
	float getRandomFunds()
	{

		int chance= Random.Range (0,2);

		int maxAmount=moneyAmounts.Length;
		if(GameManagerScript.currentTurn>GameManagerScript.turnsPerDay)
			maxAmount=6;

		int chance1=Random.Range(0,moneyAmounts.Length);
		int chance2=Random.Range(0,3);

		if(chance==0)
			return moneyAmounts[chance1];
		else 
			return moneyAmounts[chance1] + moneyAmounts[chance2];

	}

//---Methods used to perform the player's begging action on a particular car.
	public void checkIfPlayerCanBeg()
	{

		//1. Checking the following  conditions first.
			//if the traffic light is not red or you're past traffic light xPos, 
				//do the sequence to display CANT BEG.

		//check if Player is in same grid position of any one car.


		if ( PlayerScript.playerPosition_InGrid.x>3)
		{
			//don't do anything.

		}
		
		else{

			//If the car is in the right position, the player may be able to beg.
				//Check player position with the cars' positions.
				beggableCar=-1;

				for (int i=0; i<carPositionsInGrid.Length; i++)
				{
					   //If the current car's position is the same as the player's,
					   		//and if the driver hasn't already been asked money,
							//the player can perform the begging.
					   if ( ( carPositionsInGrid[i].x == PlayerScript.playerPosition_InGrid.x &&
				  	   carPositionsInGrid[i].y == PlayerScript.playerPosition_InGrid.y )  && 
				    	(!carObjectsArray[i].GetComponent<carProperties>().hasGivenMoney ) )
						{
							
							//If the traffic light is red, this means the car has stopped, 
								//thus the player can beg for money.
							if(GameManagerScript.currentTrafficLight == GameManagerScript.TrafficState.RED)
					   			{
									beggableCar=i;
									PlayerScript.canBeg=true;
									UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.CAN_BEG);
									break;
								}

							else{
									//Else, the player can't beg, display can't beg message (and play a sound).
									GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.FOOD_BUY_FAIL);	
									PlayerScript.canBeg=false;	//added this line, as it seemed to be missing.
									UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.CANT_BEG);
								}

						}
					
					else PlayerScript.canBeg=false;

				}
				
				

			}
		
	}


	//Method that performs the begging action of the player, on a particular car's driver.
	public void tryBegging()
	{
		//Obtaining the driver's begging probability.
		int begProbability= carObjectsArray[beggableCar].GetComponent<carProperties>().willDriverBeg;

		//Then, determining begging results based on the probability.

		if (begProbability==2)
		{
			//success.
			beggingResultSequence(true);
		}

		if(begProbability==1)
		{
			int chance= Random.Range (0,5);

			if(chance<5)	//success.
				beggingResultSequence(true);

			else //fail.
				beggingResultSequence(false);
		}

		else 
		{	//fail.
			beggingResultSequence(false);		
		}

	}


	//This method updates the display and player's funds with the begging outcome.
	void beggingResultSequence(bool begSuccesful)
	{

		if (begSuccesful)
		{

			float beggedFunds=carObjectsArray[beggableCar].GetComponent<carProperties>().money;

			//Check if the driver does not have money.
			if(beggedFunds <= 0.01f)
				{
					//Update the status message, the driver's broke.
					UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.BEG_BROKE);

					//Play Sound.
					GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.FOOD_BUY_FAIL);	

				}

			//Else, begging was succesful.
			else {

					//Update fund display and message.
					UIupdateScript.moneyReceived=beggedFunds; 
					UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.BEG_WIN);
					
					//Play Sound.
					GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.FOOD_BUY);	

					//Add attribute.
					GameManagerScript.currentMoney += beggedFunds;
					UIupdateScript.myUI.updateAttributesInUI();

				}

		}

		else
		{

			//Update the status message, begging failed.
			UIupdateScript.myUI.updateMessage(PlayerScript.UnitState.BEG_FAIL);
			
			//Play Sound.
			GameManagerScript.gameManager.playSound(GameManagerScript.soundIndexes.WALL_HIT);	

		}

		//The current car has been begged to, set property to true.
		carObjectsArray[beggableCar].GetComponent<carProperties>().hasGivenMoney=true;
		//reset the Player's variable.
		PlayerScript.canBeg=false;

	}

}
