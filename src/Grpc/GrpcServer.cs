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

        List<GrpcGameObjectData> worldItemObjectList = new List<GrpcGameObjectData>();
        List<GrpcGameObjectData> worldMobileObjectList = new List<GrpcGameObjectData>();

        List<uint> equippedItemSerialList = new List<uint>();
        List<uint> backpackItemSerialList = new List<uint>();
        List<uint> bankItemSerialList = new List<uint>();
        List<uint> vendorItemSerialList = new List<uint>();

        List<GrpcContainerData> openedCorpseDataList = new List<GrpcContainerData>();

        List<string> grpcPopupMenuList = new List<string>();
        List<GrpcClilocData> grpcClilocDataList = new List<GrpcClilocData>();

        List<uint> scenePlayerMobileSerialList = new List<uint>();
        List<uint> sceneMobileSerialList = new List<uint>();
        List<uint> sceneItemSerialList = new List<uint>();

        List<GrpcGameObjectSimpleData> grpcItemDropableLandSimpleList = new List<GrpcGameObjectSimpleData>();

        List<uint> grpcStaticObjectScreenXs = new List<uint>();
        List<uint> grpcStaticObjectScreenYs = new List<uint>();

        List<GrpcSkill> grpcPlayerSkillListList = new List<GrpcSkill>();

        int _totalStepScale = 2;
        int _envStep;
        string _replayName;
        string _replayPath;

        // ##################################################################################
        List<int> worldItemArrayLengthList = new List<int>();
        List<int> worldMobileArrayLengthList = new List<int>();

    	List<int> equippedItemArrayLengthList = new List<int>();
    	List<int> backpackItemArrayLengthList = new List<int>();
    	List<int> bankItemArrayLengthList = new List<int>();

    	List<int> openedCorpseArrayLengthList = new List<int>();
    	List<int> popupMenuArrayLengthList = new List<int>();
    	List<int> clilocDataArrayLengthList = new List<int>();

    	List<int> playerMobileObjectArrayLengthList = new List<int>();
        List<int> mobileObjectArrayLengthList = new List<int>();
        List<int> itemObjectArrayLengthList = new List<int>();
        List<int> vendorItemObjectArrayLengthList = new List<int>();

        List<int> playerStatusArrayLengthList = new List<int>();
        List<int> playerSkillListArrayLengthList = new List<int>();

        List<int> staticObjectInfoListArraysLengthList = new List<int>();

        // ##################################################################################
        byte[] worldItemArrays;

		byte[] equippedItemArrays;
		byte[] backpackItemArrays;
		byte[] bankItemArrays;
		byte[] openedCorpseArrays;
		byte[] popupMenuArrays;
		byte[] clilocDataArrays;

		byte[] playerMobileObjectArrays;
        byte[] mobileObjectArrays;
        byte[] itemObjectArrays;
 
        byte[] vendorItemObjectArrays;

        byte[] playerStatusArrays;
        byte[] playerSkillListArrays;

        byte[] staticObjectInfoListArrays;

        // ##################################################################################
        byte[] worldItemArraysTemp;

		byte[] equippedItemArraysTemp;
		byte[] backpackItemArraysTemp;
		byte[] bankItemArraysTemp;
		byte[] openedCorpseArraysTemp;
		byte[] popupMenuArraysTemp;
		byte[] clilocDataArraysTemp;

		byte[] playerMobileObjectArraysTemp;
        byte[] mobileObjectArraysTemp;
        byte[] itemObjectArraysTemp;
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

    	uint preActionType;
    	uint preWalkDirection;
    	uint preItemSerial;
    	uint preMobileSerial;
    	uint preIndex;
    	uint preAmount;
    	bool preRun;

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
	        vendorItemSerialList.Clear();
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

	    	preActionType = actionType;
	    	preWalkDirection = walkDirection;
	    	preItemSerial = itemSerial;
	    	preMobileSerial = mobileSerial;
	    	preIndex = index;
	    	preAmount = amount;
	    	preRun = run;
        }

        private void Reset()
        {
        	Console.WriteLine("Reset()");

        	// Clear all List and Array before using them

        	// ##################################################################################
        	worldItemArrayLengthList.Clear();

	    	equippedItemArrayLengthList.Clear();
	    	backpackItemArrayLengthList.Clear();
	    	bankItemArrayLengthList.Clear();
	    	openedCorpseArrayLengthList.Clear();
	    	popupMenuArrayLengthList.Clear();
	    	clilocDataArrayLengthList.Clear();

	    	playerMobileObjectArrayLengthList.Clear();
	        mobileObjectArrayLengthList.Clear();
	        itemObjectArrayLengthList.Clear();
	        vendorItemObjectArrayLengthList.Clear();

	        playerStatusArrayLengthList.Clear();
	        playerSkillListArrayLengthList.Clear();

	        staticObjectInfoListArraysLengthList.Clear();

	        // ##################################################################################
	        Array.Clear(worldItemArrays, 0, worldItemArrays.Length);

	        Array.Clear(equippedItemArrays, 0, equippedItemArrays.Length);
	        Array.Clear(backpackItemArrays, 0, backpackItemArrays.Length);
	        Array.Clear(bankItemArrays, 0, bankItemArrays.Length);
	        Array.Clear(openedCorpseArrays, 0, openedCorpseArrays.Length);
	        Array.Clear(popupMenuArrays, 0, popupMenuArrays.Length);
	        Array.Clear(clilocDataArrays, 0, clilocDataArrays.Length);

	        Array.Clear(playerMobileObjectArrays, 0, playerMobileObjectArrays.Length);
	        Array.Clear(mobileObjectArrays, 0, mobileObjectArrays.Length);
	        Array.Clear(itemObjectArrays, 0, itemObjectArrays.Length);
	        Array.Clear(vendorItemObjectArrays, 0, vendorItemObjectArrays.Length);

	        Array.Clear(playerStatusArrays, 0, playerStatusArrays.Length);
	        Array.Clear(playerSkillListArrays, 0, playerSkillListArrays.Length);

	        Array.Clear(staticObjectInfoListArrays, 0, staticObjectInfoListArrays.Length);

	        // ##################################################################################
	        Array.Clear(worldItemArraysTemp, 0, worldItemArraysTemp.Length);

	        Array.Clear(equippedItemArraysTemp, 0, equippedItemArraysTemp.Length);
	        Array.Clear(backpackItemArraysTemp, 0, backpackItemArraysTemp.Length);
	        Array.Clear(bankItemArraysTemp, 0, bankItemArraysTemp.Length);
	        Array.Clear(openedCorpseArraysTemp, 0, openedCorpseArraysTemp.Length);
	        Array.Clear(popupMenuArraysTemp, 0, popupMenuArraysTemp.Length);
	        Array.Clear(clilocDataArraysTemp, 0, clilocDataArraysTemp.Length);

	        Array.Clear(playerMobileObjectArraysTemp, 0, playerMobileObjectArraysTemp.Length);
	        Array.Clear(mobileObjectArraysTemp, 0, mobileObjectArraysTemp.Length);
	        Array.Clear(itemObjectArraysTemp, 0, itemObjectArraysTemp.Length);
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

	    	preActionType = actionType;
	    	preWalkDirection = walkDirection;
	    	preItemSerial = itemSerial;
	    	preMobileSerial = mobileSerial;
	    	preIndex = index;
	    	preAmount = amount;
	    	preRun = run;

	    	preActionType = actionType;
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

			_replayPath = Settings.ReplayPath;
			Console.WriteLine("_replayPath: {0}", _replayPath);

        	CreateMpqArchive(_replayPath + _replayName + ".uoreplay");
        }

        private void SaveReplayFile()
        {
        	//Console.WriteLine("SaveReplayFile()");

        	byte[] worldItemArrayLengthArray = ConvertIntListToByteArray(worldItemArrayLengthList);

            byte[] equippedItemArrayLengthArray = ConvertIntListToByteArray(equippedItemArrayLengthList);
            byte[] backpackItemArrayLengthArray = ConvertIntListToByteArray(backpackItemArrayLengthList);
            byte[] bankItemArrayLengthArray = ConvertIntListToByteArray(bankItemArrayLengthList);
            byte[] openedCorpseArrayLengthArray = ConvertIntListToByteArray(openedCorpseArrayLengthList);
            byte[] popupMenuArrayLengthArray = ConvertIntListToByteArray(popupMenuArrayLengthList);
            byte[] clilocDataArrayLengthArray = ConvertIntListToByteArray(clilocDataArrayLengthList);

	        byte[] playerMobileObjectArrayLengthArray = ConvertIntListToByteArray(playerMobileObjectArrayLengthList);
	    	byte[] mobileObjectArrayLengthArray = ConvertIntListToByteArray(mobileObjectArrayLengthList);
	    	byte[] itemObjectArrayLengthArray = ConvertIntListToByteArray(itemObjectArrayLengthList);
	    	byte[] vendorItemObjectArrayLengthArray = ConvertIntListToByteArray(vendorItemObjectArrayLengthList);

	    	//Console.WriteLine("staticObjectInfoListArraysLengthList.Count: {0}", staticObjectInfoListArraysLengthList.Count);
	    	byte[] staticObjectInfoListArraysLengthArray = ConvertIntListToByteArray(staticObjectInfoListArraysLengthList);

	    	//Console.WriteLine("playerStatusArrayLengthList.Count: {0}", playerStatusArrayLengthList.Count);
	    	for (int i = 0; i < playerStatusArrayLengthList.Count; i++)
	        {
	        	//Console.WriteLine("playerStatusArrayLengthList[{0}]: {1}", i, playerStatusArrayLengthList[i]);	
	        }

	        byte[] playerStatusArrayLengthArray = ConvertIntListToByteArray(playerStatusArrayLengthList);
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
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.type", actionTypeArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.walkDirection", walkDirectionArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.itemSerial", itemSerialArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.mobileSerial", mobileSerialArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.index", indexArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.amount", amountArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.action.run", runArray);

	    	// ##################################################################################
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.worldItemLen", worldItemArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.equippedItemLen", equippedItemArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.backpackitemLen", backpackItemArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.bankitemLen", bankItemArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.openedCorpseLen", openedCorpseArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.popupMenuLen", popupMenuArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.clilocDataLen", clilocDataArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerMobileObjectLen", playerMobileObjectArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.mobileObjectLen", mobileObjectArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.itemObjectLen", itemObjectArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.vendorItemObjectLen", vendorItemObjectArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerStatusLen", playerStatusArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerSkillListLen", playerSkillListArrayLengthArray);

            //Console.WriteLine("staticObjectInfoListArraysLengthArray.Length: {0}", staticObjectInfoListArraysLengthArray.Length);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.staticObjectInfoListArraysLen", staticObjectInfoListArraysLengthArray);

            // ##################################################################################   
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.worldItems", worldItemArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.worldMobiles", worldItemArrays);

			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.equippedItemSerials", equippedItemArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.backpackItemSerials", backpackItemArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.bankItemSerials", bankItemArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.vendorItemSerials", vendorItemObjectArrays);

			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.openedCorpse", openedCorpseArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.popupMenu", popupMenuArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.clilocData", clilocDataArrays);

	        WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerMobileObject", playerMobileObjectArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.mobileObject", mobileObjectArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.itemObject", itemObjectArrays);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerStatus", playerStatusArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerSkillList", playerSkillListArrays);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.staticObjectInfoList", staticObjectInfoListArrays);

            Console.WriteLine("equippedItemArrays.Length: {0}", equippedItemArrays.Length);
            Console.WriteLine("backpackItemArrays.Length: {0}", backpackItemArrays.Length);
            Console.WriteLine("bankItemArrays.Length: {0}", bankItemArrays.Length);
            Console.WriteLine("openedCorpseArrays.Length: {0}", openedCorpseArrays.Length);
            Console.WriteLine("popupMenuArrays.Length: {0}", popupMenuArrays.Length);
            Console.WriteLine("clilocDataArrays.Length: {0}", clilocDataArrays.Length);
            Console.WriteLine("playerMobileObjectArrays.Length: {0}", playerMobileObjectArrays.Length);
            Console.WriteLine("mobileObjectArrays.Length: {0}", mobileObjectArrays.Length);
            Console.WriteLine("itemObjectArrays.Length: {0}", itemObjectArrays.Length);
            Console.WriteLine("vendorItemObjectArrays.Length: {0}", vendorItemObjectArrays.Length);
            Console.WriteLine("playerStatusArrays.Length: {0}", playerStatusArrays.Length);
            Console.WriteLine("playerSkillListArrays.Length: {0}", playerSkillListArrays.Length);
            Console.WriteLine("staticObjectInfoListArrays.Length: {0}", staticObjectInfoListArrays.Length);
        }

        public void AddClilocData(uint serial, string text, string affix, string name)
        {
        	if (grpcClilocDataList.Count >= 50) 
        	{
        		//Console.WriteLine("grpcClilocDataList.Count: {0}", grpcClilocDataList.Count);
        		grpcClilocDataList.Clear();
        	}

        	grpcClilocDataList.Add(new GrpcClilocData{ Serial=serial, Text=text, Affix=affix, Name=name });
        }

        public void AddGameSimpleObject(string type, uint screen_x, uint screen_y, uint distance, uint game_x, uint game_y)
        {
        	try 
        	{
        		bool can_drop = (distance >= 1) && (distance < Constants.DRAG_ITEMS_DISTANCE);
        		if (can_drop)
            	{
            		//Console.WriteLine("can_drop game_x: {0}, game_y: {1}", game_x, game_y);
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
	        		//Console.WriteLine("PlayerMobile / screen_x: {0}, screen_y: {1}\n", screen_x, screen_y);
	        		//grpcPlayerMobileObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        		//												 	   GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        		//											           Title=title, Amount=amount, Price=price });
	        		scenePlayerMobileSerialList.Add(serial);
	        	}
	        	else if (type == "Item") {
	        		//grpcItemObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        		//											   GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        		//											   Title=title, Amount=amount, Price=price });
	        		sceneItemSerialList.Add(serial);
	        	}
	        	else if (type == "Mobile") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, distance: {3}", type, screen_x, screen_y, distance);
	        		//grpcMobileObjectList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        		//												 GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        		//											     Title=title, Amount=amount, Price=price });
	        		sceneMobileSerialList.Add(serial);
	        	}
	        	else if (type == "ShopItem") {
	        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, screen_x, screen_y, distance, name);
	        		//vendorItemSerialList.Add(new GrpcGameObjectData{ Type=type, ScreenX=screen_x, ScreenY=screen_y, Distance=distance, 
	        		//											         GameX=game_x, GameY=game_y, Serial=serial, Name=name, IsCorpse=is_corpse,
	        		//											         Title=title, Amount=amount, Price=price });
	        		vendorItemSerialList.Add(serial);
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

        public void UpdateWorldItems()
        {
        	if ((World.Player != null) && (World.InGame == true)) 
	        {
		        foreach (Item item in World.Items.Values)
	            {
	            	World.OPL.TryGetNameAndData(item.Serial, out string name, out string data);

	            	if (name != null)
	            	{
	            		//Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", name, item.Layer, item.Amount, item.Serial);
	            		//Item item = (Item) item;
	                    itemSerial = (uint) item;

	                    Vector2 itemPos = item.GetScreenPosition();

	                    //Console.WriteLine("Screen X: {0}, Screen Y: {1}, Distance: {2}, Game X: {3}, Game Y: {4}, Name: {5}, IsCorpse: {6}, Amount: {7}", 
	                    //                  (uint) itemPos.X, (uint) itemPos.Y, (uint) item.Distance, (uint) item.X, (uint) item.Y,
	                    //                  item.Name, item.IsCorpse, item.Amount);
	                    worldItemObjectList.Add(new GrpcGameObjectData{ Type="Item", ScreenX=(uint) itemPos.X, ScreenY=(uint) itemPos.Y, 
	                    												Distance=(uint) item.Distance, GameX=(uint) item.X, GameY=(uint) item.Y, Serial=item.Serial, 
	                    												Name=item.Name, IsCorpse=item.IsCorpse, Title="None", Amount=item.Amount, Price=item.Price });
	            	}
	            }
	        }
        }

        public void UpdateWorldMobiles()
        {
        	if ((World.Player != null) && (World.InGame == true)) 
	        {
		        foreach (Mobile mobile in World.Mobiles.Values)
	            {
	            	World.OPL.TryGetNameAndData(mobile.Serial, out string name, out string data);
	            	string title = "None";

	            	if (name != null)
	            	{
	                    mobileSerial = (uint) mobile;

	                    Vector2 mobilePos = mobile.GetScreenPosition();
	                    try
		                {
		                    TextObject mobileTextContainerItems = (TextObject) mobile.TextContainer.Items;
		                    RenderedText renderedText = mobileTextContainerItems.RenderedText;

		                    title = renderedText.Text;
		                }
		                catch (Exception ex) 
		                {
		                    //Console.WriteLine("Failed to print the TextContainer Items of Mobile: " + ex.Message);
		                }

	                    worldMobileObjectList.Add(new GrpcGameObjectData{ Type="Mobile", ScreenX=(uint) mobilePos.X, ScreenY=(uint) mobilePos.Y, 
	                    												  Distance=(uint) mobile.Distance, GameX=mobile.X, GameY=mobile.X, Serial=mobileSerial, 
	                    												  Name=name, IsCorpse=false, Title=title, Amount=0, Price=0 });
	            	}
	            }
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
            States states = new States();

            Console.WriteLine("Reset()");

            return Task.FromResult(states);
        }

        public States ReadObs()
        {
        	States states = new States();
        	//if (preActionType == 0) 
        	//{
        	//	return states;
        	//}

        	//Console.WriteLine("_envStep: {0}", _envStep);

        	if (_envStep % 1000 == 0)
        	{
        		Console.WriteLine("_envStep: {0}", _envStep);
        	}

        	/*
        	foreach (Item item in World.Items.Values)
            {
            	//Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, item.Amount, item.Serial);
            	worldItemObjectList.Add(new GrpcItemData{ Name = !string.IsNullOrEmpty(item.Name) ? item.Name : "Null", Layer = (uint) item.Layer,
	              									    Serial = (uint) item.Serial, Amount = (uint) item.Amount });
            }
            */

            if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	foreach (Layer layer in _layerOrder) {
	        		Item item = World.Player.FindItemByLayer(layer);
	        		try 
	        		{
	            		equippedItemSerialList.Add((uint) item.Serial);
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
                	
            		try 
	        		{
		              	backpackItemSerialList.Add((uint) item.Serial);
	            	}
		            catch (Exception ex)
		            {
		            	//Console.WriteLine("Failed to load the backpack item: " + ex.Message);
		            }
                }

                //Console.WriteLine("");
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

	                    	try 
			        		{
				              	bankItemSerialList.Add((uint) item.Serial);
			            	}
				            catch (Exception ex)
				            {
				            	Console.WriteLine("Failed to add the bank item serial: " + ex.Message);
				            }
		                }
	                }
	            }
	        }

	        GrpcPlayerStatus playerStatus = new GrpcPlayerStatus();
	        if ((World.Player != null) && (World.InGame == true))
            {
            	//Console.WriteLine("(uint) ItemHold.Serial: {0}", (uint) ItemHold.Serial);
            	//Console.WriteLine("(bool) World.Player.InWarMode: {0}", (bool) World.Player.InWarMode);
		        playerStatus = new GrpcPlayerStatus { Str = (uint) World.Player.Strength, Dex = (uint) World.Player.Dexterity, 
		        								      Intell = (uint) World.Player.Intelligence, Hits = (uint) World.Player.Hits,
		        								      HitsMax = (uint) World.Player.HitsMax, Stamina = (uint) World.Player.Stamina,
		        								      StaminaMax = (uint) World.Player.StaminaMax, Mana = (uint) World.Player.Mana,
		        								      Gold = (uint) World.Player.Gold, PhysicalResistance = (uint) World.Player.PhysicalResistance,
		        								      Weight = (uint) World.Player.Weight, WeightMax = (uint) World.Player.WeightMax,
		        								      HoldItemSerial = (uint) ItemHold.Serial, WarMode = (bool) World.Player.InWarMode };
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
			        	List<uint> corpseItemSerialList = new List<uint>();										
			        	for (LinkedObject i = corpseObj.Items; i != null; i = i.Next)
			            {
			                Item child = (Item) i;
			                if (child.Name != null)
			                {
			              		corpseItemSerialList.Add((uint) child.Serial);
		            		}
			            }

			            GrpcSerialList grpcCorpseItemSerialList = new GrpcSerialList();
			            grpcCorpseItemSerialList.Serials.AddRange(corpseItemSerialList);
			        	GrpcContainerData openedCorpse = new GrpcContainerData{ ContainerSerial = corpseSerial, ContainerItemSerialList = grpcCorpseItemSerialList };

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
 
	        GrpcGameObjectList grpcWorldItemList = new GrpcGameObjectList();
            grpcWorldItemList.GameObjects.AddRange(worldItemObjectList);
            states.WorldItemList = grpcWorldItemList;

            GrpcGameObjectList grpcWorldMobileList = new GrpcGameObjectList();
            grpcWorldMobileList.GameObjects.AddRange(worldItemObjectList);
            states.WorldItemList = grpcWorldMobileList;

            GrpcSerialList grpcEquippedItemSerialList = new GrpcSerialList();
            grpcEquippedItemSerialList.Serials.AddRange(equippedItemSerialList);
            states.EquippedItemSerialList = grpcEquippedItemSerialList;

            GrpcSerialList grpcBackpackItemSerialList = new GrpcSerialList();
            grpcBackpackItemSerialList.Serials.AddRange(backpackItemSerialList);
            states.BackpackItemSerialList = grpcBackpackItemSerialList;

            GrpcSerialList grpcBankItemSerialList = new GrpcSerialList();
            grpcBankItemSerialList.Serials.AddRange(bankItemSerialList);
            states.BankItemSerialList = grpcBankItemSerialList;

            GrpcSerialList grpcVendorItemSerialList = new GrpcSerialList();
            grpcVendorItemSerialList.Serials.AddRange(vendorItemSerialList);
            states.VendorItemSerialList = grpcVendorItemSerialList;

            GrpcContainerDataList grpcOpenedCorpseList = new GrpcContainerDataList();
            grpcOpenedCorpseList.Containers.AddRange(openedCorpseDataList);
            states.OpenedCorpseList = grpcOpenedCorpseList;

            GrpcPopupMenuList popupMenuList = new GrpcPopupMenuList();
            popupMenuList.Menus.AddRange(grpcPopupMenuList);
            states.PopupMenuList = popupMenuList;

            GrpcClilocDataList clilocDataList = new GrpcClilocDataList();
            clilocDataList.ClilocDatas.AddRange(grpcClilocDataList);
            states.ClilocDataList = clilocDataList;

            GrpcSkillList playerSkillList = new GrpcSkillList();
            playerSkillList.Skills.AddRange(grpcPlayerSkillListList);
            states.PlayerSkillList = playerSkillList;

            states.PlayerStatus = playerStatus;

            GrpcGameObjectInfoList gameObjectInfoList = new GrpcGameObjectInfoList();
            gameObjectInfoList.ScreenXs.AddRange(grpcStaticObjectScreenXs);
            gameObjectInfoList.ScreenYs.AddRange(grpcStaticObjectScreenYs);
            states.StaticObjectInfoList = gameObjectInfoList;

            GrpcSerialList grpcPlayerMobileSerialList = new GrpcSerialList();
            GrpcSerialList grpcMobileSerialList = new GrpcSerialList();
            GrpcSerialList grpcItemSerialList = new GrpcSerialList();

            grpcPlayerMobileSerialList.Serials.AddRange(scenePlayerMobileSerialList);
            grpcMobileSerialList.Serials.AddRange(sceneMobileSerialList);
            grpcItemSerialList.Serials.AddRange(sceneItemSerialList);

            states.PlayerMobileObjectList = grpcPlayerMobileSerialList;
            states.MobileObjectList = grpcMobileSerialList;
            states.ItemObjectList = grpcItemSerialList;

            // ##################################################################################
            byte[] worldItemArray = grpcWorldItemList.ToByteArray();
            byte[] worldMobileArray = grpcWorldMobileList.ToByteArray();

            byte[] equippedItemArray = grpcEquippedItemSerialList.ToByteArray();
            byte[] backpackItemArray = grpcBackpackItemSerialList.ToByteArray();
            byte[] bankItemArray = grpcBankItemSerialList.ToByteArray();
            byte[] openedCorpseArray = grpcOpenedCorpseList.ToByteArray();
            byte[] popupMenuArray = popupMenuList.ToByteArray();
            byte[] clilocDataArray = clilocDataList.ToByteArray();

        	byte[] playerMobileObjectArray = grpcPlayerMobileSerialList.ToByteArray();
        	byte[] mobileObjectArray = grpcMobileSerialList.ToByteArray();
        	byte[] itemObjectArray = grpcItemSerialList.ToByteArray();
        	byte[] vendorItemObjectArray = grpcVendorItemSerialList.ToByteArray();
        	
        	byte[] playerStatusArray = playerStatus.ToByteArray();
        	byte[] playerSkillListArray = playerSkillList.ToByteArray();

        	byte[] gameObjectInfoListArray = gameObjectInfoList.ToByteArray();

        	if (_envStep == 0) 
        	{
        		worldItemArraysTemp = worldItemArray;

        		equippedItemArraysTemp = equippedItemArray;
        		backpackItemArraysTemp = backpackItemArray;
        		bankItemArraysTemp = bankItemArray;
        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerMobileObjectArraysTemp = playerMobileObjectArray;
        		mobileObjectArraysTemp = mobileObjectArray;
        		itemObjectArraysTemp = itemObjectArray;
        		vendorItemObjectArraysTemp = vendorItemObjectArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
        		worldItemArrays = worldItemArraysTemp;

            	equippedItemArrays = equippedItemArraysTemp;
            	backpackItemArrays = backpackItemArraysTemp;
            	bankItemArrays = bankItemArraysTemp;
            	openedCorpseArrays = openedCorpseArraysTemp;
            	popupMenuArrays = popupMenuArraysTemp;
            	clilocDataArrays = clilocDataArraysTemp;

            	playerMobileObjectArrays = playerMobileObjectArraysTemp;
            	mobileObjectArrays = mobileObjectArraysTemp;
            	itemObjectArrays = itemObjectArraysTemp;
            	vendorItemObjectArrays = vendorItemObjectArraysTemp;

            	playerStatusArrays = playerStatusArraysTemp;
            	playerSkillListArrays = playerSkillListArraysTemp;

				staticObjectInfoListArrays = staticObjectInfoListArraysTemp;

				// ##################################################################################
				worldItemArraysTemp = worldItemArray;

        		equippedItemArraysTemp = equippedItemArray;
        		backpackItemArraysTemp = backpackItemArray;
        		bankItemArraysTemp = bankItemArray;
        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerMobileObjectArraysTemp = playerMobileObjectArray;
        		mobileObjectArraysTemp = mobileObjectArray;
        		itemObjectArraysTemp = itemObjectArray;
        		vendorItemObjectArraysTemp = vendorItemObjectArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
        		worldItemArrays = ConcatByteArrays(worldItemArrays, worldItemArraysTemp);

            	equippedItemArrays = ConcatByteArrays(equippedItemArrays, equippedItemArraysTemp);
            	backpackItemArrays = ConcatByteArrays(backpackItemArrays, backpackItemArraysTemp);
            	bankItemArrays = ConcatByteArrays(bankItemArrays, bankItemArraysTemp);
            	openedCorpseArrays = ConcatByteArrays(openedCorpseArrays, openedCorpseArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);

            	playerMobileObjectArrays = ConcatByteArrays(playerMobileObjectArrays, playerMobileObjectArraysTemp);
            	mobileObjectArrays = ConcatByteArrays(mobileObjectArrays, mobileObjectArraysTemp);
            	itemObjectArrays = ConcatByteArrays(itemObjectArrays, itemObjectArraysTemp);
            	vendorItemObjectArrays = ConcatByteArrays(vendorItemObjectArrays, vendorItemObjectArraysTemp);

            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);

            	staticObjectInfoListArrays = ConcatByteArrays(staticObjectInfoListArrays, staticObjectInfoListArraysTemp);

				// ##################################################################################
            	worldItemArraysTemp = worldItemArray;

        		equippedItemArraysTemp = equippedItemArray;
        		backpackItemArraysTemp = backpackItemArray;
        		bankItemArraysTemp = bankItemArray;
        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerMobileObjectArraysTemp = playerMobileObjectArray;
        		mobileObjectArraysTemp = mobileObjectArray;
        		itemObjectArraysTemp = itemObjectArray;
        		vendorItemObjectArraysTemp = vendorItemObjectArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);

            	equippedItemArrays = ConcatByteArrays(equippedItemArrays, equippedItemArraysTemp);
            	backpackItemArrays = ConcatByteArrays(backpackItemArrays, backpackItemArraysTemp);
            	bankItemArrays = ConcatByteArrays(bankItemArrays, bankItemArraysTemp);
            	openedCorpseArrays = ConcatByteArrays(openedCorpseArrays, openedCorpseArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);

            	playerMobileObjectArrays = ConcatByteArrays(playerMobileObjectArrays, playerMobileObjectArraysTemp);
            	mobileObjectArrays = ConcatByteArrays(mobileObjectArrays, mobileObjectArraysTemp);
            	itemObjectArrays = ConcatByteArrays(itemObjectArrays, itemObjectArraysTemp);
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
        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);

            	equippedItemArraysTemp = ConcatByteArrays(equippedItemArraysTemp, equippedItemArray);
            	backpackItemArraysTemp = ConcatByteArrays(backpackItemArraysTemp, backpackItemArray);
            	bankItemArraysTemp = ConcatByteArrays(bankItemArraysTemp, bankItemArray);
            	openedCorpseArraysTemp = ConcatByteArrays(openedCorpseArraysTemp, openedCorpseArray);
            	popupMenuArraysTemp = ConcatByteArrays(popupMenuArraysTemp, popupMenuArray);
            	clilocDataArraysTemp = ConcatByteArrays(clilocDataArraysTemp, clilocDataArray);

            	playerMobileObjectArraysTemp = ConcatByteArrays(playerMobileObjectArraysTemp, playerMobileObjectArray);
            	mobileObjectArraysTemp = ConcatByteArrays(mobileObjectArraysTemp, mobileObjectArray);
            	itemObjectArraysTemp = ConcatByteArrays(itemObjectArraysTemp, itemObjectArray);
            	vendorItemObjectArraysTemp = ConcatByteArrays(vendorItemObjectArraysTemp, vendorItemObjectArray);

            	playerStatusArraysTemp = ConcatByteArrays(playerStatusArraysTemp, playerStatusArray);
            	playerSkillListArraysTemp = ConcatByteArrays(playerSkillListArraysTemp, playerSkillListArray);

            	staticObjectInfoListArraysTemp = ConcatByteArrays(staticObjectInfoListArraysTemp, gameObjectInfoListArray);
        	}

        	// ##################################################################################
        	playerSkillListArrayLengthList.Add((int) playerSkillListArray.Length);

        	worldItemArrayLengthList.Add((int) worldItemArray.Length);

			equippedItemArrayLengthList.Add((int) equippedItemArray.Length);
			backpackItemArrayLengthList.Add((int) backpackItemArray.Length);
			bankItemArrayLengthList.Add((int) bankItemArray.Length);
			openedCorpseArrayLengthList.Add((int) openedCorpseArray.Length);
			popupMenuArrayLengthList.Add((int) popupMenuArray.Length);
			clilocDataArrayLengthList.Add((int) clilocDataArray.Length);

        	playerMobileObjectArrayLengthList.Add((int) playerMobileObjectArray.Length);
        	mobileObjectArrayLengthList.Add((int) mobileObjectArray.Length);
        	itemObjectArrayLengthList.Add((int) itemObjectArray.Length);
        	vendorItemObjectArrayLengthList.Add((int) vendorItemObjectArray.Length);

        	staticObjectInfoListArraysLengthList.Add((int) gameObjectInfoListArray.Length);

        	playerStatusArrayLengthList.Add((int) playerStatusArray.Length);
			
        	// ##################################################################################
        	_envStep++;

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
		    	Console.WriteLine("Tick:{0}, Type:{1}, Selected:{2}, Target:{3}, Index:{4}, Amount:{5}, Direction:{6}, Run:{7}", 
		    		_controller._gameTick, actionType, itemSerial, mobileSerial, index, amount, walkDirection, run);
		    }

		    /*
		    if (actionType != 0) 
		    {
			    actionTypeList.Add((int) actionType);
				walkDirectionList.Add((int) walkDirection);
				itemSerialList.Add((int) itemSerial);
				mobileSerialList.Add((int) mobileSerial);
				indexList.Add((int) index);
				amountList.Add((int) amount);
				runList.Add((bool) run);
			}
			*/

		    //actionTypeList.Add((int) actionType);
			//walkDirectionList.Add((int) walkDirection);
			//itemSerialList.Add((int) itemSerial);
			//mobileSerialList.Add((int) mobileSerial);
			//indexList.Add((int) index);
			//amountList.Add((int) amount);
			//runList.Add((bool) run);

			preActionType = actionType;
	    	preWalkDirection = walkDirection;
	    	preItemSerial = itemSerial;
	    	preMobileSerial = mobileSerial;
	    	preIndex = index;
	    	preAmount = amount;
	    	preRun = run;

			actionType = 0;
	    	walkDirection = 0;
	    	itemSerial = 0;
	    	mobileSerial = 0;
	    	index = 0;
	    	amount = 0;
	    	run = false;

	    	worldItemObjectList.Clear();

	        equippedItemSerialList.Clear();
	        backpackItemSerialList.Clear();
	        bankItemSerialList.Clear();
	        openedCorpseDataList.Clear();
            scenePlayerMobileSerialList.Clear();
            sceneMobileSerialList.Clear();
            sceneItemSerialList.Clear();
	        grpcStaticObjectScreenXs.Clear();
	        grpcStaticObjectScreenYs.Clear();
	        grpcPlayerSkillListList.Clear();
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(Actions actions, ServerCallContext context)
        {
    		if (actions.ActionType == 0) {
    			// Do nothing
            	if (World.InGame == true) {
            	}
	        }
            else if (actions.ActionType == 1) {
            	// Walk to Direction
            	/*
            	North = 0x00,
		        Right = 0x01,
		        East = 0x02,
		        Down = 0x03,
		        South = 0x04,
		        Left = 0x05,
		        West = 0x06,
		        Up = 0x07
		        */
            	if (World.InGame == true) {
            		//Console.WriteLine("actions.WalkDirection: {0}", actions.WalkDirection);

            		if (actions.WalkDirection == 0x00) {
	            		World.Player.Walk(Direction.North, actions.Run);
	            	}
	            	else if (actions.WalkDirection == 0x01) {
	            		World.Player.Walk(Direction.Right, actions.Run);
	            	}	
	            	else if (actions.WalkDirection == 0x02) {
	            		World.Player.Walk(Direction.East, actions.Run);
	            	}
	            	else if (actions.WalkDirection == 0x03) {
	            		World.Player.Walk(Direction.Down, actions.Run);
	            	}
	            	else if (actions.WalkDirection == 0x04) {
	            		World.Player.Walk(Direction.South, actions.Run);
	            	}
	            	else if (actions.WalkDirection == 0x05) {
	            		World.Player.Walk(Direction.Left, actions.Run);
	            	}	
	            	else if (actions.WalkDirection == 0x06) {
	            		World.Player.Walk(Direction.West, actions.Run);
	            	}
	            	else if (actions.WalkDirection == 0x07) {
	            		World.Player.Walk(Direction.Up, actions.Run);
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
        			GameActions.DropItem((uint) ItemHold.Serial, 0xFFFF, 0xFFFF, 0, backpack.Serial);
	        	}
	        }
	        else if (actions.ActionType == 5) {
	        	// Drop the holded item on land around the player
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 5");

	        		int randomNumber;
					Random RNG = new Random();

					Console.WriteLine("grpcItemDropableLandSimpleList.Count: {0}", grpcItemDropableLandSimpleList.Count);
	        		int index = RNG.Next(grpcItemDropableLandSimpleList.Count);
	        		try
	        		{   
	        			Console.WriteLine("index: {0}", index);
	        			GrpcGameObjectSimpleData selected = grpcItemDropableLandSimpleList[index];
	        			Console.WriteLine("selected: {0}", selected);
	        			GameActions.DropItem((uint) ItemHold.Serial, (int) selected.GameX, (int) selected.GameY, 0, 0xFFFF_FFFF);
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
	        	// Open the corpse by it's serial
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
	        	// Change the lock status of skill
	        	if (World.Player != null) {
	        		Console.WriteLine("actions.ActionType == 8");

	        		Skill skill = World.Player.Skills[actions.Index];

                    byte newStatus = (byte) skill.Lock;
                    if (newStatus < 2)
                    {
                        newStatus++;
                    }
                    else
                    {
                        newStatus = 0;
                    }

                    Console.WriteLine("actions.Index: {0}, newStatus: {1}", actions.Index, newStatus);

                    NetClient.Socket.Send_SkillStatusChangeRequest((ushort) actions.Index, newStatus);
                    skill.Lock = (Lock) newStatus;
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
	        		UIManager.ShowGamePopup(null);
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
	        		vendorItemSerialList.Clear();
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
	        		vendorItemSerialList.Clear();
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
	        else if (actions.ActionType == 19) {
	        	if (World.Player != null) {
	        		// Change the war mode
	        		Console.WriteLine("actions.ActionType == 19");
	        		bool boolIndex = Convert.ToBoolean(actions.Index);

	        		//Console.WriteLine("boolIndex: {0}", boolIndex);
        			NetClient.Socket.Send_ChangeWarMode(boolIndex);
        			World.Player.InWarMode = boolIndex;
	        	}
	        }

	        actionType = 0;
    		walkDirection = 0;
    		itemSerial = 0;
    		mobileSerial = 0;
    		amount = 0;
    		index = 0;
    		run = false;

    		worldItemObjectList.Clear();
    		grpcPlayerSkillListList.Clear();
	        equippedItemSerialList.Clear();
	        backpackItemSerialList.Clear();
	        bankItemSerialList.Clear();
	        openedCorpseDataList.Clear();
            scenePlayerMobileSerialList.Clear();
            sceneMobileSerialList.Clear();
            sceneItemSerialList.Clear();
	        grpcStaticObjectScreenXs.Clear();
	        grpcStaticObjectScreenYs.Clear();

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