// protoc --csharp_out=. --grpc_out=. --plugin=protoc-gen-grpc=`which grpc_csharp_plugin` UoService.proto
// python3.7 -m grpc_tools.protoc -I ../ --python_out=. --grpc_python_out=. UoService.proto --proto_path /home/kimbring2/uoservice/uoservice/protos/

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf;
using Uoservice;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Game.Map;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using static ClassicUO.Network.NetClient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ClassicUO.Grpc
{
	internal sealed class UoServiceImpl : UoService.UoServiceBase
    {
        GameController _controller;
        int _port;
        Server _grpcServer;
        Channel _grpcChannel;

        Layer[] _layerOrder =
        {
            Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
            Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
            Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
        };

        List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();
        List<GrpcItemData> worldItemDataList = new List<GrpcItemData>();
        List<GrpcItemData> equippedItemDataList = new List<GrpcItemData>();
        List<GrpcItemData> backpackItemDataList = new List<GrpcItemData>();
        List<GrpcItemData> corpseItemDataList = new List<GrpcItemData>();

        public List<GrpcGameObjectData> grpcLandObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcPlayerMobileObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcMobileObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcItemObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcStaticObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcItemDropableLandList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcVendorItemList = new List<GrpcGameObjectData>();

        public UoServiceImpl(GameController controller, int port)
        {
            _controller = controller;
            _port = port;

            _grpcChannel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            _grpcServer = new Server
	        {
	            Services = { UoService.BindService(this) },
	            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
	        };
        }

        public void AddGameObject(string type, uint screen_x, uint screen_y, uint distance, uint game_x, uint game_y, 
        						  uint serial, string name, bool is_corpse, string title, uint amount, uint price)
        {
        	try 
        	{
	        	if (type == "Land") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, distance: {3}", type, x, y, distance);
	        		grpcLandObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        													   GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													   Title=title, Amount=amount, Price=price });

	        		bool can_drop = (distance >= 1) && (distance < Constants.DRAG_ITEMS_DISTANCE);
	        		if (can_drop)
                	{
	        			//Console.WriteLine("type: {0}, x: {1}, y: {2}, distance: {3}", type, game_x, game_y, distance);
	        			grpcItemDropableLandList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        														 GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													     Title=title, Amount=amount, Price=price });
	        		}
	        	}
	        	else if (type == "PlayerMobile") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, distance: {3}", type, x, y, distance);
	        		grpcPlayerMobileObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        														 GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													     Title=title, Amount=amount, Price=price });
	        	}
	        	else if (type == "Item") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, screen_x, screen_y, distance, name);
	        		grpcItemObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        													   GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													   Title=title, Amount=amount, Price=price });
	        	}
	        	else if (type == "Static") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, distance: {3}", type, x, y, distance);
	        		grpcStaticObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        														 GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													     Title=title, Amount=amount, Price=price });
	        	}
	        	else if (type == "Mobile") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, distance: {3}", type, screen_x, screen_y, distance);
	        		grpcMobileObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        														 GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													     Title=title, Amount=amount, Price=price });
	        	}
	        	else if (type == "ShopItem") {
	        		//Console.WriteLine("ShopItem");
	        		//Console.WriteLine("name: {0}, serial: {1}", name, serial);
	        		grpcVendorItemList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        													   GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													   Title=title, Amount=amount, Price=price });
	        	}
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object: " + ex.Message);
            }
        }

        public void Start() 
        {
        	_grpcServer.Start();
        }

        public byte[] ReadImage(string imagePath)
        {
            try
            {
                return File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read the image: " + ex.Message);

                return null;
            }
        }

        // Server side handler of the SayHello RPC
        public override Task<States> Reset(Config config, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();

            foreach (Mobile mob in World.Mobiles.Values)
            {
            	//Console.WriteLine("mob.GetScreenPosition().X: {0}, mob.GetScreenPosition().Y: {1}", 
            	//				   mob.GetScreenPosition().X, mob.GetScreenPosition().Y);
            	//Console.WriteLine("Name: {0}, Race: {1}", mob.Name, mob.Race);

            	if ( (mob.GetScreenPosition().X <= 0.0) || (mob.GetScreenPosition().Y <= 0.0) ) 
            	{
            		continue;
            	}

                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, 
                										   X = (float) mob.GetScreenPosition().X, 
                										   Y = (float) mob.GetScreenPosition().Y,
                										   Race = (uint) mob.Race });
            }

            States states = new States();

            //states.ScreenImage = new ScreenImage { Image = byteString };

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<States> ReadObs(Config config, ServerCallContext context)
        {
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);
            try
            {
	            foreach (Mobile mob in World.Mobiles.Values)
	            {
	            	//Console.WriteLine("mob.GetScreenPosition().X: {0}, mob.GetScreenPosition().Y: {1}", 
	            	//				     mob.GetScreenPosition().X, mob.GetScreenPosition().Y);
	            	//Console.WriteLine("Name: {0}, Race: {1}, Title: {2}, Serial: {3}", mob.Name, mob.Race, mob.Title, mob.Serial);

	            	if ( (mob.GetScreenPosition().X <= 0.0) || (mob.GetScreenPosition().Y <= 0.0) ) 
	            	{
	            		continue;
	            	}

	                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, 
	                										   X = (float) mob.GetScreenPosition().X, 
	                										   Y = (float) mob.GetScreenPosition().Y,
	                										   Race = (uint) mob.Race,
	                										   Serial = (uint) mob.Serial });
	            }

	            //Console.WriteLine("\n");
	        }
	        catch (Exception ex)
            {
            	//Console.WriteLine("Failed to load the mobile: " + ex.Message);
            	grpcMobileDataList.Add(new GrpcMobileData{ Name = "base", 
	                									   X = (float) 500.0, 
	                									   Y = (float) 500.0,
	                									   Race = (uint) 0,
	                									   Serial = (uint) 1234 });
            }

            foreach (Item item in World.Items.Values)
            {
            	// Name: Valorite Longsword, Amount: 1, Serial: 1073933224
            	//Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
            	//																	   item.Amount, item.Serial);
            	worldItemDataList.Add(new GrpcItemData{ Name = !string.IsNullOrEmpty(item.Name) ? item.Name : "Null", 
	            									    Layer = (uint) item.Layer,
	              									    Serial = (uint) item.Serial,
	            									    Amount = (uint) item.Amount });


            }

	        if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	foreach (Layer layer in _layerOrder) {
	        		Item item = World.Player.FindItemByLayer(layer);
	        		try 
	        		{
	            		equippedItemDataList.Add(new GrpcItemData{ Name = item.Name, 
		            									   	       Layer = (uint) item.Layer,
		              									           Serial = (uint) item.Serial,
		            									           Amount = (uint) item.Amount });
	            	}
		            catch (Exception ex)
		            {
		            	//Console.WriteLine("Failed to load the equipped item: " + ex.Message);
		            }
		        }
	        }

            if ((World.Player != null) && (World.InGame == true))
            {
                Item backpack = World.Player.FindItemByLayer(Layer.Backpack); 
                for (LinkedObject i = backpack.Items; i != null; i = i.Next)
                {
                    Item item = (Item) i;
                    //Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
            		//																       item.Amount, item.Serial);
            		try 
	        		{
	            		backpackItemDataList.Add(new GrpcItemData{ Name = item.Name, 
		            									   	       Layer = (uint) item.Layer,
		              									           Serial = (uint) item.Serial,
		            									           Amount = (uint) item.Amount });
	            	}
		            catch (Exception ex)
		            {
		            	//Console.WriteLine("Failed to load the backpack item: " + ex.Message);
		            }
                }
	        }

	        PlayerStatus playerStatus = new PlayerStatus();
	        if ((World.Player != null) && (World.InGame == true))
            {
		        playerStatus = new PlayerStatus { Str = (uint) World.Player.Strength, Dex = (uint) World.Player.Dexterity, 
		        								  Intell = (uint) World.Player.Intelligence, Hits = (uint) World.Player.Hits,
		        								  HitsMax = (uint) World.Player.HitsMax, Stamina = (uint) World.Player.Stamina,
		        								  StaminaMax = (uint) World.Player.StaminaMax, Mana = (uint) World.Player.Mana,
		        								  Gold = (uint) World.Player.Gold, PhysicalResistance = (uint) World.Player.PhysicalResistance,
		        								  Weight = (uint) World.Player.Weight, WeightMax = (uint) World.Player.WeightMax };
		    }

            States states = new States();

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            GrpcItemList worldItemList = new GrpcItemList();
            worldItemList.Item.AddRange(worldItemDataList);
            states.WorldItemList = worldItemList;

            GrpcItemList equippedItemList = new GrpcItemList();
            equippedItemList.Item.AddRange(equippedItemDataList);
            states.EquippedItemList = equippedItemList;

            GrpcItemList backpackItemList = new GrpcItemList();
            backpackItemList.Item.AddRange(backpackItemDataList);
            states.BackpackItemList = backpackItemList;

            GrpcItemList corpseItemList = new GrpcItemList();
            corpseItemList.Item.AddRange(corpseItemDataList);
            states.CorpseItemList = corpseItemList;

            states.PlayerStatus = playerStatus;

            try
            {
	            GrpcGameObjectList landObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList playerMobileObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList staticObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList itemObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList mobileObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList itemDropableLandObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList vendorItemObjectList = new GrpcGameObjectList();

	            landObjectList.GameObject.AddRange(grpcLandObjectList);
	            playerMobileObjectList.GameObject.AddRange(grpcPlayerMobileObjectList);
	            staticObjectList.GameObject.AddRange(grpcStaticObjectList);
	            itemObjectList.GameObject.AddRange(grpcItemObjectList);
	            mobileObjectList.GameObject.AddRange(grpcMobileObjectList);
	            itemDropableLandObjectList.GameObject.AddRange(grpcItemDropableLandList);
	            vendorItemObjectList.GameObject.AddRange(grpcVendorItemList);

	            states.LandObjectList = landObjectList;
	            states.PlayerMobileObjectList = playerMobileObjectList;
	            states.StaticObjectList = staticObjectList;
	            states.MobileObjectList = mobileObjectList;
	            states.ItemObjectList = itemObjectList;
	            states.ItemDropableLandList = itemDropableLandObjectList;
	            states.VendorItemObjectList = vendorItemObjectList;
	        }
	        catch (Exception ex)
            {
            	//Console.WriteLine("Failed to load the land object list: " + ex.Message);
            }

            grpcMobileDataList.Clear();
	        worldItemDataList.Clear();
	        equippedItemDataList.Clear();
	        backpackItemDataList.Clear();

            grpcLandObjectList.Clear();
            grpcItemDropableLandList.Clear();
            grpcPlayerMobileObjectList.Clear();
            grpcMobileObjectList.Clear();
            grpcItemObjectList.Clear();
            grpcStaticObjectList.Clear();

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(Actions actions, ServerCallContext context)
        {
        	//Console.WriteLine("actions.ActionType: {0}", actions.ActionType);
        	//Console.WriteLine("actions.MobileSerial: {0}", actions.MobileSerial);
        	//Console.WriteLine("actions.WalkDirection.Direction: {0}", actions.WalkDirection.Direction);

            _controller.action_1 = actions.ActionType;

            if (actions.ActionType == 1) {
            	if (World.InGame == true) {
            		if (actions.WalkDirection.Direction == 0) {
	            		World.Player.Walk(Direction.Up, true);
	            	}
	            	else if (actions.WalkDirection.Direction == 2) {
	            		World.Player.Walk(Direction.Right, true);
	            	}	
	            	else if (actions.WalkDirection.Direction == 3) {
	            		World.Player.Walk(Direction.Left, true);
	            	}
	            	else if (actions.WalkDirection.Direction == 4) {
	            		World.Player.Walk(Direction.Down, true);
	            	}
            	}
	        }
	        else if (actions.ActionType == 2) {
	        	if (World.Player != null) {
        			GameActions.DoubleClick(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 3) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 3");

	        		try
	        		{
	        			Item item = World.Items.Get(actions.ItemSerial);
	        			Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
            																		     	 item.Amount, item.Serial);
	        			GameActions.PickUp(actions.ItemSerial, 0, 0, item.Amount);
					}
	        		catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to parse the item info: " + ex.Message);
		            }
	        	}
	        }
	        else if (actions.ActionType == 4) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 4");
        			Item backpack = World.Player.FindItemByLayer(Layer.Backpack);
        			GameActions.DropItem(actions.ItemSerial, 0xFFFF, 0xFFFF, 0, backpack.Serial);
	        	}
	        }
	        else if (actions.ActionType == 5) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 5");

	        		int randomNumber;
					Random RNG = new Random();
	        		int index = RNG.Next(grpcItemDropableLandList.Count);

	        		try
	        		{
	        			GrpcGameObjectData selected = grpcItemDropableLandList[index];
	        			Console.WriteLine("ItemSerial: {0}, GameX: {0}, GameY: {1}", selected.GameX, selected.GameY);
	        			GameActions.DropItem(actions.ItemSerial, (int) selected.GameX, (int) selected.GameY, 0, 0xFFFF_FFFF);
	        		}
	        		catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to fine the item dropable land: " + ex.Message);
		            }
	        	}
	        } 
	        else if (actions.ActionType == 6) {
	        	if (World.Player != null) {
                    GameActions.Equip();
	        	}
	        }
	        else if (actions.ActionType == 7) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 7");

                    try
                    {
                    	Console.WriteLine("actions.ItemSerial: {0}", actions.ItemSerial);
                    	GameActions.OpenCorpse(actions.ItemSerial);
			        }
			        catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to check the items of the corpse: " + ex.Message);
		            }
	        	}
	        }
	        else if (actions.ActionType == 8) {
	        	if (World.Player != null) {
                    Console.WriteLine("actions.ActionType == 8");
                    try {
                    	UIManager.GetGump<ContainerGump>(actions.ItemSerial).CloseWindow();
                    }
                    catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to close the container gump of the corpse: " + ex.Message);
		            }

                    corpseItemDataList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 9) {
	        	if (World.Player != null) {
                    Console.WriteLine("actions.ActionType == 9");
	        		try 
		        	{
	                    Item item = World.Items.Get(actions.ItemSerial);
		        		Console.WriteLine("item: {0}, item.Items: {1}", item, item.Items);

			            for (LinkedObject i = item.Items; i != null; i = i.Next)
			            {
			                Item child = (Item) i;
			                Console.WriteLine("i test: {0}, child.Name: {1}, child.Serial: {2}", i, child.Name, child.Serial);
			                
		            		corpseItemDataList.Add(new GrpcItemData{ Name = child.Name, Layer = (uint) child.Layer,
			              									         Serial = (uint) child.Serial, Amount = (uint) child.Amount });
			            }
			        }
			        catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to save the corpse items: " + ex.Message);
		            }
	        	}
	        }
	        else if (actions.ActionType == 10) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 10");
	        		GameActions.OpenPopupMenu(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 11) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 11");
	        		GameActions.ResponsePopupMenu(actions.MobileSerial, (ushort) actions.Index);
	        	}
	        }
	        else if (actions.ActionType == 12) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 12");
	        		//Console.WriteLine("ItemSerial: {0}, Amount: {1}", actions.ItemSerial, actions.Amount);

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint)actions.ItemSerial, (ushort)actions.Amount);
	        		NetClient.Socket.Send_BuyRequest(actions.MobileSerial, items);

	        		UIManager.GetGump<ShopGump>(actions.MobileSerial).CloseWindow();
	        		grpcVendorItemList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 13) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 13");
	        		//Console.WriteLine("ItemSerial: {0}, Amount: {1}", actions.ItemSerial, actions.Amount);

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint)actions.ItemSerial, (ushort)actions.Amount);
	        		NetClient.Socket.Send_SellRequest(actions.MobileSerial, items);

	        		UIManager.GetGump<ShopGump>(actions.MobileSerial).CloseWindow();
	        		grpcVendorItemList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 14) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 14");
	        		Item bandage = World.Player.FindBandage();
	        		if (bandage != null) 
	        		{
	        			Console.WriteLine("Serial: {0}, Amount: {0}", bandage.Serial, bandage.Amount);
	        			NetClient.Socket.Send_TargetSelectedObject(bandage.Serial, World.Player.Serial);
	        		}
	        	}
	        }

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ActSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            //Console.WriteLine("Step 1:");
            _controller.sem_action.Release();

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ObsSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            //Console.WriteLine("Step 3:");
            _controller.sem_observation.WaitOne();

            return Task.FromResult(new Empty {});
        }
    }
}