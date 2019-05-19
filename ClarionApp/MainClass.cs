
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
		public MainClass (string [] args)
		{
			Application.Init ();
			Console.WriteLine ("IA941 - Clarion - Activity");
			try {
				ws = new WSProxy ("localhost", 4011);

				String message = ws.Connect ();

				if (ws != null && ws.IsConnected) {
					Console.Out.WriteLine ("[SUCCESS] " + message + "\n");
					if (args.Length > 1) {
						if (args [1] == "reset") {
							ws.SendWorldReset ();
						}
					} else {
						ws.SendWorldReset ();
					}

					ws.NewCreature (100, 500, 0, out creatureId, out creatureName);

					// Start >>> Leaflet process preparation //
					String leaflets_raw = ws.SendCreateLeaflet ();
					string [] leaflets_arr = leaflets_raw.Split (' ');
					//Console.WriteLine ("Leaflets: " + leaflets_raw);

					Leaflet leaflet1 = new Leaflet ();
					leaflet1.items = new List<LeafletItem> ();
					Leaflet leaflet2 = new Leaflet ();
					leaflet2.items = new List<LeafletItem> ();
					Leaflet leaflet3 = new Leaflet ();
					leaflet3.items = new List<LeafletItem> ();

					int leafNum = 1;
					int lf1T = 0;
					int lf2T = 0;
					int lf3T = 0;
					for (int i=0; i < leaflets_arr.Length; i++) {
						if (leaflets_arr [i] != "") {
							if(leaflets_arr [i] == "false") {
								leafNum++;
							} else if (leaflets_arr [i].Length > 3) {
								switch (leafNum) {
								case 1:
									leaflet1.items.Add (new LeafletItem (leaflets_arr [i], Int32.Parse (leaflets_arr [i + 2]), 0));
									lf1T = lf1T + Int32.Parse (leaflets_arr [i + 2]);
									i = i + 2;
									break;
								case 2:
									leaflet2.items.Add (new LeafletItem (leaflets_arr [i], Int32.Parse (leaflets_arr [i + 2]), 0));
									lf2T = lf2T + Int32.Parse (leaflets_arr [i + 2]);
									i = i + 2;
									break;
								case 3:
									leaflet3.items.Add (new LeafletItem (leaflets_arr [i], Int32.Parse (leaflets_arr [i + 2]), 0));
									lf3T = lf3T + Int32.Parse (leaflets_arr [i + 2]);
									i = i + 2;
									break;
								default:
									break;
								}
							}
						}
					}

					//Console.WriteLine ("Leaflet 1 items: ");
					//foreach(LeafletItem lt in leaflet1.items) {
					//	Console.WriteLine ("Jewel: " + lt.itemKey);
					//	Console.WriteLine ("Total: " + lt.totalNumber);

					//}

					//Console.WriteLine ("Leaflet 2 items: ");
					//foreach (LeafletItem lt in leaflet2.items) {
					//	Console.WriteLine ("Jewel: " + lt.itemKey);
					//	Console.WriteLine ("Total: " + lt.totalNumber);

					//}

					//Console.WriteLine ("Leaflet 3 items: ");
					//foreach (LeafletItem lt in leaflet3.items) {
					//	Console.WriteLine ("Jewel: " + lt.itemKey);
					//	Console.WriteLine ("Total: " + lt.totalNumber);

					//}

					// In case the leaflets sum have less than 9 jewels, then add remaining
					int remJewels = 9 - (lf1T+lf2T+lf3T);
					//Console.WriteLine ("Rem Jewels: " + remJewels);
					//Console.WriteLine ("count lf1: " + leaflet1.items.Count);
					if (remJewels > 0) {
						for (int i = 0; i < remJewels; i++) {
							if (lf1T < 3) {
								leaflet1.items.Add (new LeafletItem ("Red", 1, 0));
								lf1T++;
								//Console.WriteLine ("Added rem Jewel - Leaflet 1");
							} else if (lf2T < 3) {
								leaflet2.items.Add (new LeafletItem ("Red", 1, 0));
								lf2T++;
								//Console.WriteLine ("Added rem Jewel - Leaflet 2");
							} else if (lf3T < 3) {
								leaflet3.items.Add (new LeafletItem ("Red", 1, 0));
								lf3T++;
								//Console.WriteLine ("Added rem Jewel - Leaflet 3");
							}
						}
					}
					//Console.WriteLine ("Double check: ");
					//Console.WriteLine ("Leaflet 1 items: ");
					//foreach (LeafletItem lt in leaflet1.items) {
					//	Console.WriteLine ("Jewel: " + lt.itemKey);
					//	Console.WriteLine ("Total: " + lt.totalNumber);

					//}

					//Console.WriteLine ("Leaflet 2 items: ");
					//foreach (LeafletItem lt in leaflet2.items) {
					//	Console.WriteLine ("Jewel: " + lt.itemKey);
					//	Console.WriteLine ("Total: " + lt.totalNumber);

					//}

					//Console.WriteLine ("Leaflet 3 items: ");
					//foreach (LeafletItem lt in leaflet3.items) {
					//	Console.WriteLine ("Jewel: " + lt.itemKey);
					//	Console.WriteLine ("Total: " + lt.totalNumber);

					//}
					// End >>> Leaflet process preparation //

					ws.SendNewWayPoint (700, 500);
					if (args.Length > 1) {
						if (args [3] == "grow") {
							//ws.NewBrick (4, 747, 2, 800, 567);
							//ws.NewBrick (4, 50, -4, 747, 47);
							//ws.NewBrick (4, 49, 562, 796, 599);
							//ws.NewBrick (4, -2, 6, 50, 599);

							Random x = new Random ();
							Random y = new Random ();
							Random z = new Random ();
							int minX = 100;
							int maxX = 600;
							int minY = 100;
							int maxY = 500;
							int xNew = 0;
							int yNew = 0;

							// Random Jewels
							for (int j = 0; j < 7; j++) {
								for (int i = 0; i < 5; i++) {
									xNew = x.Next (minX, maxX);
									yNew = y.Next (minY, maxY);
									if (((mod (xNew, 100) < 50) && (mod (yNew, 100) < 50)) || ((mod (xNew, 100) < 50) && (mod (yNew, 500) < 50))) {
										x.Next (minX + z.Next (1, 50), maxX);
										if (i > 0) i--;
									} else {
										ws.NewJewel (j, xNew, yNew);
										x.Next (minX + z.Next (1, 50), maxX);
									}
								}
							}

							// Random food
							for (int j = 0; j < 2; j++) {
								for (int i = 0; i < 2; i++) {
									xNew = x.Next (minX, maxX);
									yNew = y.Next (minY, maxY);
									if (!(((xNew > 60) && (xNew < 140)) || (((xNew > 360) && (xNew < 440))))) {
										ws.NewFood (j, xNew, yNew);
										x.Next (minX + i, maxX);
									} else {
										x.Next (minX + i, maxX);
										if (i > 0) i--;
									}
								}
							}
						}
					} else {
						//ws.NewBrick (4, 747, 2, 800, 567);
						//ws.NewBrick (4, 50, -4, 747, 47);
						//ws.NewBrick (4, 49, 562, 796, 599);
						//ws.NewBrick (4, -2, 6, 50, 599);

						Random x = new Random ();
						Random y = new Random ();
						Random z = new Random ();
						int minX = 100;
						int maxX = 600;
						int minY = 100;
						int maxY = 500;
						int xNew = 0;
						int yNew = 0;

						// Random Jewels
						for (int j = 0; j < 7; j++) {
							for (int i = 0; i < 5; i++) {
								xNew = x.Next (minX, maxX);
								yNew = y.Next (minY, maxY);
								if (((mod (xNew, 100) < 50) && (mod (yNew, 100) < 50)) || ((mod (xNew, 100) < 50) && (mod (yNew, 500) < 50))) {
									x.Next (minX + z.Next (1, 50), maxX);
									if (i > 0) i--;
								} else {
									ws.NewJewel (j, xNew, yNew);
									x.Next (minX + z.Next (1, 50), maxX);
								}
							}
						}

						// Random food
						for (int j = 0; j < 2; j++) {
							for (int i = 0; i < 2; i++) {
								xNew = x.Next (minX, maxX);
								yNew = y.Next (minY, maxY);
								if (!(((xNew > 60) && (xNew < 140)) || (((xNew > 360) && (xNew < 440))))) {
									ws.NewFood (j, xNew, yNew);
									x.Next (minX + i, maxX);
								} else {
									x.Next (minX + i, maxX);
									if (i > 0) i--;
								}
							}
						}
					}


					// ---- Start - Leaflet Generation --- //
					IList<Thing> response = null;
					if (ws != null && ws.IsConnected) {
						response = ws.SendGetCreatureState (creatureName);
						if (leaflet1.leafletID == 0 && leaflet2.leafletID == 0 && leaflet3.leafletID == 0) {
							Creature cc = (Creature)response [0];
							//leaflet1.leafletID = cc.leaflets [0].leafletID;
							//leaflet2.leafletID = cc.leaflets [1].leafletID;
							//leaflet3.leafletID = cc.leaflets [2].leafletID;
							leaflet1 = cc.leaflets [0];
							leaflet2 = cc.leaflets [1];
							leaflet3 = cc.leaflets [2];
							//Console.WriteLine ("Creature found: " + cc.Name);
							//Console.WriteLine ("LF1: " + cc.leaflets [0].leafletID);
							//Console.WriteLine ("LF2: " + cc.leaflets [1].leafletID);
							//Console.WriteLine ("LF3: " + cc.leaflets [2].leafletID);
						}
					}
					List<Leaflet> leafletList = new List<Leaflet> ();
					leafletList.Add (leaflet1);
					leafletList.Add (leaflet2);
					leafletList.Add (leaflet3);
					// ---- Finish - Leaflet generation --- //



					if (!String.IsNullOrWhiteSpace (creatureId)) {
						ws.SendStartCamera (creatureId);
						ws.SendStartCreature (creatureId);
					}

					if (args.Length > 0) {
						Console.Out.WriteLine ("Sleep time: " + Convert.ToInt32 (args [0]));
						Thread.Sleep (Convert.ToInt32 (args [0]));
					} else {
						Thread.Sleep (500);
					}

					Console.Out.WriteLine ("Creature created with name: " + creatureId + "\n");
					agent = new ClarionAgent (ws, creatureId, creatureName, leafletList);
					agent.Run ();
					Console.Out.WriteLine ("Running Simulation ...\n");
				} else {
					Console.Out.WriteLine ("The WorldServer3D engine was not found ! You must start WorldServer3D before running this application !");
					System.Environment.Exit (1);
				}
			} catch (WorldServerInvalidArgument invalidArtgument) {
				Console.Out.WriteLine (String.Format ("[ERROR] Invalid Argument: {0}\n", invalidArtgument.Message));
			} catch (WorldServerConnectionError serverError) {
				Console.Out.WriteLine (String.Format ("[ERROR] Is is not possible to connect to server: {0}\n", serverError.Message));
			} catch (Exception ex) {
				Console.Out.WriteLine (String.Format ("[ERROR] Unknown Error: {0}\n", ex.Message));
			}
			Application.Run ();
		}
		#endregion

		#region Methods
		public static void Main (string [] args)
		{
			if (args.Length > 0) {
				Console.Out.WriteLine ("Sleep time: " + Convert.ToInt32 (args [2]));
				Thread.Sleep (Convert.ToInt32 (args [2]));
			} else {
				Thread.Sleep (500);
			}
			new MainClass (args);
		}

		public int mod (int a, int b)
		{
			int x = a - b;
			if (x < 0) {
				x = x * -1;
			}
			return x;
		}
		#endregion
	}


}
