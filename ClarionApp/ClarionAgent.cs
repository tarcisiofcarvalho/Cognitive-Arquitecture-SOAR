using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Core;
using Clarion.Framework.Templates;
using ClarionApp.Exceptions;
using ClarionApp.Model;
using ClarionApp;
using System.Threading;
using Gtk;


namespace ClarionApp
{
    /// <summary>
    /// Public enum that represents all possibilities of agent actions
    /// </summary>
    public enum CreatureActions
    {
        DO_NOTHING,
        ROTATE_CLOCKWISE,
        GO_AHEAD,
        GET_JEWEL, // Added by Tarcisio
        HIDE_JEWEL, // Added by Tarcisio
        EAT_FOOD, // Added by Tarcisio
        GOAL_ACHIEVED, // Added by Tarcisio
        GO_TO_JEWEL // Added by Tarcisio
        	
    }

    public class ClarionAgent
    {
        #region Constants
        /// <summary>
        /// Constant that represents the Visual Sensor
        /// </summary>
        private String SENSOR_VISUAL_DIMENSION = "VisualSensor";
        /// <summary>
        /// Constant that represents that there is at least one wall ahead
        /// </summary>
        private String DIMENSION_WALL_AHEAD = "WallAhead";
        /// <summary>
        /// Constant that represents that there is at least one jewel ahead
        /// </summary>
        private String DIMENSION_JEWEL_AHEAD = "JewelAhead";     
        /// <summary>
        /// Constant that represents that there is at least one food ahead
        /// </summary>
        private String DIMENSION_FOOD_AHEAD = "FoodAhead"; 
        /// <summary>
        /// Constant that represents that there is a jewel in view 
        /// </summary>
        private String DIMENSION_JEWEL_IN_VISION = "JewelInVision"; 
        /// <summary>
        /// Current jewel name to be get/hide
        /// </summary>
        private Thing currentJewel = null; 
        /// <summary>
        /// Current food name to be eaten 
        /// </summary>
        private Thing currentFood;        
        double prad = 0;
        #endregion

        #region Properties
		public MindViewer mind;
		String creatureId = String.Empty;
		String creatureName = String.Empty;
		int jewelGoal = 0;
		#endregion
        #region Simulation
        /// <summary>
        /// If this value is greater than zero, the agent will have a finite number of cognitive cycle. Otherwise, it will have infinite cycles.
        /// </summary>
        public double MaxNumberOfCognitiveCycles = -1;
        /// <summary>
        /// Current cognitive cycle number
        /// </summary>
        private double CurrentCognitiveCycle = 0;
        /// <summary>
        /// Time between cognitive cycle in miliseconds
        /// </summary>
        public Int32 TimeBetweenCognitiveCycles = 200;
        /// <summary>
        /// A thread Class that will handle the simulation process
        /// </summary>
        private Thread runThread;
        #endregion

        #region Agent
		private WSProxy worldServer;
        /// <summary>
        /// The agent 
        /// </summary>
        private Clarion.Framework.Agent CurrentAgent;
        #endregion

        #region Perception Input
        /// <summary>
        /// Perception input to indicates a wall ahead
        /// </summary>
		private DimensionValuePair inputWallCreatureAhead;
        /// <summary>
        /// Perception input to indicates a food ahead
        /// </summary>
		private DimensionValuePair inputFoodAhead;
        /// <summary>
        /// Perception input to indicates a jewel ahead
        /// </summary>
		private DimensionValuePair inputJewelAhead;	
        /// <summary>
        /// Perception input to indicates a jewel ahead
        /// </summary>
		private DimensionValuePair inputJewelInVision;			
		
        #endregion
        
        
        #region Action Output
        /// <summary>
        /// Output action that makes the agent to rotate clockwise
        /// </summary>
		private ExternalActionChunk outputRotateClockwise;
        /// <summary>
        /// Output action that makes the agent go ahead
        /// </summary>
		private ExternalActionChunk outputGoAhead;
        /// <summary>
        /// Output action that makes the agent to get jewel
        /// </summary>
		private ExternalActionChunk outputGetJewel;
        /// <summary>
        /// Output action that makes the agent to hide jewel
        /// </summary>
		private ExternalActionChunk outputHideJewel;
        /// <summary>
        /// Output action that makes the agent to eat food
        /// </summary>
		private ExternalActionChunk outputEatFood;
		/// <summary>
        /// Output action that stop agent due goal achieved
        /// </summary>
		private ExternalActionChunk outputGoalAchieved;	
		/// <summary>
        /// Output action that go to Jewel in vision
        /// </summary>
		private ExternalActionChunk outputGoToJewelInVision;			
        #endregion

