using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
using Grpc.Core;
using Google.Protobuf;
using StormLibSharp;
using Uoservice;


namespace ClassicUO.Grpc
{
	internal partial class UoServiceImpl : UoService.UoServiceBase
    {
        GameController _controller;
        int _port;
        Server _grpcServer;

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

        public List<GrpcGameObjectData> grpcPlayerMobileObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcMobileObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectData> grpcItemObjectList = new List<GrpcGameObjectData>();
        public List<GrpcGameObjectSimpleData> grpcItemDropableLandSimpleList = new List<GrpcGameObjectSimpleData>();
        public List<GrpcGameObjectData> grpcVendorItemObjectList = new List<GrpcGameObjectData>();

        public List<string> grpcPopupMenuList = new List<string>();

        public List<GrpcClilocData> grpcClilocDataList = new List<GrpcClilocData>();

        int _envStep = 0;
        string _replayName;

        // ###############
    	List<int> mobileDataArrayLengthList = new List<int>();
    	List<int> worldItemArrayLengthList = new List<int>();
    	List<int> equippedItemArrayLengthList = new List<int>();
    	List<int> backpackItemArrayLengthList = new List<int>();
    	List<int> corpseItemArrayLengthList = new List<int>();
    	List<int> popupMenuArrayLengthList = new List<int>();
    	List<int> clilocDataArrayLengthList = new List<int>();

    	List<int> playerMobileObjectArrayLengthList = new List<int>();
        List<int> mobileObjectArrayLengthList = new List<int>();
        List<int> itemObjectArrayLengthList = new List<int>();
        List<int> itemDropableLandSimpleArrayLengthList = new List<int>();
        List<int> vendorItemObjectArrayLengthList = new List<int>();

        List<int> playerStatusArrayLengthList = new List<int>();

        // ###############
        byte[] mobileDataArrays;
		byte[] worldItemArrays;
		byte[] equippedItemArrays;
		byte[] backpackItemArrays;
		byte[] corpseItemArrays;
		byte[] popupMenuArrays;
		byte[] clilocDataArrays;

		byte[] playerMobileObjectArrays;
        byte[] mobileObjectArrays;
        byte[] itemObjectArrays;
        byte[] itemDropableLandSimpleArrays;
        byte[] vendorItemObjectArrays;

        byte[] playerStatusArrays;

        // ###############
    	List<int> actionTypeList = new List<int>();
    	List<int> walkDirectionList = new List<int>();

    	public uint actionType;
    	public uint walkDirection;
    	public uint mobileSerial;
    	public uint itemSerial;
    	public uint index;
    	public uint amount;
    	public uint openedCorpse;

        public UoServiceImpl(GameController controller, int port)
        {
            _controller = controller;
            _port = port;

            _grpcServer = new Server
	        {
	            Services = { UoService.BindService(this) },
	            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
	        };

	        actionType = 0;
	    	walkDirection = 0;
	    	mobileSerial = 0;
	    	itemSerial = 0;
	    	index = 0;
	    	amount = 0;
	    	openedCorpse = 0;
        }

        public void CreateMpqFile()
        {
        	Console.WriteLine("CreateMpqFile()");

        	string userName = Settings.GlobalSettings.Username;
	        DateTime currentTime = DateTime.Now;
			string currentYear = currentTime.Year.ToString();
			string currentMonth = currentTime.Month.ToString();
			string currentDay = currentTime.Day.ToString();
			string currentHour = currentTime.ToString("HH-mm-ss");

			_replayName = userName + "-" + currentYear + "-" + currentMonth + "-" + currentDay + "-" + currentHour;
			Console.WriteLine("_replayName: {0}", _replayName);

        	CreateMpqArchive("Replay/" + _replayName + ".uoreplay");
        }

