
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ClarionApp;
using ClarionApp.Model;
using ClarionApp.Exceptions;
using Gtk;

namespace ClarionApp
{
	class MainClass
	{
		#region properties
		private WSProxy ws = null;
        private ClarionAgent agent;
        String creatureId = String.Empty;
        String creatureName = String.Empty;
		#endregion

		#region constructor
		public MainClass(string[] args) {
			Application.Init();
			Console.WriteLine ("IA941 - Clarion - Activity");
			try
            {
                ws = new WSProxy("localhost", 4011);

                String message = ws.Connect();

                if (ws != null && ws.IsConnected)
                {
                    Console.Out.WriteLine ("[SUCCESS] " + message + "\n");
		            if(args.Length>1){
                    	if(args[1] == "reset"){
                    		ws.SendWorldReset();
                    	}
					}else{
						ws.SendWorldReset();
					}

                    ws.NewCreature(100, 500, 0, out creatureId, out creatureName);
					//Console.WriteLine ("SendCreateLeaflet: ");
					//ws.SendCreateLeaflet();
					//ws.SendNewWayPoint (700, 500);
							
		            if(args.Length>1){
						if(args[3] == "grow"){
	                    	//ws.NewBrick(4, 747, 2, 800, 567);
	                   		//ws.NewBrick(4, 50, -4, 747, 47);
	                   		//ws.NewBrick(4, 49, 562, 796, 599);
	                    	//ws.NewBrick(4, -2, 6, 50, 599);
		                   
		                    Random x = new Random();
		                    Random y = new Random();
		                    Random z = new Random();
		                    int minX = 100;
		                    int maxX = 600;
		                    int minY = 100;
		                    int maxY = 500;
		                    int xNew = 0;
		                    int yNew = 0;

		                    // Random Jewels
		                    for(int j=0; j < 7; j++){
			                    for(int i=0; i<5; i++){
		                    		xNew = x.Next(minX,maxX);
		                    		yNew = y.Next(minY,maxY);
		                    		if(((mod(xNew,100)<50) && (mod(yNew,100)<50)) || ((mod(xNew,100)<50) && (mod(yNew,500)<50))){
		                    			x.Next(minX+z.Next(1,50),maxX); 
		                    			if(i>0)i--;
		                    		}else{
		                    			ws.NewJewel(j, xNew, yNew);
		                    		 	x.Next(minX+z.Next(1,50),maxX);
		                    		}
			                    }
		                    }
		                    
		                    // Random food
		                    for(int j=0; j < 2; j++){
			                    for(int i=0; i<2;i++){
		                    		xNew = x.Next(minX,maxX);
		                    		yNew = y.Next(minY,maxY);
		                    		if(!(((xNew>60)&&(xNew<140))||(((xNew>360)&&(xNew<440))))){
		                    		 	ws.NewFood(j, xNew, yNew);
			                    		x.Next(minX+i,maxX);  	
		                    		}else{
		                    			x.Next(minX+i,maxX);
		                    			if(i>0)i--;
		                    		}
			                    }
		                    }	
						}
					}else{
	                    	//ws.NewBrick(4, 747, 2, 800, 567);
	                   		//ws.NewBrick(4, 50, -4, 747, 47);
	                   	    //ws.NewBrick(4, 49, 562, 796, 599);
	                    	//ws.NewBrick(4, -2, 6, 50, 599);
		                   
		                    Random x = new Random();
		                    Random y = new Random();
		                    Random z = new Random();
		                    int minX = 100;
		                    int maxX = 600;
		                    int minY = 100;
		                    int maxY = 500;
		                    int xNew = 0;
		                    int yNew = 0;

		                    // Random Jewels
		                    for(int j=0; j < 7; j++){
			                    for(int i=0; i<5; i++){
		                    		xNew = x.Next(minX,maxX);
		                    		yNew = y.Next(minY,maxY);
		                    		if(((mod(xNew,100)<50) && (mod(yNew,100)<50)) || ((mod(xNew,100)<50) && (mod(yNew,500)<50))){
		                    			x.Next(minX+z.Next(1,50),maxX); 
		                    			if(i>0)i--;
		                    		}else{
		                    			ws.NewJewel(j, xNew, yNew);
		                    		 	x.Next(minX+z.Next(1,50),maxX);
		                    		}
			                    }
		                    }
		                    
		                    // Random food
		                    for(int j=0; j < 2; j++){
			                    for(int i=0; i<2;i++){
		                    		xNew = x.Next(minX,maxX);
		                    		yNew = y.Next(minY,maxY);
		                    		if(!(((xNew>60)&&(xNew<140))||(((xNew>360)&&(xNew<440))))){
		                    		 	ws.NewFood(j, xNew, yNew);
			                    		x.Next(minX+i,maxX);  	
		                    		}else{
		                    			x.Next(minX+i,maxX);
		                    			if(i>0)i--;
		                    		}
			                    }
		                    }							
					}
                    if (!String.IsNullOrWhiteSpace(creatureId))
                    {
                        ws.SendStartCamera(creatureId);
                        ws.SendStartCreature(creatureId);
                    }
                    
		            if(args.Length>0){
						Console.Out.WriteLine("Sleep time: " + Convert.ToInt32(args[0]));
						Thread.Sleep(Convert.ToInt32(args[0]));
					}else{
                    	Thread.Sleep(500);
					}	

                    Console.Out.WriteLine("Creature created with name: " + creatureId + "\n");
					agent = new ClarionAgent(ws,creatureId,creatureName);
                    agent.Run();
					Console.Out.WriteLine("Running Simulation ...\n");
                }
				else {
					Console.Out.WriteLine("The WorldServer3D engine was not found ! You must start WorldServer3D before running this application !");
					System.Environment.Exit(1);
				}
            }
            catch (WorldServerInvalidArgument invalidArtgument)
            {
                Console.Out.WriteLine(String.Format("[ERROR] Invalid Argument: {0}\n", invalidArtgument.Message));
            }
            catch (WorldServerConnectionError serverError)
            {
                Console.Out.WriteLine(String.Format("[ERROR] Is is not possible to connect to server: {0}\n", serverError.Message));
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(String.Format("[ERROR] Unknown Error: {0}\n", ex.Message));
            }
			Application.Run();
		}
		#endregion

		#region Methods
		public static void Main (string[] args)	{    
            if(args.Length>0){
				Console.Out.WriteLine("Sleep time: " + Convert.ToInt32(args[2]));
				Thread.Sleep(Convert.ToInt32(args[2]));
			}else{
            	Thread.Sleep(500);
			}		
			new MainClass(args);
		}
		
		public int mod(int a, int b){
			int x = a - b;
			if(x<0){
				x = x * -1;
			}
			return x;
		}
        #endregion
	}
	
	
}