        #region Leaflet properties and methods
	    private int leafletRemainingJewelWhite = 2;
	    private int leafletRemainingJewelRed = 1;
	    
	    private int leafletRemainingJewelBlue = 2;
	    private int leafletRemainingJewelYellow = 1;
	    private int leafletRemainingJewelMagenta = 2;
	    private int leafletRemainingJewelGreen = 1;
        
	    private List<string> jewelOutOfScope = new List<string>();
	    
	    private void processLeafletControl(String color){
	        
	        if(color.Equals("White")){
	    		if(leafletRemainingJewelWhite > 0){ 
	    			leafletRemainingJewelWhite--;
	    		}else if(isDesiredJewel(color)){
	    		    jewelOutOfScope.Add(color);
	    		}
	        }else if(color.Equals("Red")){
	    		if(leafletRemainingJewelRed > 0){
	    			leafletRemainingJewelRed--;
	    		}else if(isDesiredJewel(color)){
	    		    jewelOutOfScope.Add(color);
	    		}
	        }else if(color.Equals("Blue")){
	    		if(leafletRemainingJewelBlue > 0){
	    			leafletRemainingJewelBlue--;
	    		}else if(isDesiredJewel(color)){
	    		    jewelOutOfScope.Add(color);
	    		}
	        }else if(color.Equals("Yellow")){
	    		if(leafletRemainingJewelYellow > 0){
	    			leafletRemainingJewelYellow--;
	    		}else if(isDesiredJewel(color)){
	    		    jewelOutOfScope.Add(color);
	    		}
	        }else if(color.Equals("Magenta")){
	    		if(leafletRemainingJewelMagenta > 0){
	    			leafletRemainingJewelMagenta--;
	    		}else if(isDesiredJewel(color)){
	    		    jewelOutOfScope.Add(color);
	    		}
	        }else if(color.Equals("Green")){
	    		if(leafletRemainingJewelGreen > 0){
	    			leafletRemainingJewelGreen--;
	    		}else if(isDesiredJewel(color)){
	    		    jewelOutOfScope.Add(color);
	    		}
	        }
	    	
	    	// Update Visual Leaflet data //
	    	// Prepare leaflet list
	    	Leaflet leaflet1 = new Leaflet();
	    	leaflet1.items = new List<LeafletItem>();
	    	leaflet1.items.Add(new LeafletItem("Red", 1-leafletRemainingJewelRed, 0));
	    	leaflet1.items.Add(new LeafletItem("Green", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("Blue", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("Yellow", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("Magenta", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("White", 2-leafletRemainingJewelWhite, 0));	
	    	Leaflet leaflet2 = new Leaflet();
	    	leaflet2.items = new List<LeafletItem>();	    	
	    	leaflet2.items.Add(new LeafletItem("Red", 0, 0));
	    	leaflet2.items.Add(new LeafletItem("Green", 0, 0));
	    	leaflet2.items.Add(new LeafletItem("Blue", 2-leafletRemainingJewelBlue, 0));
	    	leaflet2.items.Add(new LeafletItem("Yellow", 1-leafletRemainingJewelYellow, 0));
	    	leaflet2.items.Add(new LeafletItem("Magenta", 0, 0));
	    	leaflet2.items.Add(new LeafletItem("White", 0, 0));
	    	Leaflet leaflet3 = new Leaflet();
	    	leaflet3.items = new List<LeafletItem>();	    	
	    	leaflet3.items.Add(new LeafletItem("Red", 0, 0));
	    	leaflet3.items.Add(new LeafletItem("Green", 1-leafletRemainingJewelGreen, 0));
	    	leaflet3.items.Add(new LeafletItem("Blue", 0, 0));
	    	leaflet3.items.Add(new LeafletItem("Yellow", 0, 0));
	    	leaflet3.items.Add(new LeafletItem("Magenta", 2-leafletRemainingJewelMagenta, 0));
	    	leaflet3.items.Add(new LeafletItem("White", 0, 0));  

	    	// Update leaflets to be showed in UI view
	    	mind.updateLeaflet(0,leaflet1);
	    	mind.updateLeaflet(1,leaflet2);
	    	mind.updateLeaflet(2,leaflet3);
	    	
	    	//Console.WriteLine("processLeafletControl: " + color);
	    	
	    }
	    
	    private void processLeafletControl(){

	    	// Update Visual Leaflet data //
	    	// Prepare leaflet list
	    	Leaflet leaflet1 = new Leaflet();
	    	leaflet1.items = new List<LeafletItem>();
	    	leaflet1.items.Add(new LeafletItem("Red", 1-leafletRemainingJewelRed, 0));
	    	leaflet1.items.Add(new LeafletItem("Green", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("Blue", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("Yellow", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("Magenta", 0, 0));
	    	leaflet1.items.Add(new LeafletItem("White", 2-leafletRemainingJewelWhite, 0));	
	    	Leaflet leaflet2 = new Leaflet();
	    	leaflet2.items = new List<LeafletItem>();	    	
	    	leaflet2.items.Add(new LeafletItem("Red", 0, 0));
	    	leaflet2.items.Add(new LeafletItem("Green", 0, 0));
	    	leaflet2.items.Add(new LeafletItem("Blue", 2-leafletRemainingJewelBlue, 0));
	    	leaflet2.items.Add(new LeafletItem("Yellow", 1-leafletRemainingJewelYellow, 0));
	    	leaflet2.items.Add(new LeafletItem("Magenta", 0, 0));
	    	leaflet2.items.Add(new LeafletItem("White", 0, 0));
	    	Leaflet leaflet3 = new Leaflet();
	    	leaflet3.items = new List<LeafletItem>();	    	
	    	leaflet3.items.Add(new LeafletItem("Red", 0, 0));
	    	leaflet3.items.Add(new LeafletItem("Green", 1-leafletRemainingJewelGreen, 0));
	    	leaflet3.items.Add(new LeafletItem("Blue", 0, 0));
	    	leaflet3.items.Add(new LeafletItem("Yellow", 0, 0));
	    	leaflet3.items.Add(new LeafletItem("Magenta", 2-leafletRemainingJewelMagenta, 0));
	    	leaflet3.items.Add(new LeafletItem("White", 0, 0));  

	    	// Update leaflets to be showed in UI view
	    	mind.updateLeaflet(0,leaflet1);
	    	mind.updateLeaflet(1,leaflet2);
	    	mind.updateLeaflet(2,leaflet3);
	    }	    

	    private bool isDesiredJewel(String color){
		    foreach (String jewel in jewelOutOfScope){
	    		if(jewel.Equals(color)){
	    			return false;
	    		}
	        }
	    	return true;
	    }
	    
        private int getJewelRemainingTotal(){	    	        
	        int result = leafletRemainingJewelWhite+
	                     leafletRemainingJewelRed+
	                     leafletRemainingJewelBlue+
	                     leafletRemainingJewelYellow+
	                     leafletRemainingJewelMagenta+
	                     leafletRemainingJewelGreen;
	        return result;
	    }
        #endregion
        
        #region Constructor
		public ClarionAgent(WSProxy nws, String creature_ID, String creature_Name)
        {
			worldServer = nws;
			// Initialize the agent
            CurrentAgent = World.NewAgent("Current Agent");
			mind = new MindViewer();
			mind.Show ();
			creatureId = creature_ID;
			creatureName = creature_Name;

            // Initialize Input Information
            inputWallCreatureAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_WALL_AHEAD);
            inputFoodAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_FOOD_AHEAD);
            inputJewelAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_JEWEL_AHEAD); 
            inputJewelInVision = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_JEWEL_IN_VISION);             
            
            // Initialize Output actions
            outputRotateClockwise = World.NewExternalActionChunk(CreatureActions.ROTATE_CLOCKWISE.ToString());
            outputGoAhead = World.NewExternalActionChunk(CreatureActions.GO_AHEAD.ToString());
            outputEatFood = World.NewExternalActionChunk(CreatureActions.EAT_FOOD.ToString());
            outputGetJewel = World.NewExternalActionChunk(CreatureActions.GET_JEWEL.ToString());
            outputHideJewel = World.NewExternalActionChunk(CreatureActions.HIDE_JEWEL.ToString());
            outputGoalAchieved = World.NewExternalActionChunk(CreatureActions.GOAL_ACHIEVED.ToString()); 
            outputGoToJewelInVision = World.NewExternalActionChunk(CreatureActions.GO_TO_JEWEL.ToString());             
            
            // Define jewels out of scope
            jewelOutOfScope.Add("Orange");
            jewelOutOfScope.Add("DarkGray_Spoiled");
            
            //Create thread to simulation
            runThread = new Thread(CognitiveCycle);
			Console.WriteLine("Agent started");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Run the Simulation in World Server 3d Environment
        /// </summary>
        public void Run()
        {                
			Console.WriteLine ("Running ...");
            // Setup Agent to run
            if (runThread != null && !runThread.IsAlive)
            {
                SetupAgentInfraStructure();
				// Start Simulation Thread                
                runThread.Start(null);
            }
        }

        /// <summary>
        /// Abort the current Simulation
        /// </summary>
        /// <param name="deleteAgent">If true beyond abort the current simulation it will die the agent.</param>
        public void Abort(Boolean deleteAgent)
        {   Console.WriteLine ("Aborting ...");
            if (runThread != null && runThread.IsAlive)
            {
                runThread.Abort();
            }

            if (CurrentAgent != null && deleteAgent)
            {
                CurrentAgent.Die();
            }
        }

		IList<Thing> processSensoryInformation()
		{
			IList<Thing> response = null;

			if (worldServer != null && worldServer.IsConnected)
			{
				response = worldServer.SendGetCreatureState(creatureName);
				prad = (Math.PI / 180) * response.First().Pitch;
				while (prad > Math.PI) prad -= 2 * Math.PI;
				while (prad < - Math.PI) prad += 2 * Math.PI;
				Sack s = worldServer.SendGetSack("0");
				mind.setBag(s);
			}

			return response;
		}

		void processSelectedAction(CreatureActions externalAction)
		{   Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			if (worldServer != null && worldServer.IsConnected)
			{
				//Console.WriteLine(externalAction);
				switch (externalAction)
				{
				case CreatureActions.DO_NOTHING:
					// Do nothing as the own value says
					break;
				case CreatureActions.ROTATE_CLOCKWISE:
					//worldServer.SendSetAngle(creatureId, 2, 0, 0);
					worldServer.SendSetAngle(creatureId, 0.5, -0.5, 0.5);
					break;
				case CreatureActions.GO_AHEAD:
					//worldServer.SendSetAngle(creatureId, 1, 1, prad);
					worldServer.SendSetAngle(creatureId, 0.5, -0.5, 0.5);
					//worldServer.SendSetAngle(creatureId, 2, 0, 0);
					break;
				case CreatureActions.GO_TO_JEWEL:
					worldServer.SendSetGoTo(creatureId,1,1,currentJewel.X1,currentJewel.Y1);
					break;
				case CreatureActions.EAT_FOOD:
					worldServer.SendEatIt(creatureId, currentFood.Name);
					break;
				case CreatureActions.GET_JEWEL:
					worldServer.SendSackIt(creatureId, currentJewel.Name);
					processLeafletControl(currentJewel.Material.Color);
					//Console.WriteLine(currentJewel.Name + " | " + currentJewel.Material.Color + " | " + currentJewel.X1 + "|" + currentJewel.Y1);
					break;	
				case CreatureActions.HIDE_JEWEL:
					worldServer.SendHideIt(creatureId, currentJewel.Name);
					break;	
				case CreatureActions.GOAL_ACHIEVED:
					worldServer.SendStopCreature(creatureId);
					processLeafletControl();
					Console.Out.WriteLine();
					Console.Out.WriteLine("Clarion Creature: Goal achieved at: " + DateTime.Now);
					MaxNumberOfCognitiveCycles = CurrentCognitiveCycle;
					break;						
				default:
					break;
				}
			}
			//Console.WriteLine("Remaining jewel: " + getJewelRemainingTotal());
		}

        #endregion

        #region Setup Agent Methods
        /// <summary>
        /// Setup agent infra structure (ACS, NACS, MS and MCS)
        /// </summary>
        private void SetupAgentInfraStructure()
        {
            // Setup the ACS Subsystem
            SetupACS();                    
        }

        private void SetupMS()
        {            
            //RichDrive
        }

        /// <summary>
        /// Setup the ACS subsystem
        /// </summary>
        private void SetupACS()
        {
            // Create Rule to avoid collision with wall
            SupportCalculator avoidCollisionWallSupportCalculator = FixedRuleToAvoidCollisionWall;
            FixedRule ruleAvoidCollisionWall = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputRotateClockwise, avoidCollisionWallSupportCalculator);

            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleAvoidCollisionWall);

            // Create Colission To Go Ahead
            SupportCalculator goAheadSupportCalculator = FixedRuleToGoAhead;
            FixedRule ruleGoAhead = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoAhead, goAheadSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleGoAhead);
            
            // Create Go to Jewel
            SupportCalculator goToJewelSupportCalculator = FixedRuleToGoToJewel;
            FixedRule ruleGoToJewel = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoToJewelInVision, goToJewelSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleGoToJewel);

            // Create eat food
            SupportCalculator eatFoodSupportCalculator = FixedRuleToEatFood;
            FixedRule ruleEatFood = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputEatFood, eatFoodSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleEatFood);
            