        public void SaveReplayFile()
        {
        	Console.WriteLine("SaveReplayFile()");

        	foreach (int ary_len in mobileObjectArrayLengthList)
	        {
	            //Console.WriteLine("ary_len: {0}", ary_len);
	        }

            byte[] mobileDataArrayLengthArray = ConvertIntListToByteArray(mobileDataArrayLengthList);
            byte[] worldItemArrayLengthArray = ConvertIntListToByteArray(worldItemArrayLengthList);
            byte[] equippedItemArrayLengthArray = ConvertIntListToByteArray(equippedItemArrayLengthList);
            byte[] backpackItemArrayLengthArray = ConvertIntListToByteArray(backpackItemArrayLengthList);
            byte[] corpseItemArrayLengthArray = ConvertIntListToByteArray(corpseItemArrayLengthList);
            byte[] popupMenuArrayLengthArray = ConvertIntListToByteArray(popupMenuArrayLengthList);
            byte[] clilocDataArrayLengthArray = ConvertIntListToByteArray(clilocDataArrayLengthList);

	        byte[] playerMobileObjectArrayLengthArray = ConvertIntListToByteArray(playerMobileObjectArrayLengthList);
	    	byte[] mobileObjectArrayLengthArray = ConvertIntListToByteArray(mobileObjectArrayLengthList);
	    	byte[] itemObjectArrayLengthArray = ConvertIntListToByteArray(itemObjectArrayLengthList);
	    	byte[] itemDropableLandSimpleArrayLengthArray = ConvertIntListToByteArray(itemDropableLandSimpleArrayLengthList);
	    	byte[] vendorItemObjectArrayLengthArray = ConvertIntListToByteArray(vendorItemObjectArrayLengthList);

	    	byte[] playerStatusArrayLengthArray = ConvertIntListToByteArray(playerStatusArrayLengthList);

	    	byte[] actionTypeArray = ConvertIntListToByteArray(actionTypeList);
            byte[] walkDirectionArray = ConvertIntListToByteArray(walkDirectionList);

            // ###############
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.type", actionTypeArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.walkDirection", walkDirectionArray);

	    	// ###############
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.mobileDataLen", mobileDataArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.worldItemLen", worldItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.equippedItemLen", equippedItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.backpackitemLen", backpackItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.corpseItemLen", corpseItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.popupMenuLen", popupMenuArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.clilocDataLen", clilocDataArrayLengthArray);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerMobileObjectLen", playerMobileObjectArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.mobileObjectLen", mobileObjectArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.itemObjectLen", itemObjectArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.itemDropableLandSimpleLen", itemDropableLandSimpleArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.vendorItemObjectLen", vendorItemObjectArrayLengthArray);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerStatusLen", playerStatusArrayLengthArray);

            // ###############
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.mobileData", mobileDataArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.worldItem", worldItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.equippedItem", equippedItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.backpackItem", backpackItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.corpseItem", corpseItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.popupMenu", popupMenuArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.clilocData", clilocDataArrays);

