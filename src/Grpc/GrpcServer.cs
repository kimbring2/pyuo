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
        List<GrpcItemData> equippedItemDataList = new List<GrpcItemData>();
        List<GrpcItemData> backpackItemDataList = new List<GrpcItemData>();
        List<GrpcItemData> bankItemDataList = new List<GrpcItemData>();
        List<GrpcContainerData> openedCorpseDataList = new List<GrpcContainerData>();

        List<string> grpcPopupMenuList = new List<string>();
        List<GrpcClilocData> grpcClilocDataList = new List<GrpcClilocData>();

        List<GrpcGameObjectData> grpcPlayerMobileObjectList = new List<GrpcGameObjectData>();
        List<GrpcGameObjectData> grpcMobileObjectList = new List<GrpcGameObjectData>();
        List<GrpcGameObjectData> grpcItemObjectList = new List<GrpcGameObjectData>();
        List<GrpcGameObjectSimpleData> grpcItemDropableLandSimpleList = new List<GrpcGameObjectSimpleData>();
        List<GrpcGameObjectData> grpcVendorItemObjectList = new List<GrpcGameObjectData>();

        List<uint> grpcStaticObjectScreenXs = new List<uint>();
        List<uint> grpcStaticObjectScreenYs = new List<uint>();

        List<GrpcSkill> grpcPlayerSkillListList = new List<GrpcSkill>();

        int _totalStepScale = 2;
        int _envStep;
        string _replayName;

        // ##################################################################################
    	List<int> mobileDataArrayLengthList = new List<int>();
    	List<int> equippedItemArrayLengthList = new List<int>();
    	List<int> backpackItemArrayLengthList = new List<int>();
    	List<int> bankItemArrayLengthList = new List<int>();
    	List<int> openedCorpseArrayLengthList = new List<int>();
    	List<int> popupMenuArrayLengthList = new List<int>();
    	List<int> clilocDataArrayLengthList = new List<int>();

    	List<int> playerMobileObjectArrayLengthList = new List<int>();
        List<int> mobileObjectArrayLengthList = new List<int>();
        List<int> itemObjectArrayLengthList = new List<int>();
        List<int> itemDropableLandSimpleArrayLengthList = new List<int>();
        List<int> vendorItemObjectArrayLengthList = new List<int>();

        List<int> playerStatusArrayZeroLengthStepList = new List<int>();
        List<int> playerSkillListArrayLengthList = new List<int>();

        List<int> staticObjectInfoListArraysLengthList = new List<int>();

        // ##################################################################################
        byte[] mobileDataArrays;
		byte[] equippedItemArrays;
		byte[] backpackItemArrays;
		byte[] bankItemArrays;
		byte[] openedCorpseArrays;
		byte[] popupMenuArrays;
		byte[] clilocDataArrays;

		byte[] playerMobileObjectArrays;
        byte[] mobileObjectArrays;
        byte[] itemObjectArrays;
        byte[] itemDropableLandSimpleArrays;
        byte[] vendorItemObjectArrays;

        byte[] playerStatusArrays;
        byte[] playerSkillListArrays;

        byte[] staticObjectInfoListArrays;

        // ##################################################################################
        byte[] mobileDataArraysTemp;
		byte[] equippedItemArraysTemp;
		byte[] backpackItemArraysTemp;
		byte[] bankItemArraysTemp;
		byte[] openedCorpseArraysTemp;
		byte[] popupMenuArraysTemp;
		byte[] clilocDataArraysTemp;

		byte[] playerMobileObjectArraysTemp;
        byte[] mobileObjectArraysTemp;
        byte[] itemObjectArraysTemp;
        byte[] itemDropableLandSimpleArraysTemp;
        byte[] vendorItemObjectArraysTemp;

        byte[] playerStatusArraysTemp;
        byte[] playerSkillListArraysTemp;

        byte[] staticObjectInfoListArraysTemp;

        // ##################################################################################
    	List<int> actionTypeList = new List<int>();
    	List<int> walkDirectionList = new List<int>();
    	List<int> itemSerialList = new List<int>();
    	List<int> mobileSerialList = new List<int>();
    	List<int> indexList = new List<int>();
    	List<int> amountList = new List<int>();
    	List<bool> runList = new List<bool>();

    	uint actionType;
    	uint walkDirection;
    	uint itemSerial;
    	uint mobileSerial;
    	uint index;
    	uint amount;
    	bool run;

    	public void SetActionType(uint value)
	    {
	        actionType = value;
	    }

	    public void SetWalkDirection(uint value)
	    {
	        walkDirection = value;
	    }

	    public void SetItemSerial(uint value)
	    {
	        itemSerial = value;
	    }

	    public void SetMobileSerial(uint value)
	    {
	        mobileSerial = value;
	    }

	    public void SetIndex(uint value)
	    {
	        index = value;
	    }

	    public void SetAmount(uint value)
	    {
	        amount = value;
	    }

	    public void SetRun(bool value)
	    {
	        run = value;
	    }

	    public void AddToPopupMenuList(string text)
	    {
	        grpcPopupMenuList.Add(text);
	    }

	    public void ClearPopupMenuList()
	    {
	        grpcPopupMenuList.Clear();
	    }

	    public void ClearVendorItemObjectList()
	    {
	        grpcVendorItemObjectList.Clear();
	    }

        public UoServiceImpl(GameController controller, int port)
        {
        	Console.WriteLine("port: {0}", port);

            _controller = controller;
            _port = port;

            _grpcServer = new Server
	        {
	            Services = { UoService.BindService(this) },
	            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
	        };

	        _envStep = 0;

	        actionType = 0;
	    	walkDirection = 0;
	    	itemSerial = 0;
	    	mobileSerial = 0;
	    	index = 0;
	    	amount = 0;
	    	run = false;
        }

        private void Reset()
        {
        	Console.WriteLine("Reset()");

        	// Clear all List and Array before using them

        	// ##################################################################################
	    	mobileDataArrayLengthList.Clear();
	    	equippedItemArrayLengthList.Clear();
	    	backpackItemArrayLengthList.Clear();
	    	bankItemArrayLengthList.Clear();
	    	openedCorpseArrayLengthList.Clear();
	    	popupMenuArrayLengthList.Clear();
	    	clilocDataArrayLengthList.Clear();

	    	playerMobileObjectArrayLengthList.Clear();
	        mobileObjectArrayLengthList.Clear();
	        itemObjectArrayLengthList.Clear();
	        itemDropableLandSimpleArrayLengthList.Clear();
	        vendorItemObjectArrayLengthList.Clear();

	        playerStatusArrayZeroLengthStepList.Clear();
	        playerSkillListArrayLengthList.Clear();

	        staticObjectInfoListArraysLengthList.Clear();

	        // ##################################################################################
	        Array.Clear(mobileDataArrays, 0, mobileDataArrays.Length);
	        Array.Clear(equippedItemArrays, 0, equippedItemArrays.Length);
	        Array.Clear(backpackItemArrays, 0, backpackItemArrays.Length);
	        Array.Clear(bankItemArrays, 0, bankItemArrays.Length);
	        Array.Clear(openedCorpseArrays, 0, openedCorpseArrays.Length);
	        Array.Clear(popupMenuArrays, 0, popupMenuArrays.Length);
	        Array.Clear(clilocDataArrays, 0, clilocDataArrays.Length);

	        Array.Clear(playerMobileObjectArrays, 0, playerMobileObjectArrays.Length);
	        Array.Clear(mobileObjectArrays, 0, mobileObjectArrays.Length);
	        Array.Clear(itemObjectArrays, 0, itemObjectArrays.Length);
	        Array.Clear(itemDropableLandSimpleArrays, 0, itemDropableLandSimpleArrays.Length);
	        Array.Clear(vendorItemObjectArrays, 0, vendorItemObjectArrays.Length);

	        Array.Clear(playerStatusArrays, 0, playerStatusArrays.Length);
	        Array.Clear(playerSkillListArrays, 0, playerSkillListArrays.Length);

	        Array.Clear(staticObjectInfoListArrays, 0, staticObjectInfoListArrays.Length);

	        // ##################################################################################
			Array.Clear(mobileDataArraysTemp, 0, mobileDataArraysTemp.Length);
	        Array.Clear(equippedItemArraysTemp, 0, equippedItemArraysTemp.Length);
	        Array.Clear(backpackItemArraysTemp, 0, backpackItemArraysTemp.Length);
	        Array.Clear(bankItemArraysTemp, 0, bankItemArraysTemp.Length);
	        Array.Clear(openedCorpseArraysTemp, 0, openedCorpseArraysTemp.Length);
	        Array.Clear(popupMenuArraysTemp, 0, popupMenuArraysTemp.Length);
	        Array.Clear(clilocDataArraysTemp, 0, clilocDataArraysTemp.Length);

	        Array.Clear(playerMobileObjectArraysTemp, 0, playerMobileObjectArraysTemp.Length);
	        Array.Clear(mobileObjectArraysTemp, 0, mobileObjectArraysTemp.Length);
	        Array.Clear(itemObjectArraysTemp, 0, itemObjectArraysTemp.Length);
	        Array.Clear(itemDropableLandSimpleArraysTemp, 0, itemDropableLandSimpleArraysTemp.Length);
	        Array.Clear(vendorItemObjectArraysTemp, 0, vendorItemObjectArraysTemp.Length);

	        Array.Clear(playerStatusArraysTemp, 0, playerStatusArraysTemp.Length);
	        Array.Clear(playerSkillListArraysTemp, 0, playerSkillListArraysTemp.Length);

	        Array.Clear(staticObjectInfoListArraysTemp, 0, staticObjectInfoListArraysTemp.Length);

	        // ##################################################################################
    		actionTypeList.Clear();
    		walkDirectionList.Clear();
    		itemSerialList.Clear();
    		mobileSerialList.Clear();
    		indexList.Clear();
    		amountList.Clear();
    		runList.Clear();

    		// ##################################################################################
    		_envStep = 0;

    		// Action related values
    		actionType = 0;
	    	walkDirection = 0;
	    	itemSerial = 0;
	    	mobileSerial = 0;
	    	index = 0;
	    	amount = 0;
	    	run = false;
        }

        private void CreateMpqFile()
        {
        	Console.WriteLine("###########################################################");
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

        private void SaveReplayFile()
        {
        	//Console.WriteLine("SaveReplayFile()");

            byte[] mobileDataArrayLengthArray = ConvertIntListToByteArray(mobileDataArrayLengthList);
            byte[] equippedItemArrayLengthArray = ConvertIntListToByteArray(equippedItemArrayLengthList);
            byte[] backpackItemArrayLengthArray = ConvertIntListToByteArray(backpackItemArrayLengthList);
            byte[] bankItemArrayLengthArray = ConvertIntListToByteArray(bankItemArrayLengthList);
            byte[] openedCorpseArrayLengthArray = ConvertIntListToByteArray(openedCorpseArrayLengthList);
            byte[] popupMenuArrayLengthArray = ConvertIntListToByteArray(popupMenuArrayLengthList);
            byte[] clilocDataArrayLengthArray = ConvertIntListToByteArray(clilocDataArrayLengthList);

	        byte[] playerMobileObjectArrayLengthArray = ConvertIntListToByteArray(playerMobileObjectArrayLengthList);
	    	byte[] mobileObjectArrayLengthArray = ConvertIntListToByteArray(mobileObjectArrayLengthList);
	    	byte[] itemObjectArrayLengthArray = ConvertIntListToByteArray(itemObjectArrayLengthList);
	    	byte[] itemDropableLandSimpleArrayLengthArray = ConvertIntListToByteArray(itemDropableLandSimpleArrayLengthList);
	    	byte[] vendorItemObjectArrayLengthArray = ConvertIntListToByteArray(vendorItemObjectArrayLengthList);

	    	//Console.WriteLine("staticObjectInfoListArraysLengthList.Count: {0}", staticObjectInfoListArraysLengthList.Count);
	    	byte[] staticObjectInfoListArraysLengthArray = ConvertIntListToByteArray(staticObjectInfoListArraysLengthList);

	    	//Console.WriteLine("playerStatusArrayZeroLengthStepList.Count: {0}", playerStatusArrayZeroLengthStepList.Count);
	    	for (int i = 0; i < playerStatusArrayZeroLengthStepList.Count; i++)
	        {
	        	//Console.WriteLine("playerStatusArrayZeroLengthStepList[{0}]: {1}", i, playerStatusArrayZeroLengthStepList[i]);	
	        }

	        byte[] playerStatusArrayZeroLengthStepArray = ConvertIntListToByteArray(playerStatusArrayZeroLengthStepList);
	        byte[] playerSkillListArrayLengthArray = ConvertIntListToByteArray(playerSkillListArrayLengthList);

	    	//Console.WriteLine("actionTypeList.Count: {0}", actionTypeList.Count);
			//Console.WriteLine("walkDirectionList.Count: {0}", walkDirectionList.Count);
	    	byte[] actionTypeArray = ConvertIntListToByteArray(actionTypeList);
            byte[] walkDirectionArray = ConvertIntListToByteArray(walkDirectionList);
            byte[] itemSerialArray = ConvertIntListToByteArray(itemSerialList);
            byte[] mobileSerialArray = ConvertIntListToByteArray(mobileSerialList);
            byte[] indexArray = ConvertIntListToByteArray(indexList);
            byte[] amountArray = ConvertIntListToByteArray(amountList);
            byte[] runArray = ConvertBoolListToByteArray(runList);

            // ##################################################################################
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.type", actionTypeArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.walkDirection", walkDirectionArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.itemSerial", itemSerialArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.mobileSerial", mobileSerialArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.index", indexArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.amount", amountArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.action.run", runArray);

	    	// ##################################################################################
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.mobileDataLen", mobileDataArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.equippedItemLen", equippedItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.backpackitemLen", backpackItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.bankitemLen", bankItemArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.openedCorpseLen", openedCorpseArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.popupMenuLen", popupMenuArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.clilocDataLen", clilocDataArrayLengthArray);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerMobileObjectLen", playerMobileObjectArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.mobileObjectLen", mobileObjectArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.itemObjectLen", itemObjectArrayLengthArray);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.itemDropableLandSimpleLen", itemDropableLandSimpleArrayLengthArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.vendorItemObjectLen", vendorItemObjectArrayLengthArray);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerStatusZeroLenStep", playerStatusArrayZeroLengthStepArray);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerSkillListLen", playerSkillListArrayLengthArray);

            //Console.WriteLine("staticObjectInfoListArraysLengthArray.Length: {0}", staticObjectInfoListArraysLengthArray.Length);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.staticObjectInfoListArraysLen", staticObjectInfoListArraysLengthArray);

            // ##################################################################################   
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.mobileData", mobileDataArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.equippedItem", equippedItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.backpackItem", backpackItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.bankItem", bankItemArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.openedCorpse", openedCorpseArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.popupMenu", popupMenuArrays);
			WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.clilocData", clilocDataArrays);

	        WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.playerMobileObject", playerMobileObjectArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.mobileObject", mobileObjectArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.itemObject", itemObjectArrays);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.itemDropableLandSimple", itemDropableLandSimpleArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.vendorItemObject", vendorItemObjectArrays);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.playerStatus", playerStatusArrays);
            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.playerSkillList", playerSkillListArrays);

            WrtieToMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.staticObjectInfoList", staticObjectInfoListArrays);
        }

        public void AddClilocData(uint serial, string text, string affix, string name)
        {
        	grpcClilocDataList.Add(new GrpcClilocData{ Serial=serial, Text=text, Affix=affix, Name=name });
        }

        public void AddGameSimpleObject(string type, uint screen_x, uint screen_y, uint distance, uint game_x, uint game_y)
        {
        	//Console.WriteLine("AddGameSimpleObject()");

        	try 
        	{
        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, screen_x, screen_y, distance, name);
        		bool can_drop = (distance >= 1) && (distance < Constants.DRAG_ITEMS_DISTANCE);
        		if (can_drop)
            	{
            		//Console.WriteLine("game_x: {0}, game_y: {1}", game_x, game_y);
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
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, screen_x, screen_y, distance, name);
	        		grpcVendorItemObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        													   GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        													   Title=title, Amount=amount, Price=price });
	        	}
	        	else if (type == "Static") {
	        		if (distance <= 6) 
	        		{
	        			grpcStaticObjectScreenXs.Add(screen_x);
			        	grpcStaticObjectScreenYs.Add(screen_y);
	        		}
	        	}
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object: " + ex.Message);
            }
        }

        public void Start() 
        {
        	if (Settings.HumanPlay == false)
            {
                _grpcServer.Start();
            }

            if (Settings.Replay == true)
            {
                CreateMpqFile();
            }
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

        public States ReadObs()
        {
        	//Console.WriteLine("_envStep: {0}", _envStep);

        	if (_envStep % 1000 == 0)
        	{
        		Console.WriteLine("_envStep: {0}", _envStep);
        	}

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

	        // Add bank item
	        if ((World.Player != null) && (World.InGame == true))
	        {
	        	Item bank = World.Player.FindItemByLayer(Layer.Bank);
	            if (bank != null && bank.Opened)
	            {
	                if (!bank.IsEmpty)
	                {
	                    for (LinkedObject i = bank.Items; i != null; i = i.Next)
		                {
		                    Item item = (Item) i;
		                    uint itemSerial = (uint) item;

		                    World.OPL.TryGetNameAndData(itemSerial, out string name, out string data);
		                    //Console.WriteLine("OPL name: {0}, data: {1}", name, data);

		                    if (name != null)
		                    {
		                    	try 
				        		{
				        			//Console.WriteLine("bankItemDataList, Name: {0}, Amount: {1}", name, item.Amount);
				            		bankItemDataList.Add(new GrpcItemData{ Name = name,  Layer = (uint) item.Layer,
					              									       Serial = (uint) item.Serial, Amount = (uint) item.Amount });
				            	}
					            catch (Exception ex)
					            {
					            	Console.WriteLine("Failed to load the bank item: " + ex.Message);
					            }
		                    }
		                }
	                }
	            }
	        }

	        GrpcPlayerStatus playerStatus = new GrpcPlayerStatus();
	        if ((World.Player != null) && (World.InGame == true))
            {
		        playerStatus = new GrpcPlayerStatus { Str = (uint) World.Player.Strength, Dex = (uint) World.Player.Dexterity, 
		        								      Intell = (uint) World.Player.Intelligence, Hits = (uint) World.Player.Hits,
		        								      HitsMax = (uint) World.Player.HitsMax, Stamina = (uint) World.Player.Stamina,
		        								      StaminaMax = (uint) World.Player.StaminaMax, Mana = (uint) World.Player.Mana,
		        								      Gold = (uint) World.Player.Gold, PhysicalResistance = (uint) World.Player.PhysicalResistance,
		        								      Weight = (uint) World.Player.Weight, WeightMax = (uint) World.Player.WeightMax };
		    }

		    // Add corpse item
	        if ((World.Player != null) && (World.InGame == true))
	        {
		        foreach (uint corpseSerial in World.Player.ManualOpenedCorpses)
			    {
			    	Item corpseObj = World.Items.Get(corpseSerial);
			    	if (corpseObj != null)
            		{
				    	Vector2 corpseObjPos = corpseObj.GetScreenPosition();

						//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", "Corpse", corpseObjPos.X, corpseObjPos.Y, corpseObj.Distance, corpseObj.Name);
			        	GrpcGameObjectData corpse = new GrpcGameObjectData{ Type="Corpse", ScreenX=(uint) corpseObjPos.X, ScreenY=(uint) corpseObjPos.Y, 
			        												  		Distance=(uint) corpseObj.Distance, GameX=corpseObj.X, GameY=corpseObj.Y, 
			        												  		Serial=corpseSerial, Name=corpseObj.Name, IsCorpse=true, Title="None", 
			        												  		Amount=corpseObj.Amount, Price=corpseObj.Price };

			        	List<GrpcItemData> corpseItemDataList = new List<GrpcItemData>();										
			        	for (LinkedObject i = corpseObj.Items; i != null; i = i.Next)
			            {
			                Item child = (Item) i;
			                if (child.Name != null)
			                {
			                	//Console.WriteLine("Name: {0}, Serial: {1}, Amount: {2}, ", child.Name, child.Serial, child.Amount);
		            			corpseItemDataList.Add(new GrpcItemData{ Name = child.Name, Layer = (uint) child.Layer,
			              									         	Serial = (uint) child.Serial, Amount = (uint) child.Amount });
		            		}
			            }

			            GrpcItemList corpseItemList = new GrpcItemList();
			            corpseItemList.Item.AddRange(corpseItemDataList);

			        	GrpcContainerData openedCorpse = new GrpcContainerData{ Container = corpse, ContainerItemList = corpseItemList };

			        	openedCorpseDataList.Add(openedCorpse);
			        }
			    }
			}

			// Add player skill 
	        if ((World.Player != null) && (World.InGame == true))
	        {
	            //Console.WriteLine("\nWorld.Player.Skills.Length: {0}", World.Player.Skills.Length);
		        for (int i = 0; i < World.Player.Skills.Length; i++)
	            {
	            	Skill skill = World.Player.Skills[i];
	            	//Console.WriteLine("Name: {0}, Index: {1}, IsClickable: {2}, Value: {3}, Base: {4}, Cap: {5}, Lock: {6}", 
	            	//	skill.Name, skill.Index, skill.IsClickable, skill.Value, skill.Base, skill.Cap, skill.Lock);
	            	grpcPlayerSkillListList.Add(new GrpcSkill{ Name = skill.Name, Index = (uint) skill.Index, IsClickable = (bool) skill.IsClickable,
		              								           Value = (uint) skill.Value, Base = (uint) skill.Base, Cap = (uint) skill.Cap, 
		              								           Lock = (uint) skill.Lock });
	            }
	        }
 
            States states = new States();

			GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            GrpcItemList equippedItemList = new GrpcItemList();
            equippedItemList.Item.AddRange(equippedItemDataList);
            states.EquippedItemList = equippedItemList;

            GrpcItemList backpackItemList = new GrpcItemList();
            backpackItemList.Item.AddRange(backpackItemDataList);
            states.BackpackItemList = backpackItemList;

            GrpcItemList bankItemList = new GrpcItemList();
            bankItemList.Item.AddRange(bankItemDataList);
            states.BankItemList = bankItemList;

            GrpcContainerDataList openedCorpseList = new GrpcContainerDataList();
            openedCorpseList.Containers.AddRange(openedCorpseDataList);
            states.OpenedCorpseList = openedCorpseList;

            GrpcPopupMenuList popupMenuList = new GrpcPopupMenuList();
            popupMenuList.Menu.AddRange(grpcPopupMenuList);
            states.PopupMenuList = popupMenuList;

            GrpcClilocDataList clilocDataList = new GrpcClilocDataList();
            clilocDataList.ClilocData.AddRange(grpcClilocDataList);
            states.ClilocDataList = clilocDataList;

            GrpcSkillList playerSkillList = new GrpcSkillList();
            playerSkillList.Skills.AddRange(grpcPlayerSkillListList);
            states.PlayerSkillList = playerSkillList;

            states.PlayerStatus = playerStatus;

            GrpcGameObjectInfoList gameObjectInfoList = new GrpcGameObjectInfoList();
            gameObjectInfoList.ScreenXs.AddRange(grpcStaticObjectScreenXs);
            gameObjectInfoList.ScreenYs.AddRange(grpcStaticObjectScreenYs);
            states.StaticObjectInfoList = gameObjectInfoList;

            GrpcGameObjectList playerMobileObjectList = new GrpcGameObjectList();
            GrpcGameObjectList mobileObjectList = new GrpcGameObjectList();
            GrpcGameObjectList itemObjectList = new GrpcGameObjectList();
            GrpcGameObjectSimpleList itemDropableLandSimpleList = new GrpcGameObjectSimpleList();
            GrpcGameObjectList vendorItemObjectList = new GrpcGameObjectList();

            playerMobileObjectList.GameObject.AddRange(grpcPlayerMobileObjectList);
            mobileObjectList.GameObject.AddRange(grpcMobileObjectList);
            itemObjectList.GameObject.AddRange(grpcItemObjectList);
            itemDropableLandSimpleList.GameSimpleObject.AddRange(grpcItemDropableLandSimpleList);
            vendorItemObjectList.GameObject.AddRange(grpcVendorItemObjectList);

            states.PlayerMobileObjectList = playerMobileObjectList;
            states.MobileObjectList = mobileObjectList;
            states.ItemObjectList = itemObjectList;
            states.ItemDropableLandList = itemDropableLandSimpleList;
            states.VendorItemObjectList = vendorItemObjectList;

            // ##################################################################################
            byte[] mobileDataArray = grpcMobileList.ToByteArray();
            byte[] equippedItemArray = equippedItemList.ToByteArray();
            byte[] backpackItemArray = backpackItemList.ToByteArray();
            byte[] bankItemArray = bankItemList.ToByteArray();
            byte[] openedCorpseArray = openedCorpseList.ToByteArray();
            byte[] popupMenuArray = popupMenuList.ToByteArray();
            byte[] clilocDataArray = clilocDataList.ToByteArray();

        	byte[] playerMobileObjectArray = playerMobileObjectList.ToByteArray();
        	byte[] mobileObjectArray = mobileObjectList.ToByteArray();
        	byte[] itemObjectArray = itemObjectList.ToByteArray();
        	byte[] itemDropableLandSimpleArray = itemDropableLandSimpleList.ToByteArray();
        	byte[] vendorItemObjectArray = vendorItemObjectList.ToByteArray();
        	
        	byte[] playerStatusArray = playerStatus.ToByteArray();
        	byte[] playerSkillListArray = playerSkillList.ToByteArray();
        	//Console.WriteLine("playerStatusArray.Length: {0}", playerStatusArray.Length);

        	byte[] gameObjectInfoListArray = gameObjectInfoList.ToByteArray();
        	//Console.WriteLine("gameObjectInfoListArray.Length: {0}", gameObjectInfoListArray.Length);

        	if (playerStatusArray.Length != 30) 
        	{
        		//Console.WriteLine("playerStatusArray.Length != 30(), _envStep: {0}", _envStep);
        		playerStatusArrayZeroLengthStepList.Add(_envStep);
        	}

        	if (_envStep == 0) 
        	{
        		mobileDataArraysTemp = mobileDataArray;
        		equippedItemArraysTemp = equippedItemArray;
        		backpackItemArraysTemp = backpackItemArray;
        		bankItemArraysTemp = bankItemArray;
        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerMobileObjectArraysTemp = playerMobileObjectArray;
        		mobileObjectArraysTemp = mobileObjectArray;
        		itemObjectArraysTemp = itemObjectArray;
        		itemDropableLandSimpleArraysTemp = itemDropableLandSimpleArray;
        		vendorItemObjectArraysTemp = vendorItemObjectArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
            	mobileDataArrays = mobileDataArraysTemp;
            	equippedItemArrays = equippedItemArraysTemp;
            	backpackItemArrays = backpackItemArraysTemp;
            	bankItemArrays = bankItemArraysTemp;
            	openedCorpseArrays = openedCorpseArraysTemp;
            	popupMenuArrays = popupMenuArraysTemp;
            	clilocDataArrays = clilocDataArraysTemp;

            	playerMobileObjectArrays = playerMobileObjectArraysTemp;
            	mobileObjectArrays = mobileObjectArraysTemp;
            	itemObjectArrays = itemObjectArraysTemp;
            	itemDropableLandSimpleArrays = itemDropableLandSimpleArraysTemp;
            	vendorItemObjectArrays = vendorItemObjectArraysTemp;

            	playerStatusArrays = playerStatusArraysTemp;
            	playerSkillListArrays = playerSkillListArraysTemp;

				staticObjectInfoListArrays = staticObjectInfoListArraysTemp;

				// ##################################################################################
        		mobileDataArraysTemp = mobileDataArray;
        		equippedItemArraysTemp = equippedItemArray;
        		backpackItemArraysTemp = backpackItemArray;
        		bankItemArraysTemp = bankItemArray;
        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerMobileObjectArraysTemp = playerMobileObjectArray;
        		mobileObjectArraysTemp = mobileObjectArray;
        		itemObjectArraysTemp = itemObjectArray;
        		itemDropableLandSimpleArraysTemp = itemDropableLandSimpleArray;
        		vendorItemObjectArraysTemp = vendorItemObjectArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
            	mobileDataArrays = ConcatByteArrays(mobileDataArrays, mobileDataArraysTemp);
            	equippedItemArrays = ConcatByteArrays(equippedItemArrays, equippedItemArraysTemp);
            	backpackItemArrays = ConcatByteArrays(backpackItemArrays, backpackItemArraysTemp);
            	bankItemArrays = ConcatByteArrays(bankItemArrays, bankItemArraysTemp);
            	openedCorpseArrays = ConcatByteArrays(openedCorpseArrays, openedCorpseArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);

            	playerMobileObjectArrays = ConcatByteArrays(playerMobileObjectArrays, playerMobileObjectArraysTemp);
            	mobileObjectArrays = ConcatByteArrays(mobileObjectArrays, mobileObjectArraysTemp);
            	itemObjectArrays = ConcatByteArrays(itemObjectArrays, itemObjectArraysTemp);
            	itemDropableLandSimpleArrays = ConcatByteArrays(itemDropableLandSimpleArrays, itemDropableLandSimpleArraysTemp);
            	vendorItemObjectArrays = ConcatByteArrays(vendorItemObjectArrays, vendorItemObjectArraysTemp);

            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);

            	staticObjectInfoListArrays = ConcatByteArrays(staticObjectInfoListArrays, staticObjectInfoListArraysTemp);

				// ##################################################################################
        		mobileDataArraysTemp = mobileDataArray;
        		equippedItemArraysTemp = equippedItemArray;
        		backpackItemArraysTemp = backpackItemArray;
        		bankItemArraysTemp = bankItemArray;
        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerMobileObjectArraysTemp = playerMobileObjectArray;
        		mobileObjectArraysTemp = mobileObjectArray;
        		itemObjectArraysTemp = itemObjectArray;
        		itemDropableLandSimpleArraysTemp = itemDropableLandSimpleArray;
        		vendorItemObjectArraysTemp = vendorItemObjectArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
            	mobileDataArrays = ConcatByteArrays(mobileDataArrays, mobileDataArraysTemp);
            	equippedItemArrays = ConcatByteArrays(equippedItemArrays, equippedItemArraysTemp);
            	backpackItemArrays = ConcatByteArrays(backpackItemArrays, backpackItemArraysTemp);
            	bankItemArrays = ConcatByteArrays(bankItemArrays, bankItemArraysTemp);
            	openedCorpseArrays = ConcatByteArrays(openedCorpseArrays, openedCorpseArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);

            	playerMobileObjectArrays = ConcatByteArrays(playerMobileObjectArrays, playerMobileObjectArraysTemp);
            	mobileObjectArrays = ConcatByteArrays(mobileObjectArrays, mobileObjectArraysTemp);
            	itemObjectArrays = ConcatByteArrays(itemObjectArrays, itemObjectArraysTemp);
            	itemDropableLandSimpleArrays = ConcatByteArrays(itemDropableLandSimpleArrays, itemDropableLandSimpleArraysTemp);
            	vendorItemObjectArrays = ConcatByteArrays(vendorItemObjectArrays, vendorItemObjectArraysTemp);

            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);

            	staticObjectInfoListArrays = ConcatByteArrays(staticObjectInfoListArrays, staticObjectInfoListArraysTemp);
				
            	if (Settings.Replay == true)
	            {
	                SaveReplayFile();
	        		CreateMpqFile();
	        		Reset();
	            }
	            else
	            {
	            	Reset();
	            }

        		return states;
        	}
        	else
        	{
        		mobileDataArraysTemp = ConcatByteArrays(mobileDataArraysTemp, mobileDataArray);
            	equippedItemArraysTemp = ConcatByteArrays(equippedItemArraysTemp, equippedItemArray);
            	backpackItemArraysTemp = ConcatByteArrays(backpackItemArraysTemp, backpackItemArray);
            	bankItemArraysTemp = ConcatByteArrays(bankItemArraysTemp, bankItemArray);
            	openedCorpseArraysTemp = ConcatByteArrays(openedCorpseArraysTemp, openedCorpseArray);
            	popupMenuArraysTemp = ConcatByteArrays(popupMenuArraysTemp, popupMenuArray);
            	clilocDataArraysTemp = ConcatByteArrays(clilocDataArraysTemp, clilocDataArray);

            	playerMobileObjectArraysTemp = ConcatByteArrays(playerMobileObjectArraysTemp, playerMobileObjectArray);
            	mobileObjectArraysTemp = ConcatByteArrays(mobileObjectArraysTemp, mobileObjectArray);
            	itemObjectArraysTemp = ConcatByteArrays(itemObjectArraysTemp, itemObjectArray);
            	itemDropableLandSimpleArraysTemp = ConcatByteArrays(itemDropableLandSimpleArraysTemp, itemDropableLandSimpleArray);
            	vendorItemObjectArraysTemp = ConcatByteArrays(vendorItemObjectArraysTemp, vendorItemObjectArray);

            	playerStatusArraysTemp = ConcatByteArrays(playerStatusArraysTemp, playerStatusArray);
            	playerSkillListArraysTemp = ConcatByteArrays(playerSkillListArraysTemp, playerSkillListArray);

            	staticObjectInfoListArraysTemp = ConcatByteArrays(staticObjectInfoListArraysTemp, gameObjectInfoListArray);
        	}

        	// ##################################################################################
        	playerSkillListArrayLengthList.Add((int) playerSkillListArray.Length);

			mobileDataArrayLengthList.Add((int) mobileDataArray.Length);
			equippedItemArrayLengthList.Add((int) equippedItemArray.Length);
			backpackItemArrayLengthList.Add((int) backpackItemArray.Length);
			bankItemArrayLengthList.Add((int) bankItemArray.Length);
			openedCorpseArrayLengthList.Add((int) openedCorpseArray.Length);
			popupMenuArrayLengthList.Add((int) popupMenuArray.Length);
			clilocDataArrayLengthList.Add((int) clilocDataArray.Length);

        	playerMobileObjectArrayLengthList.Add((int) playerMobileObjectArray.Length);
        	mobileObjectArrayLengthList.Add((int) mobileObjectArray.Length);
        	itemObjectArrayLengthList.Add((int) itemObjectArray.Length);
        	itemDropableLandSimpleArrayLengthList.Add((int) itemDropableLandSimpleArray.Length);
        	vendorItemObjectArrayLengthList.Add((int) vendorItemObjectArray.Length);

        	staticObjectInfoListArraysLengthList.Add((int) gameObjectInfoListArray.Length);
			
        	// ##################################################################################
        	_envStep++;

        	grpcPlayerSkillListList.Clear();

	        grpcMobileDataList.Clear();
	        equippedItemDataList.Clear();
	        backpackItemDataList.Clear();
	        bankItemDataList.Clear();

	        openedCorpseDataList.Clear();

            grpcItemDropableLandSimpleList.Clear();
            grpcPlayerMobileObjectList.Clear();
            grpcMobileObjectList.Clear();
            grpcItemObjectList.Clear();

	        grpcStaticObjectScreenXs.Clear();
	        grpcStaticObjectScreenYs.Clear();

	        return states;
        }

        public override Task<States> ReadObs(Config config, ServerCallContext context)
        {
        	States obs = ReadObs();

            return Task.FromResult(obs);
        }

        public void WriteAct()
        {
            if ( (actionType != 1) && (actionType != 0) ) 
		    {
		    	Console.WriteLine("Tick:{0},Type:{1},Selected:{2},Target:{3},Index:{4},Amount:{5},Direction:{6},Run:{7}", 
		    		_controller._gameTick, actionType, itemSerial, mobileSerial, index, amount, walkDirection, run);
		    }

		    actionTypeList.Add((int) actionType);
			walkDirectionList.Add((int) walkDirection);
			itemSerialList.Add((int) itemSerial);
			mobileSerialList.Add((int) mobileSerial);
			indexList.Add((int) index);
			amountList.Add((int) amount);
			runList.Add((bool) run);

			actionType = 0;
	    	walkDirection = 0;
	    	itemSerial = 0;
	    	mobileSerial = 0;
	    	index = 0;
	    	amount = 0;
	    	run = false;
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(Actions actions, ServerCallContext context)
        {
    		if (actions.ActionType == 0) {
            	if (World.InGame == true) {

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
	        	// Attack target by it's serial
	        	if (World.Player != null) {
        			GameActions.DoubleClick(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 3) {
	        	// Pick up the amount of item by it's serial
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
	        	// Drop the holded item on land around the player
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 5");

	        		int randomNumber;
					Random RNG = new Random();
	        		int index = RNG.Next(grpcItemDropableLandSimpleList.Count);
	        		try
	        		{   
	        			GrpcGameObjectSimpleData selected = grpcItemDropableLandSimpleList[index];
	        			//Console.WriteLine("mobileSerial: {0}, GameX: {0}, GameY: {1}", selected.GameX, selected.GameY);
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
	        	// Open the corpse of mobile by it's serial
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 7");
                    try
                    {
                    	//Console.WriteLine("actions.mobileSerial: {0}", actions.mobileSerial);
                    	GameActions.OpenCorpse(actions.ItemSerial);
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
                    }
                    catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to close the container gump of the corpse: " + ex.Message);
		            }
	        	}
	        }
	        else if (actions.ActionType == 9) {
	        	// Update the items of opened corpse 
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
			                
		            		//corpseItemDataList.Add(new GrpcItemData{ Name = child.Name, Layer = (uint) child.Layer,
			              	//								         Serial = (uint) child.Serial, Amount = (uint) child.Amount });
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
	        		// Open the pop up menu of the vendor/teacher
	        		Console.WriteLine("actions.ActionType == 10");
	        		grpcClilocDataList.Clear();
	        		GameActions.OpenPopupMenu(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 11) {
	        	if (World.Player != null) {
	        		// Select one of menu from the pop up menu the vendor/teacher
	        		Console.WriteLine("actions.ActionType == 11");
	        		GameActions.ResponsePopupMenu(actions.MobileSerial, (ushort) actions.Index);
	        		grpcPopupMenuList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 12) {
	        	if (World.Player != null) {
	        		// Buy the item from vendor by selecing the item from shop gump
	        		Console.WriteLine("actions.ActionType == 12");

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint) actions.ItemSerial, (ushort) actions.Amount);
	        		NetClient.Socket.Send_BuyRequest(actions.MobileSerial, items);

	        		UIManager.GetGump<ShopGump>(actions.MobileSerial).CloseWindow();
	        		grpcVendorItemObjectList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 13) {
	        	if (World.Player != null) {
	        		// Sell the item to vendor by selecing the item from shop gump
	        		Console.WriteLine("actions.ActionType == 13");

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint) actions.ItemSerial, (ushort) actions.Amount);
	        		NetClient.Socket.Send_SellRequest(actions.MobileSerial, items);

	        		UIManager.GetGump<ShopGump>(actions.MobileSerial).CloseWindow();
	        		grpcVendorItemObjectList.Clear();
	        	}
	        }
	        else if (actions.ActionType == 14) {
	        	if (World.Player != null) {
	        		// Use the bandage myself
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
	        		// Open the door in front of player
	        		Console.WriteLine("actions.ActionType == 15");
	        		GameActions.OpenDoor();
	        	}
	        }
	        else if (actions.ActionType == 16) {
	        	if (World.Player != null) {
	        		// Drop the item to one of mobile(vendor)
	        		Console.WriteLine("actions.ActionType == 16");
        			GameActions.DropItem(actions.ItemSerial, 0xFFFF, 0xFFFF, 0, actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 17) {
	        	if (World.Player != null) {
	        		// Close the pop up menu
	        		Console.WriteLine("actions.ActionType == 17");
	        		UIManager.ShowGamePopup(null);
	        	}
	        }
	        else if (actions.ActionType == 18) {
	        	if (World.Player != null) {
	        		// Drop the item to the bank
	        		Console.WriteLine("actions.ActionType == 18");
        			Item bank = World.Player.FindItemByLayer(Layer.Bank);
        			GameActions.DropItem(actions.ItemSerial, 0xFFFF, 0xFFFF, 0, bank);
	        	}
	        }

	        actionType = 0;
    		walkDirection = 0;
    		itemSerial = 0;
    		mobileSerial = 0;
    		amount = 0;
    		index = 0;
    		run = false;

            return Task.FromResult(new Empty {});
        }

        public void ActSemaphoreControl()
        {
        	//Console.WriteLine("Step 1");
            _controller.semAction.Release();
        }

        public void ObsSemaphoreControl()
        {
        	//Console.WriteLine("Step 3");
            _controller.semObservation.WaitOne();
        }

        public override Task<Empty> ActSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            //_controller.semAction.Release();
            ActSemaphoreControl();

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ObsSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            ObsSemaphoreControl();

            return Task.FromResult(new Empty {});
        }
    }
}