            // Create get desired jewel
            SupportCalculator getDesiredJewelSupportCalculator = FixedRuleToGetDesiredJewel;
            FixedRule ruleGetDesiredJewel = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGetJewel, getDesiredJewelSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleGetDesiredJewel);   
            
            // Create hide jewel
            SupportCalculator hideJewelSupportCalculator = FixedRuleToHideJewel;
            FixedRule ruleHideJewel = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputHideJewel, hideJewelSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleHideJewel);        

            // Create goal achieved
            SupportCalculator goalAchievedSupportCalculator = FixedRuleGoalAchieved;
            FixedRule ruleGoalAchieved = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoalAchieved, goalAchievedSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleGoalAchieved);            
            
            // Disable Rule Refinement
            CurrentAgent.ACS.Parameters.PERFORM_RER_REFINEMENT = false;

            // The selection type will be probabilistic
            CurrentAgent.ACS.Parameters.LEVEL_SELECTION_METHOD = ActionCenteredSubsystem.LevelSelectionMethods.STOCHASTIC;

            // The action selection will be fixed (not variable) i.e. only the statement defined above.
            CurrentAgent.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;

            // Define Probabilistic values
            CurrentAgent.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 1;
            CurrentAgent.ACS.Parameters.FIXED_IRL_LEVEL_SELECTION_MEASURE = 0;
            CurrentAgent.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 0;
            CurrentAgent.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 0;
        }

        /// <summary>
        /// Make the agent perception. In other words, translate the information that came from sensors to a new type that the agent can understand
        /// </summary>
        /// <param name="sensorialInformation">The information that came from server</param>
        /// <returns>The perceived information</returns>
		private SensoryInformation prepareSensoryInformation(IList<Thing> listOfThings)
        {
            // New sensory information
            SensoryInformation si = World.NewSensoryInformation(CurrentAgent);
            currentJewel = null;
            currentFood = null;
            bool brickCreatureCheck = false;
            bool jewelAhead = false;
            bool foodAhead = false;
            bool desiredJewelInVision = false;
            Thing jewelInVision = null;
            Thing desiredJewelToGet = null;
            Thing foodToEat = null;
            
            if (getJewelRemainingTotal()==jewelGoal){
				si.Add(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
				si.Add(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
				si.Add(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
				si.Add(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION);
				return si;
        	}else{
				// Read each Thing to prepare Sensory Information activation
				for(int i = 0; i < listOfThings.Count ; i++){
	            	if(listOfThings[i].CategoryId == Thing.CATEGORY_BRICK && listOfThings[i].DistanceToCreature <= 50){
						brickCreatureCheck = true;
						break;
	            	}
					if(listOfThings[i].CategoryId == Thing.CATEGORY_CREATURE && !listOfThings[i].Name.Equals(creatureName) && listOfThings[i].DistanceToCreature <= 50){
						brickCreatureCheck = true;
						break;
	            	}
					if(listOfThings[i].CategoryId == Thing.CATEGORY_JEWEL && listOfThings[i].DistanceToCreature <= 50){
						jewelAhead = true;
						if(desiredJewelToGet != null){					
							if(listOfThings[i].DistanceToCreature < desiredJewelToGet.DistanceToCreature){
								desiredJewelToGet = listOfThings[i];
							}					
						}else{
							desiredJewelToGet = listOfThings[i];
						}
	            	}
					if(listOfThings[i].CategoryId == Thing.categoryPFOOD && listOfThings[i].DistanceToCreature <= 50){
						foodAhead = true;
						if(foodToEat!= null){					
							if(listOfThings[i].DistanceToCreature < foodToEat.DistanceToCreature){
								foodToEat = listOfThings[i];
							}					
						}else{
							foodToEat = listOfThings[i];
						}
	            	}
					if(listOfThings[i].CategoryId == Thing.CATEGORY_FOOD && listOfThings[i].DistanceToCreature <= 50){
						foodAhead = true;
						if(foodToEat!= null){					
							if(listOfThings[i].DistanceToCreature < foodToEat.DistanceToCreature){
								foodToEat = listOfThings[i];
							}					
						}else{
							foodToEat = listOfThings[i];
						}
	            	}
					if(listOfThings[i].CategoryId == Thing.CATEGORY_NPFOOD && listOfThings[i].DistanceToCreature <= 50){
						foodAhead = true;
						if(foodToEat!= null){					
							if(listOfThings[i].DistanceToCreature < foodToEat.DistanceToCreature){
								foodToEat = listOfThings[i];
							}					
						}else{
							foodToEat = listOfThings[i];
						}
	            	}
					if(listOfThings[i].CategoryId == Thing.CATEGORY_JEWEL && isDesiredJewel(listOfThings[i].Material.Color) 
					   													  && listOfThings[i].DistanceToCreature > 50){
						desiredJewelInVision = true;
						if(jewelInVision != null){					
							if(listOfThings[i].DistanceToCreature < jewelInVision.DistanceToCreature){
								jewelInVision = listOfThings[i];
							}					
						}else{
							jewelInVision = listOfThings[i];
						}
	            	}					
	            }
				
            	if(brickCreatureCheck){
					si.Add(inputWallCreatureAhead, CurrentAgent.Parameters.MAX_ACTIVATION);
					si.Add(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION);
					//Console.WriteLine("CATEGORY_BRICK");
            	}
				else if(jewelAhead){
					si.Add(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelAhead, CurrentAgent.Parameters.MAX_ACTIVATION);
					si.Add(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION);
            		currentJewel = desiredJewelToGet;
            		//Console.WriteLine("CATEGORY_JEWEL_AHEAD");
            	}
				else if(foodAhead){
					si.Add(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputFoodAhead, CurrentAgent.Parameters.MAX_ACTIVATION);
					si.Add(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION);					
					currentFood = foodToEat;
            		//Console.WriteLine("CATEGORY_FOOD");
            	}
				else if(desiredJewelInVision){
					si.Add(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelInVision, CurrentAgent.Parameters.MAX_ACTIVATION);
					currentJewel = jewelInVision;
					//Console.WriteLine("CATEGORY_JEWEL_IN_VISION");
				}
				else{
					si.Add(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION);
					si.Add(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION);
					//Console.WriteLine("WANDER");
				}
	    		return si;
            }
 				
	    } 
        #endregion

        #region Fixed Rules
        private double FixedRuleToAvoidCollisionWall(ActivationCollection currentInput, Rule target)
        {
            // See partial match threshold to verify what are the rules available for action selection
            return ((currentInput.Contains(inputWallCreatureAhead, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }

        private double FixedRuleToGoAhead(ActivationCollection currentInput, Rule target)
        {
            // Here we will make the logic to go ahead
            if((currentInput.Contains(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION)) &&                 
               (getJewelRemainingTotal()!=jewelGoal)){
            	return 1.0;
            }else{
            	return 0.0;
            }
        }
 
        private double FixedRuleToEatFood(ActivationCollection currentInput, Rule target)
        {
            // Here we will make the logic to go ahead
            if((currentInput.Contains(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputFoodAhead, CurrentAgent.Parameters.MAX_ACTIVATION)) &&
               (currentInput.Contains(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&               
               (currentInput.Contains(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION)) &&               
               (getJewelRemainingTotal()!=jewelGoal)){
            	return 1.0;
            }else{
            	return 0.0;
            }
        }
        
        private double FixedRuleToGoToJewel(ActivationCollection currentInput, Rule target)
        {
            // Here we will make the logic to go ahead
            if((currentInput.Contains(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputJewelAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&               
               (currentInput.Contains(inputJewelInVision, CurrentAgent.Parameters.MAX_ACTIVATION)) &&
               (isDesiredJewel(currentJewel.Material.Color)) &&
               (getJewelRemainingTotal()!=jewelGoal)){
            	return 1.0;
            }else{
            	return 0.0;
            }
        }

        private double FixedRuleToGetDesiredJewel(ActivationCollection currentInput, Rule target)
        {
            // Here we will make the logic to go ahead
            if((currentInput.Contains(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputJewelAhead, CurrentAgent.Parameters.MAX_ACTIVATION))&&
               (currentInput.Contains(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&               
               (currentInput.Contains(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (isDesiredJewel(currentJewel.Material.Color))&&               
               (getJewelRemainingTotal()!=jewelGoal)){
            	return 1.0;
            }else{
            	return 0.0;
            }
        } 
        private double FixedRuleToHideJewel(ActivationCollection currentInput, Rule target)
        {
            // Here we will make the logic to go ahead
            if((currentInput.Contains(inputWallCreatureAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (currentInput.Contains(inputJewelAhead, CurrentAgent.Parameters.MAX_ACTIVATION)) &&
               (currentInput.Contains(inputFoodAhead, CurrentAgent.Parameters.MIN_ACTIVATION)) &&               
               (currentInput.Contains(inputJewelInVision, CurrentAgent.Parameters.MIN_ACTIVATION)) &&
               (!isDesiredJewel(currentJewel.Material.Color)) &&
               (getJewelRemainingTotal()!=jewelGoal)){
            	return 1.0;
            }else{
            	return 0.0;
            }
        }   

        private double FixedRuleGoalAchieved(ActivationCollection currentInput, Rule target)
        {
        	if (getJewelRemainingTotal()==jewelGoal){
				return 1.0;
        	}else{
				return 0.0;
			}
        }          
        #endregion

        #region Run Thread Method
        private void CognitiveCycle(object obj)
        {

			Console.WriteLine("Starting Cognitive Cycle ... press CTRL-C to finish !");
            // Cognitive Cycle starts here getting sensorial information
            while (CurrentCognitiveCycle != MaxNumberOfCognitiveCycles)
            {   
				// Get current sensory information                    
				IList<Thing> currentSceneInWS3D = processSensoryInformation();

                // Make the perception
                SensoryInformation si = prepareSensoryInformation(currentSceneInWS3D);

                //Perceive the sensory information
                CurrentAgent.Perceive(si);

                //Choose an action
                ExternalActionChunk chosen = CurrentAgent.GetChosenExternalAction(si);

                // Get the selected action
                String actionLabel = chosen.LabelAsIComparable.ToString();
                CreatureActions actionType = (CreatureActions)Enum.Parse(typeof(CreatureActions), actionLabel, true);

                // Increment the number of cognitive cycles
                CurrentCognitiveCycle++;
                	
                // Call the output event handler
				processSelectedAction(actionType);

                //Wait to the agent accomplish his job
                if (TimeBetweenCognitiveCycles > 0)
                {
                    Thread.Sleep(TimeBetweenCognitiveCycles);
                }
			}
            Console.WriteLine("Finished");
        }
        #endregion
    }
    
  }