	        WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.playerMobileObject", playerMobileObjectArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.mobileObject", mobileObjectArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.itemObject", itemObjectArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.itemDropableLandSimple", itemDropableLandSimpleArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.vendorItemObject", vendorItemObjectArrays);
        }

        public void AddClilocData(string text, string affix)
        {
        	grpcClilocDataList.Add(new GrpcClilocData{ Text=text, Affix=affix });
        }

        public void AddGameSimpleObject(string type, uint screen_x, uint screen_y, uint distance, uint game_x, uint game_y)
        {
        	try 
        	{
        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, screen_x, screen_y, distance, name);
        		bool can_drop = (distance >= 1) && (distance < Constants.DRAG_ITEMS_DISTANCE);
        		if (can_drop)
            	{
        			grpcItemDropableLandSimpleList.Add(new GrpcGameObjectSimpleData{ GameX=game_x, GameY=game_y });
        		}
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object: " + ex.Message);
            }
        }

        public void AddGameObject(string type, uint screen_x, uint screen_y, uint distance, uint game_x, uint game_y, 
        						  uint serial, string name, bool is_corpse, string title, uint amount, uint price)
        {
        	try 
        	{
        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, screen_x, screen_y, distance, name);
	        	if (type == "PlayerMobile") {
	        		grpcPlayerMobileObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        														 GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													     Title=title, Amount=amount, Price=price });
	        	}
	        	else if (type == "Item") {
	        		grpcItemObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
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
	        		grpcVendorItemObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
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
        	CreateMpqFile();
        }

        public override Task<States> Reset(Config config, ServerCallContext context)
        {
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

                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, X = (float) mob.GetScreenPosition().X, Y = (float) mob.GetScreenPosition().Y,
                										   Race = (uint) mob.Race });
            }

            States states = new States();

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            return Task.FromResult(states);
        }

        public override Task<States> ReadObs(Config config, ServerCallContext context)
        {
        	Console.WriteLine("_envStep: {0}", _envStep);

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

	                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, X = (float) mob.GetScreenPosition().X, Y = (float) mob.GetScreenPosition().Y,
	                										   Race = (uint) mob.Race, Serial = (uint) mob.Serial });
	            }
	        }
	        catch (Exception ex)
            {
            	//Console.WriteLine("Failed to load the mobile: " + ex.Message);
            	grpcMobileDataList.Add(new GrpcMobileData{ Name = "base", X = (float) 500.0, Y = (float) 500.0,
	                									   Race = (uint) 0, Serial = (uint) 1234 });
            }

            foreach (Item item in World.Items.Values)
            {
            	//Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, item.Amount, item.Serial);
            	worldItemDataList.Add(new GrpcItemData{ Name = !string.IsNullOrEmpty(item.Name) ? item.Name : "Null", Layer = (uint) item.Layer,
	              									    Serial = (uint) item.Serial, Amount = (uint) item.Amount });


            }

	        if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	foreach (Layer layer in _layerOrder) {
	        		Item item = World.Player.FindItemByLayer(layer);
	        		try 
	        		{
	            		equippedItemDataList.Add(new GrpcItemData{ Name = item.Name, Layer = (uint) item.Layer, 
	            												   Serial = (uint) item.Serial, Amount = (uint) item.Amount });
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
                    //Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, item.Amount, item.Serial);
            		try 
	        		{
	            		backpackItemDataList.Add(new GrpcItemData{ Name = item.Name,  Layer = (uint) item.Layer,
		              									           Serial = (uint) item.Serial, Amount = (uint) item.Amount });
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

		    if (openedCorpse != 0) 
		    {
		    	//Console.WriteLine("openedCorpse != 0");
			    try 
	        	{
	                Item item = World.Items.Get(openedCorpse);
	        		//Console.WriteLine("item: {0}, item.Items: {1}", item, item.Items);

		            for (LinkedObject i = item.Items; i != null; i = i.Next)
		            {
		                Item child = (Item) i;
		                //Console.WriteLine("i test: {0}, child.Name: {1}, child.Serial: {2}", i, child.Name, child.Serial);
		                
	            		corpseItemDataList.Add(new GrpcItemData{ Name = child.Name, Layer = (uint) child.Layer,
		              									         Serial = (uint) child.Serial, Amount = (uint) child.Amount });
		            }
		        }
		        catch (Exception ex)
	            {
	            	Console.WriteLine("Failed to save the corpse items: " + ex.Message);
	            }
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

            GrpcPopupMenuList popupMenuList = new GrpcPopupMenuList();
            popupMenuList.Menu.AddRange(grpcPopupMenuList);
            states.PopupMenuList = popupMenuList;

            GrpcClilocDataList clilocDataList = new GrpcClilocDataList();
            clilocDataList.ClilocData.AddRange(grpcClilocDataList);
            states.ClilocDataList = clilocDataList;

            states.PlayerStatus = playerStatus;

            try
            {
	            GrpcGameObjectList playerMobileObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList itemObjectList = new GrpcGameObjectList();
	            GrpcGameObjectList mobileObjectList = new GrpcGameObjectList();
	            GrpcGameObjectSimpleList itemDropableLandSimpleList = new GrpcGameObjectSimpleList();
	            GrpcGameObjectList vendorItemObjectList = new GrpcGameObjectList();

	            playerMobileObjectList.GameObject.AddRange(grpcPlayerMobileObjectList);
	            itemObjectList.GameObject.AddRange(grpcItemObjectList);
	            mobileObjectList.GameObject.AddRange(grpcMobileObjectList);
	            itemDropableLandSimpleList.GameSimpleObject.AddRange(grpcItemDropableLandSimpleList);
	            vendorItemObjectList.GameObject.AddRange(grpcVendorItemObjectList);

	            states.MobileObjectList = mobileObjectList;
	            states.PlayerMobileObjectList = playerMobileObjectList;
	            states.MobileObjectList = mobileObjectList;
	            states.ItemObjectList = itemObjectList;
	            states.ItemDropableLandList = itemDropableLandSimpleList;
	            states.VendorItemObjectList = vendorItemObjectList;

	            // ##################################################################################
	            byte[] mobileDataArray = grpcMobileList.ToByteArray();
	            byte[] worldItemArray = worldItemList.ToByteArray();
	            byte[] equippedItemArray = equippedItemList.ToByteArray();
	            byte[] backpackItemArray = backpackItemList.ToByteArray();
	            byte[] corpseItemArray = corpseItemList.ToByteArray();
	            byte[] popupMenuArray = popupMenuList.ToByteArray();
	            byte[] clilocDataArray = clilocDataList.ToByteArray();

	        	byte[] playerMobileObjectArray = playerMobileObjectList.ToByteArray();
	        	byte[] itemObjectArray = itemObjectList.ToByteArray();
	        	byte[] mobileObjectArray = mobileObjectList.ToByteArray();
	        	byte[] itemDropableLandSimpleArray = itemDropableLandSimpleList.ToByteArray();
	        	byte[] vendorItemObjectArray = vendorItemObjectList.ToByteArray();
	        	
	        	//Console.WriteLine("playerMobileObjectArray.Length: {0}" + playerMobileObjectArray.Length);
	        	//Console.WriteLine("itemObjectArray.Length: {0}" + itemObjectArray.Length);
	        	//Console.WriteLine("mobileObjectArray.Length: {0}" + mobileObjectArray.Length);
	        	//Console.WriteLine("itemDropableLandSimpleArray.Length: {0}" + itemDropableLandSimpleArray.Length);
	        	//Console.WriteLine("vendorItemObjectArray.Length: {0}" + vendorItemObjectArray.Length);
	        	//Console.WriteLine("\n");

	        	byte[] playerStatusArray = playerStatus.ToByteArray();

	        	if ( (playerMobileObjectArray.Length != 0) ) {
	            	if (_envStep == 0) 
	            	{
	            		mobileDataArrays = mobileDataArray;
	            		worldItemArrays = worldItemArray;
	            		equippedItemArrays = equippedItemArray;
	            		backpackItemArrays = backpackItemArray;
	            		corpseItemArrays = corpseItemArray;
	            		popupMenuArrays = popupMenuArray;
	            		clilocDataArrays = clilocDataArray;

	            		playerMobileObjectArrays = playerMobileObjectArray;
	            		mobileObjectArrays = mobileObjectArray;
	            		itemObjectArrays = itemObjectArray;
	            		itemDropableLandSimpleArrays = itemDropableLandSimpleArray;
	            		vendorItemObjectArrays = vendorItemObjectArray;

	            		playerStatusArrays = playerStatusArray;
	            	}
	            	else
	            	{
	            		/*
	            		mobileDataArrays = mobileDataArrays.Concat(mobileDataArray).ToArray();
	            		worldItemArrays = worldItemArrays.Concat(worldItemArray).ToArray();
	            		equippedItemArrays = equippedItemArrays.Concat(equippedItemArray).ToArray();
	            		backpackItemArrays = backpackItemArrays.Concat(backpackItemArray).ToArray();
	            		corpseItemArrays = corpseItemArrays.Concat(corpseItemArray).ToArray();
	            		popupMenuArrays = popupMenuArrays.Concat(popupMenuArray).ToArray();
	            		clilocDataArrays = clilocDataArrays.Concat(clilocDataArray).ToArray();
	            		
	            		playerMobileObjectArrays = playerMobileObjectArrays.Concat(playerMobileObjectArray).ToArray();
		            	mobileObjectArrays = mobileObjectArrays.Concat(mobileObjectArray).ToArray();
		            	itemObjectArrays = itemObjectArrays.Concat(itemObjectArray).ToArray();
		            	itemDropableLandSimpleArrays = itemDropableLandSimpleArrays.Concat(itemDropableLandSimpleArray).ToArray();
		            	vendorItemObjectArrays = vendorItemObjectArrays.Concat(vendorItemObjectArray).ToArray();

		            	playerStatusArrays = playerStatusArrays.Concat(playerStatusArray).ToArray();
		            	*/

		            	mobileDataArrays = ConcatByteArrays(mobileDataArrays, mobileDataArray);
		            	worldItemArrays = ConcatByteArrays(worldItemArrays, worldItemArray);
		            	equippedItemArrays = ConcatByteArrays(equippedItemArrays, equippedItemArray);
		            	backpackItemArrays = ConcatByteArrays(backpackItemArrays, backpackItemArray);
		            	corpseItemArrays = ConcatByteArrays(corpseItemArrays, corpseItemArray);
		            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArray);
		            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArray);

		            	playerMobileObjectArrays = ConcatByteArrays(playerMobileObjectArrays, playerMobileObjectArray);
		            	mobileObjectArrays = ConcatByteArrays(mobileObjectArrays, mobileObjectArray);
		            	itemObjectArrays = ConcatByteArrays(itemObjectArrays, itemObjectArray);
		            	itemDropableLandSimpleArrays = ConcatByteArrays(itemDropableLandSimpleArrays, itemDropableLandSimpleArray);
		            	vendorItemObjectArrays = ConcatByteArrays(vendorItemObjectArrays, vendorItemObjectArray);

		            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArray);
	            	}

					mobileDataArrayLengthList.Add((int) mobileDataArray.Length);
					worldItemArrayLengthList.Add((int) worldItemArray.Length);
					equippedItemArrayLengthList.Add((int) equippedItemArray.Length);
					backpackItemArrayLengthList.Add((int) backpackItemArray.Length);
					corpseItemArrayLengthList.Add((int) corpseItemArray.Length);
					popupMenuArrayLengthList.Add((int) popupMenuArray.Length);
					clilocDataArrayLengthList.Add((int) clilocDataArray.Length);

	            	playerMobileObjectArrayLengthList.Add((int) playerMobileObjectArray.Length);
	            	mobileObjectArrayLengthList.Add((int) mobileObjectArray.Length);
	            	itemObjectArrayLengthList.Add((int) itemObjectArray.Length);
	            	itemDropableLandSimpleArrayLengthList.Add((int) itemDropableLandSimpleArray.Length);
	            	vendorItemObjectArrayLengthList.Add((int) vendorItemObjectArray.Length);
					
	            	playerStatusArrayLengthList.Add((int) playerStatusArray.Length);

	            	byte[] mobileDataArrayLengthArray = ConvertIntListToByteArray(mobileDataArrayLengthList);
	            	byte[] playerMobileObjectArrayLengthArray = ConvertIntListToByteArray(playerMobileObjectArrayLengthList);
	            	//Console.WriteLine("mobileDataArrayLengthArray.Length: {0}" + mobileDataArrayLengthArray.Length);
	        		//Console.WriteLine("playerMobileObjectArrayLengthArray.Length: {0}" + playerMobileObjectArrayLengthArray.Length);
	        		//Console.WriteLine("mobileObjectArrays.Length: {0}" + mobileObjectArrays.Length);
	        		//sConsole.WriteLine("\n");

	            	_envStep++;

	            	if (_envStep == 1500) 
	            	{
	            		SaveReplayFile();
	            	}
	            }
	        }
	        catch (Exception ex)
            {
            	//Console.WriteLine("Failed to set the states of GRPC: " + ex.Message);
            }
            
            grpcMobileDataList.Clear();
	        worldItemDataList.Clear();
	        equippedItemDataList.Clear();
	        backpackItemDataList.Clear();

            grpcItemDropableLandSimpleList.Clear();
            grpcPlayerMobileObjectList.Clear();
            grpcMobileObjectList.Clear();
            grpcItemObjectList.Clear();

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(Actions actions, ServerCallContext context)
        {
        	//Console.WriteLine("actions.ActionType: {0}", actions.ActionType);
        	//Console.WriteLine("actions.MobileSerial: {0}", actions.MobileSerial);
        	//Console.WriteLine("actions.WalkDirection: {0}", actions.WalkDirection);

        	if (actionType != 0) 
		    {
		    	//Console.WriteLine("gameTick: {0}, actionType: {1}", _controller._gameTick, actionType);
		    }

            actionTypeList.Add((int) actionType);
    		walkDirectionList.Add((int) walkDirection);

    		if (actions.ActionType == 0) {
            	// Walk to Direction
            	if (World.InGame == true) {
            		//corpseItemDataList.Clear();
            	}
	        }
            else if (actions.ActionType == 1) {
            	// Walk to Direction
            	if (World.InGame == true) {
            		if (actions.WalkDirection == 0) {
	            		World.Player.Walk(Direction.Up, true);
	            	}
	            	else if (actions.WalkDirection == 2) {
	            		World.Player.Walk(Direction.Right, true);
	            	}	
	            	else if (actions.WalkDirection == 3) {
	            		World.Player.Walk(Direction.Left, true);
	            	}
	            	else if (actions.WalkDirection == 4) {
	            		World.Player.Walk(Direction.Down, true);
	            	}
            	}
	        }
	        else if (actions.ActionType == 2) {
	        	// Attack Target by it's Serial
	        	if (World.Player != null) {
        			GameActions.DoubleClick(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 3) {
	        	// Pick Up the amount of item by it's serial
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 3");
	        		try
	        		{
	        			Item item = World.Items.Get(actions.ItemSerial);
	        			Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
            																		     	 item.Amount, item.Serial);
	        			GameActions.PickUp(actions.ItemSerial, 0, 0, (int) actions.Amount);
					}
	        		catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to parse the item info: " + ex.Message);
		            }
	        	}
	        }
	        else if (actions.ActionType == 4) {
	        	// Drop the holded item into my backpack
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 4");
        			Item backpack = World.Player.FindItemByLayer(Layer.Backpack);
        			GameActions.DropItem(actions.ItemSerial, 0xFFFF, 0xFFFF, 0, backpack.Serial);
	        	}
	        }
	        else if (actions.ActionType == 5) {
	        	// Drop the holded item on land around Player
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 5");

	        		int randomNumber;
					Random RNG = new Random();
	        		int index = RNG.Next(grpcItemDropableLandSimpleList.Count);

	        		try
	        		{   
	        			GrpcGameObjectSimpleData selected = grpcItemDropableLandSimpleList[index];
	        			//Console.WriteLine("ItemSerial: {0}, GameX: {0}, GameY: {1}", selected.GameX, selected.GameY);
	        			GameActions.DropItem(actions.ItemSerial, (int) selected.GameX, (int) selected.GameY, 0, 0xFFFF_FFFF);
	        		}
	        		catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to fine the item dropable land: " + ex.Message);
		            }
	        	}
	        } 
	        else if (actions.ActionType == 6) {
	        	// Equip the holded item
	        	if (World.Player != null) {
                    GameActions.Equip();
	        	}
	        }
	        else if (actions.ActionType == 7) {
	        	// Open the Corpse of mobile by it's Serial
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 7");
                    try
                    {
                    	//Console.WriteLine("actions.ItemSerial: {0}", actions.ItemSerial);
                    	GameActions.OpenCorpse(actions.ItemSerial);
                    	openedCorpse = actions.ItemSerial;
			        }
			        catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to check the items of the corpse: " + ex.Message);
		            }
	        	}
	        }
	        else if (actions.ActionType == 8) {
	        	// Close the opened corpse
	        	if (World.Player != null) {
                    Console.WriteLine("actions.ActionType == 8");
                    try 
                    {
                    	UIManager.GetGump<ContainerGump>(actions.ItemSerial).CloseWindow();
                    	openedCorpse = 0;
                    }
                    catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to close the container gump of the corpse: " + ex.Message);
		            }

                    corpseItemDataList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 9) {
	        	// Close the opened corpse
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
	        		grpcClilocDataList.Clear();
	        		GameActions.OpenPopupMenu(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 11) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 11");
	        		GameActions.ResponsePopupMenu(actions.MobileSerial, (ushort) actions.Index);
	        		grpcPopupMenuList.Clear();
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
	        		grpcVendorItemObjectList.Clear();
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
	        		grpcVendorItemObjectList.Clear();
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
	        else if (actions.ActionType == 15) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 15");
	        		GameActions.OpenDoor();
	        	}
	        }
	        else if (actions.ActionType == 16) {
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 16");
        			GameActions.DropItem(actions.ItemSerial, 0xFFFF, 0xFFFF, 0, actions.MobileSerial);
	        	}
	        }

	        actionType = 0;
    		walkDirection = 0;
    		mobileSerial = 0;
    		itemSerial = 0;
    		amount = 0;
    		index = 0;

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ActSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            //Console.WriteLine("Step 1:");
            _controller.semAction.Release();

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ObsSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            //Console.WriteLine("Step 3:");
            _controller.semObservation.WaitOne();

            return Task.FromResult(new Empty {});
        }
    }
}