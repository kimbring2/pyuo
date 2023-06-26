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

        GrpcPlayerObject grpcPlayerObject = new GrpcPlayerObject();

        List<GrpcItemObjectData> worldItemObjectList = new List<GrpcItemObjectData>();
        List<GrpcMobileObjectData> worldMobileObjectList = new List<GrpcMobileObjectData>();

        List<uint> equippedItemSerialList = new List<uint>();
        List<uint> backpackItemSerialList = new List<uint>();
        List<uint> bankItemSerialList = new List<uint>();
        List<uint> vendorItemSerialList = new List<uint>();

        List<GrpcContainerData> openedCorpseDataList = new List<GrpcContainerData>();
        List<string> grpcPopupMenuList = new List<string>();
        List<GrpcClilocData> grpcClilocDataList = new List<GrpcClilocData>();

        List<Vector2> itemDropableLandSimpleList = new List<Vector2>();

        List<uint> grpcStaticObjectGameXs = new List<uint>();
        List<uint> grpcStaticObjectGameYs = new List<uint>();

        GrpcPlayerStatus grpcPlayerStatus = new GrpcPlayerStatus();
        List<GrpcSkill> grpcPlayerSkillListList = new List<GrpcSkill>();

        int _totalStepScale = 2;
        int _envStep;
        string _replayName;
        string _replayPath;

        // ##################################################################################
        List<int> playerObjectArrayLengthList = new List<int>();

        List<int> worldItemArrayLengthList = new List<int>();
        List<int> worldMobileArrayLengthList = new List<int>();

    	List<int> equippedItemSerialArrayLengthList = new List<int>();
    	List<int> backpackItemSerialArrayLengthList = new List<int>();
    	List<int> bankItemSerialArrayLengthList = new List<int>();
    	List<int> vendorItemSerialArrayLengthList = new List<int>();

    	List<int> openedCorpseArrayLengthList = new List<int>();
    	List<int> popupMenuArrayLengthList = new List<int>();
    	List<int> clilocDataArrayLengthList = new List<int>();

    	List<int> playerStatusArrayLengthList = new List<int>();
        List<int> playerSkillListArrayLengthList = new List<int>();

        List<int> staticObjectInfoListArraysLengthList = new List<int>();

        int playerStatusArrayLength;
        int playerSkillListArrayLength;

        // ##################################################################################
        byte[] playerObjectArrays;

        byte[] worldItemArrays;
        byte[] worldMobileArrays;

		byte[] equippedItemSerialArrays;
		byte[] backpackItemSerialArrays;
		byte[] bankItemSerialArrays;
		byte[] vendorItemSerialArrays;

		byte[] openedCorpseArrays;
		byte[] popupMenuArrays;
		byte[] clilocDataArrays;

        byte[] playerStatusArrays;
        byte[] playerSkillListArrays;

        byte[] staticObjectInfoListArrays;

        // ##################################################################################
        byte[] playerObjectArraysTemp;

        byte[] worldItemArraysTemp;
        byte[] worldMobileArraysTemp;

		byte[] equippedItemSerialArraysTemp;
		byte[] backpackItemSerialArraysTemp;
		byte[] bankItemSerialArraysTemp;
		byte[] vendorItemSerialArraysTemp;

		byte[] openedCorpseArraysTemp;
		byte[] popupMenuArraysTemp;
		byte[] clilocDataArraysTemp;

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

	    public int GetEnvStep()
	    {
	        return _envStep;
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
        	playerObjectArrayLengthList.Clear();

        	worldItemArrayLengthList.Clear();
        	worldMobileArrayLengthList.Clear();

	    	equippedItemSerialArrayLengthList.Clear();
	    	backpackItemSerialArrayLengthList.Clear();
	    	bankItemSerialArrayLengthList.Clear();
	    	vendorItemSerialArrayLengthList.Clear();

	    	openedCorpseArrayLengthList.Clear();
	    	popupMenuArrayLengthList.Clear();
	    	clilocDataArrayLengthList.Clear();

	    	playerStatusArrayLengthList.Clear();
	        playerSkillListArrayLengthList.Clear();

	        staticObjectInfoListArraysLengthList.Clear();

	        // ##################################################################################
	        Array.Clear(playerObjectArrays, 0, playerObjectArrays.Length);

	        Array.Clear(worldItemArrays, 0, worldItemArrays.Length);
	        Array.Clear(worldMobileArrays, 0, worldMobileArrays.Length);

	        Array.Clear(equippedItemSerialArrays, 0, equippedItemSerialArrays.Length);
	        Array.Clear(backpackItemSerialArrays, 0, backpackItemSerialArrays.Length);
	        Array.Clear(bankItemSerialArrays, 0, bankItemSerialArrays.Length);
	        Array.Clear(vendorItemSerialArrays, 0, vendorItemSerialArrays.Length);

	        Array.Clear(openedCorpseArrays, 0, openedCorpseArrays.Length);
	        Array.Clear(popupMenuArrays, 0, popupMenuArrays.Length);
	        Array.Clear(clilocDataArrays, 0, clilocDataArrays.Length);

	        Array.Clear(playerStatusArrays, 0, playerStatusArrays.Length);
	        Array.Clear(playerSkillListArrays, 0, playerSkillListArrays.Length);

	        Array.Clear(staticObjectInfoListArrays, 0, staticObjectInfoListArrays.Length);

	        // ##################################################################################
	        Array.Clear(playerObjectArraysTemp, 0, playerObjectArraysTemp.Length);

	        Array.Clear(worldItemArraysTemp, 0, worldItemArraysTemp.Length);
	        Array.Clear(worldMobileArraysTemp, 0, worldMobileArraysTemp.Length);

	        Array.Clear(equippedItemSerialArraysTemp, 0, equippedItemSerialArraysTemp.Length);
	        Array.Clear(backpackItemSerialArraysTemp, 0, backpackItemSerialArraysTemp.Length);
	        Array.Clear(bankItemSerialArraysTemp, 0, bankItemSerialArraysTemp.Length);
	        Array.Clear(vendorItemSerialArraysTemp, 0, vendorItemSerialArraysTemp.Length);

	        Array.Clear(openedCorpseArraysTemp, 0, openedCorpseArraysTemp.Length);
	        Array.Clear(popupMenuArraysTemp, 0, popupMenuArraysTemp.Length);
	        Array.Clear(clilocDataArraysTemp, 0, clilocDataArraysTemp.Length);

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
        	Console.WriteLine("SaveReplayFile() 1");

        	byte[] playerObjectArrayLengthArray = ConvertIntListToByteArray(playerObjectArrayLengthList);

        	byte[] worldItemArrayLengthArray = ConvertIntListToByteArray(worldItemArrayLengthList);
        	byte[] worldMobileArrayLengthArray = ConvertIntListToByteArray(worldMobileArrayLengthList);

            byte[] equippedItemSerialArrayLengthArray = ConvertIntListToByteArray(equippedItemSerialArrayLengthList);
            byte[] backpackItemSerialArrayLengthArray = ConvertIntListToByteArray(backpackItemSerialArrayLengthList);
            byte[] bankItemSerialArrayLengthArray = ConvertIntListToByteArray(bankItemSerialArrayLengthList);
            byte[] vendorItemSerialArrayLengthArray = ConvertIntListToByteArray(vendorItemSerialArrayLengthList);

            byte[] openedCorpseArrayLengthArray = ConvertIntListToByteArray(openedCorpseArrayLengthList);
            byte[] popupMenuArrayLengthArray = ConvertIntListToByteArray(popupMenuArrayLengthList);
            byte[] clilocDataArrayLengthArray = ConvertIntListToByteArray(clilocDataArrayLengthList);

	    	byte[] playerStatusArrayLengthArray = ConvertIntListToByteArray(playerStatusArrayLengthList);
	        byte[] playerSkillListArrayLengthArray = ConvertIntListToByteArray(playerSkillListArrayLengthList);

	        byte[] staticObjectInfoListArraysLengthArray = ConvertIntListToByteArray(staticObjectInfoListArraysLengthList);

	        // ##################################################################################

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
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerObjectLen", playerObjectArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.worldItemLen", worldItemArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.worldMobileLen", worldMobileArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.equippedItemSerialLen", equippedItemSerialArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.backpackitemSerialLen", backpackItemSerialArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.bankitemSerialLen", bankItemSerialArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.vendorItemSerialLen", vendorItemSerialArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.openedCorpseLen", openedCorpseArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.popupMenuLen", popupMenuArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.clilocDataLen", clilocDataArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerStatusLen", playerObjectArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerSkillListLen", playerSkillListArrayLengthArray);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.staticObjectInfoListArraysLen", 
            																						staticObjectInfoListArraysLengthArray);

            // ##################################################################################
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerObject", playerObjectArrays);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.worldItems", worldItemArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.worldMobiles", worldMobileArrays);

			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.equippedItemSerials", equippedItemSerialArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.backpackItemSerials", backpackItemSerialArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.bankItemSerials", bankItemSerialArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.vendorItemSerials", vendorItemSerialArrays);

			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.openedCorpse", openedCorpseArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.popupMenu", popupMenuArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.clilocData", clilocDataArrays);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerStatus", playerStatusArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerSkillList", playerSkillListArrays);

            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.staticObjectInfoList", staticObjectInfoListArrays);

            Console.WriteLine("playerObjectArrays.Length: {0}", playerObjectArrays.Length);

            Console.WriteLine("worldItemArrays.Length: {0}", worldItemArrays.Length);
            Console.WriteLine("worldMobileArrays.Length: {0}", worldMobileArrays.Length);

            Console.WriteLine("equippedItemSerialArrays.Length: {0}", equippedItemSerialArrays.Length);
            Console.WriteLine("backpackItemSerialArrays.Length: {0}", backpackItemSerialArrays.Length);
            Console.WriteLine("bankItemSerialArrays.Length: {0}", bankItemSerialArrays.Length);
            Console.WriteLine("vendorItemSerialArrays.Length: {0}", vendorItemSerialArrays.Length);

            Console.WriteLine("openedCorpseArrays.Length: {0}", openedCorpseArrays.Length);
            Console.WriteLine("popupMenuArrays.Length: {0}", popupMenuArrays.Length);
            Console.WriteLine("clilocDataArrays.Length: {0}", clilocDataArrays.Length);

            Console.WriteLine("playerStatusArrays.Length: {0}", playerStatusArrays.Length);
            Console.WriteLine("playerSkillListArrays.Length: {0}", playerSkillListArrays.Length);

            Console.WriteLine("staticObjectInfoListArrays.Length: {0}", staticObjectInfoListArrays.Length);
        }

        public void UpdatePlayerObject()
        {
        	// Add player game object 
	        if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	try
	        	{
			        grpcPlayerObject = new GrpcPlayerObject{ GameX=(uint) World.Player.X, GameY=(uint) World.Player.Y, 
			                    							 Serial=World.Player.Serial, Name=World.Player.Name, Title="None",
			                    							 HoldItemSerial = (uint) ItemHold.Serial, 
			                    							 WarMode = (bool) World.Player.InWarMode };
			    }
			    catch (Exception ex)
	            {
	            	Console.WriteLine("Failed to add the player object: " + ex.Message);
	            }

		    }
		}

        public void AddClilocData(uint serial, string text, string affix, string name)
        {
        	if (grpcClilocDataList.Count >= 50) 
        	{
        		//Console.WriteLine("grpcClilocDataList.Count: {0}", grpcClilocDataList.Count);
        		//grpcClilocDataList.Clear();
        	}

        	grpcClilocDataList.Add(new GrpcClilocData{ Serial=serial, Text=text, Affix=affix, Name=name });
        }

        public void AddGameSimpleObject(string type, uint distance, uint game_x, uint game_y)
        {
        	try 
        	{
        		bool can_drop = (distance >= 1) && (distance < Constants.DRAG_ITEMS_DISTANCE);
        		if (can_drop)
            	{
            		//Console.WriteLine("can_drop game_x: {0}, game_y: {1}", game_x, game_y);
        			itemDropableLandSimpleList.Add(new Vector2{ X=game_x, Y=game_y });
        		}
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the item object: " + ex.Message);
            }
        }

        public void AddItemObject(uint distance, uint game_x, uint game_y, uint serial, string name, bool is_corpse, 
        						  uint amount, uint price, uint layer, uint container, bool onGround)
        {
        	try 
        	{
        		worldItemObjectList.Add(new GrpcItemObjectData{ Distance=distance, GameX=game_x, GameY=game_y, 
	                    										Serial=serial, Name=name, IsCorpse=is_corpse,
	                    										Amount=amount, Price=price, Layer=layer,
	                    										Container=container, OnGround=onGround });
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object: " + ex.Message);
            }
        }

        public void AddMobileObject(uint distance, uint game_x, uint game_y, uint serial, string name, bool is_corpse, 
        						    string title)
        {
        	try 
        	{
        		worldMobileObjectList.Add(new GrpcMobileObjectData{ Distance=distance, GameX=game_x, GameY=game_y, 
	                    										    Serial=serial, Name=name, IsCorpse=is_corpse, 
	                    										    Title=title });
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the mobile object: " + ex.Message);
            }
        }

        public void AddGameObjectSerial(string type, uint distance, uint game_x, uint game_y, uint serial, string name, 
        								bool is_corpse, string title, uint amount, uint price)
        {
        	try 
        	{
        		//Console.WriteLine("type: {0}, x: {1}, y: {2}, dis: {3}, name: {4}", type, game_x, game_y, distance, name);
	        	if (type == "ShopItem") {
	        		vendorItemSerialList.Add(serial);
	        	}
	        	else if (type == "Static") {
	        		if (distance <= 6) 
	        		{
	        			grpcStaticObjectGameXs.Add(game_x);
			        	grpcStaticObjectGameYs.Add(game_y);
	        		}
	        	}
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object serial: " + ex.Message);
            }
        }

        public void UpdateWorldItems()
        {
        	//Console.WriteLine("UpdateWorldItems()");

        	if ((World.Player != null) && (World.InGame == true)) 
	        {
		        foreach (Item item in World.Items.Values)
	            {
	            	World.OPL.TryGetNameAndData(item.Serial, out string name, out string data);
	            	if (name != null)
	            	{
	                    itemSerial = (uint) item;

	                    AddItemObject((uint) item.Distance, (uint) item.X, (uint) item.Y, item.Serial, item.Name, 
	                    			   item.IsCorpse, item.Amount, item.Price, (uint) item.Layer, (uint) item.Container, 
	                    			   item.OnGround);
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

	                    AddMobileObject((uint) mobile.Distance, (uint) mobile.X, (uint) mobile.Y, mobileSerial, name, false, title);
	            	}
	            }
	        }
        }

        public void UpdatePlayerSkills()
        {
        	// Add player skill 
	        if ((World.Player != null) && (World.InGame == true))
	        {
	        	//Console.WriteLine("UpdatePlayerSkills()");

	            //Console.WriteLine("\nWorld.Player.Skills.Length: {0}", World.Player.Skills.Length);
		        for (int i = 0; i < World.Player.Skills.Length; i++)
	            {
	            	Skill skill = World.Player.Skills[i];
	            	//Console.WriteLine("Name: {0}, Index: {1}, IsClickable: {2}, Value: {3}, Base: {4}, Cap: {5}, Lock: {6}", 
	            	//	skill.Name, skill.Index, skill.IsClickable, skill.Value, skill.Base, skill.Cap, skill.Lock);
	            	grpcPlayerSkillListList.Add(new GrpcSkill{ Name=skill.Name, Index=(uint) skill.Index, 
	            											   IsClickable=(bool) skill.IsClickable, 
	            											   Value=(uint) skill.Value, Base=(uint) skill.Base, 
	            											   Cap=(uint) skill.Cap, Lock=(uint) skill.Lock });
	            }
	        }
	    }

	    public void UpdatePlayerStatus()
        {
        	// Add player status 
	        if ((World.Player != null) && (World.InGame == true))
            {
            	//Mobile playerMobileEntity = World.Mobiles.Get(objSerial);
            	Vector2 playerPos = World.Player.GetScreenPosition();

            	//Console.WriteLine("playerPos.X: {0}, playerPos.Y: {1}", playerPos.X, playerPos.Y);
		        grpcPlayerStatus = new GrpcPlayerStatus { Str=(uint) World.Player.Strength, Dex=(uint) World.Player.Dexterity, 
		        								          Intell=(uint) World.Player.Intelligence, Hits=(uint) World.Player.Hits,
		        								          HitsMax=(uint) World.Player.HitsMax, Stamina=(uint) World.Player.Stamina,
		        								          StaminaMax=(uint) World.Player.StaminaMax, Mana=(uint) World.Player.Mana,
		        								          Gold=(uint) World.Player.Gold, 
		        								          PhysicalResistance=(uint) World.Player.PhysicalResistance,
		        								          Weight=(uint) World.Player.Weight, WeightMax=(uint) World.Player.WeightMax };
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

        public States ReadObs(bool config_init)
        {
        	States states = new States();
        	//if (preActionType == 0) 
        	//{
        	//	return states;
        	//}

        	if (config_init == true)
        	{
        		UpdateWorldItems();
        		UpdatePlayerStatus();
        	}

        	UpdateWorldItems();

        	//Console.WriteLine("_envStep: {0}", _envStep);
        	//Console.WriteLine("Layer.Backpack: {0}", (uint) Layer.Backpack);

        	if (_envStep % 1000 == 0)
        	{
        		Console.WriteLine("_envStep: {0}", _envStep);
        	}

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
			        	GrpcContainerData openedCorpse = new GrpcContainerData{ ContainerSerial=corpseSerial, 
			        															ContainerItemSerialList=grpcCorpseItemSerialList };

			        	openedCorpseDataList.Add(openedCorpse);
			        }
			    }
			}

	        /*
	        // Player status etc
	        grpcPlayerObject grpcPlayerObject = new grpcPlayerObject();
	        if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	//Console.WriteLine("(bool) World.Player.InWarMode: {0}", (bool) World.Player.InWarMode);
	        	//Console.WriteLine("(uint) ItemHold.Serial: {0}", (uint) ItemHold.Serial);
	        	grpcPlayerObject = new grpcPlayerObject { HoldItemSerial = (uint) ItemHold.Serial, 
	        											        WarMode = (bool) World.Player.InWarMode };

	        }
	        */

		    states.PlayerObject = grpcPlayerObject;

	        GrpcItemObjectList grpcWorldItemList = new GrpcItemObjectList();
            grpcWorldItemList.ItemObjects.AddRange(worldItemObjectList);
            states.WorldItemList = grpcWorldItemList;

            GrpcMobileObjectList grpcWorldMobileList = new GrpcMobileObjectList();
            grpcWorldMobileList.MobileObjects.AddRange(worldMobileObjectList);
            states.WorldMobileList = grpcWorldMobileList;

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

            states.PlayerStatus = grpcPlayerStatus;

            GrpcGameObjectInfoList gameObjectInfoList = new GrpcGameObjectInfoList();
            gameObjectInfoList.GameXs.AddRange(grpcStaticObjectGameXs);
            gameObjectInfoList.GameYs.AddRange(grpcStaticObjectGameYs);
            states.StaticObjectInfoList = gameObjectInfoList;

            // ##################################################################################
            byte[] playerObjectArray = grpcPlayerObject.ToByteArray();

            byte[] worldItemArray = grpcWorldItemList.ToByteArray();
            byte[] worldMobileArray = grpcWorldMobileList.ToByteArray();

            byte[] equippedItemSerialArray = grpcEquippedItemSerialList.ToByteArray();
            byte[] backpackItemSerialArray = grpcBackpackItemSerialList.ToByteArray();
            byte[] bankItemSerialArray = grpcBankItemSerialList.ToByteArray();
            byte[] vendorItemSerialArray = grpcVendorItemSerialList.ToByteArray();

            byte[] openedCorpseArray = grpcOpenedCorpseList.ToByteArray();
            byte[] popupMenuArray = popupMenuList.ToByteArray();
            byte[] clilocDataArray = clilocDataList.ToByteArray();

        	byte[] playerStatusArray = grpcPlayerStatus.ToByteArray();
        	byte[] playerSkillListArray = playerSkillList.ToByteArray();

        	byte[] gameObjectInfoListArray = gameObjectInfoList.ToByteArray();

        	//Console.WriteLine("sceneMobileObjectSerialArray.Length: {0}", sceneMobileObjectSerialArray.Length);
        	//Console.WriteLine("playerSkillListArray.Length: {0}", playerSkillListArray.Length);
        	//Console.WriteLine("playerObjectArray.Length: {0}", playerObjectArray.Length);
        	//Console.WriteLine("playerStatusArray.Length: {0}", playerStatusArray.Length);
        	//Console.WriteLine("playerObjectArray.Length: {0}", playerObjectArray.Length);
        	//Console.WriteLine("playerSkillListArray.Length: {0}", playerSkillListArray.Length);
        	//Console.WriteLine("");

        	if (_envStep == 0) 
        	{
        		playerObjectArraysTemp = playerObjectArray;

        		worldItemArraysTemp = worldItemArray;
        		worldMobileArraysTemp = worldMobileArray;

        		equippedItemSerialArraysTemp = equippedItemSerialArray;
        		backpackItemSerialArraysTemp = backpackItemSerialArray;
        		bankItemSerialArraysTemp = bankItemSerialArray;
        		vendorItemSerialArraysTemp = vendorItemSerialArray;

        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
        		playerObjectArrays = playerObjectArraysTemp;

        		worldItemArrays = worldItemArraysTemp;
        		worldMobileArrays = worldMobileArraysTemp;

            	equippedItemSerialArrays = equippedItemSerialArraysTemp;
            	backpackItemSerialArrays = backpackItemSerialArraysTemp;
            	bankItemSerialArrays = bankItemSerialArraysTemp;
            	vendorItemSerialArrays = vendorItemSerialArraysTemp;

            	openedCorpseArrays = openedCorpseArraysTemp;
            	popupMenuArrays = popupMenuArraysTemp;
            	clilocDataArrays = clilocDataArraysTemp;

            	playerStatusArrays = playerStatusArraysTemp;
            	playerSkillListArrays = playerSkillListArraysTemp;

				staticObjectInfoListArrays = staticObjectInfoListArraysTemp;

				// ##################################################################################
				worldItemArraysTemp = worldItemArray;
				worldMobileArraysTemp = worldMobileArray;

        		equippedItemSerialArraysTemp = equippedItemSerialArray;
        		backpackItemSerialArraysTemp = backpackItemSerialArray;
        		bankItemSerialArraysTemp = bankItemSerialArray;
        		vendorItemSerialArraysTemp = vendorItemSerialArray;

        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerObjectArraysTemp = playerObjectArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
        		playerObjectArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);

        		worldItemArrays = ConcatByteArrays(worldItemArrays, worldItemArraysTemp);
        		worldMobileArrays = ConcatByteArrays(worldMobileArrays, worldMobileArraysTemp);

            	equippedItemSerialArrays = ConcatByteArrays(equippedItemSerialArrays, equippedItemSerialArraysTemp);
            	backpackItemSerialArrays = ConcatByteArrays(backpackItemSerialArrays, backpackItemSerialArraysTemp);
            	bankItemSerialArrays = ConcatByteArrays(bankItemSerialArrays, bankItemSerialArraysTemp);
            	vendorItemSerialArrays = ConcatByteArrays(vendorItemSerialArrays, vendorItemSerialArraysTemp);

            	openedCorpseArrays = ConcatByteArrays(openedCorpseArrays, openedCorpseArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);

            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);

            	staticObjectInfoListArrays = ConcatByteArrays(staticObjectInfoListArrays, staticObjectInfoListArraysTemp);

				// ##################################################################################
            	playerObjectArraysTemp = playerObjectArray;

            	worldItemArraysTemp = worldItemArray;
            	worldMobileArraysTemp = worldMobileArray;

        		equippedItemSerialArraysTemp = equippedItemSerialArray;
        		backpackItemSerialArraysTemp = backpackItemSerialArray;
        		bankItemSerialArraysTemp = bankItemSerialArray;
        		vendorItemSerialArraysTemp = vendorItemSerialArray;

        		openedCorpseArraysTemp = openedCorpseArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;

        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;

        		staticObjectInfoListArraysTemp = gameObjectInfoListArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
        		playerObjectArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);

        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);
        		worldMobileArraysTemp = ConcatByteArrays(worldMobileArraysTemp, worldMobileArray);

            	equippedItemSerialArrays = ConcatByteArrays(equippedItemSerialArrays, equippedItemSerialArraysTemp);
            	backpackItemSerialArrays = ConcatByteArrays(backpackItemSerialArrays, backpackItemSerialArraysTemp);
            	bankItemSerialArrays = ConcatByteArrays(bankItemSerialArrays, bankItemSerialArraysTemp);
            	vendorItemSerialArrays = ConcatByteArrays(vendorItemSerialArrays, vendorItemSerialArraysTemp);

            	openedCorpseArrays = ConcatByteArrays(openedCorpseArrays, openedCorpseArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);

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
        		playerObjectArraysTemp = ConcatByteArrays(playerObjectArraysTemp, playerObjectArray);

        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);
        		worldMobileArraysTemp = ConcatByteArrays(worldMobileArraysTemp, worldMobileArray);

            	equippedItemSerialArraysTemp = ConcatByteArrays(equippedItemSerialArraysTemp, equippedItemSerialArray);
            	backpackItemSerialArraysTemp = ConcatByteArrays(backpackItemSerialArraysTemp, backpackItemSerialArray);
            	bankItemSerialArraysTemp = ConcatByteArrays(bankItemSerialArraysTemp, bankItemSerialArray);
            	vendorItemSerialArraysTemp = ConcatByteArrays(vendorItemSerialArraysTemp, vendorItemSerialArray);

            	openedCorpseArraysTemp = ConcatByteArrays(openedCorpseArraysTemp, openedCorpseArray);
            	popupMenuArraysTemp = ConcatByteArrays(popupMenuArraysTemp, popupMenuArray);
            	clilocDataArraysTemp = ConcatByteArrays(clilocDataArraysTemp, clilocDataArray);

            	playerStatusArraysTemp = ConcatByteArrays(playerStatusArraysTemp, playerStatusArray);
            	playerSkillListArraysTemp = ConcatByteArrays(playerSkillListArraysTemp, playerSkillListArray);

            	staticObjectInfoListArraysTemp = ConcatByteArrays(staticObjectInfoListArraysTemp, gameObjectInfoListArray);
        	}

        	// ##################################################################################
        	playerObjectArrayLengthList.Add((int) playerObjectArray.Length);

        	worldItemArrayLengthList.Add((int) worldItemArray.Length);
        	worldMobileArrayLengthList.Add((int) worldMobileArray.Length);

			equippedItemSerialArrayLengthList.Add((int) equippedItemSerialArray.Length);
			backpackItemSerialArrayLengthList.Add((int) backpackItemSerialArray.Length);
			bankItemSerialArrayLengthList.Add((int) bankItemSerialArray.Length);
			vendorItemSerialArrayLengthList.Add((int) vendorItemSerialArray.Length);

			openedCorpseArrayLengthList.Add((int) openedCorpseArray.Length);
			popupMenuArrayLengthList.Add((int) popupMenuArray.Length);
			clilocDataArrayLengthList.Add((int) clilocDataArray.Length);

        	playerStatusArrayLengthList.Add((int) playerStatusArray.Length);
        	playerSkillListArrayLengthList.Add((int) playerSkillListArray.Length);

        	staticObjectInfoListArraysLengthList.Add((int) gameObjectInfoListArray.Length);
			
        	// ##################################################################################
        	_envStep++;

	        return states;
        }

        public override Task<States> ReadObs(Config config, ServerCallContext context)
        {
        	//Console.WriteLine("config.Init: {0}", config.Init);
        	States obs = ReadObs(config.Init);

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

		    actionTypeList.Add((int) actionType);
			walkDirectionList.Add((int) walkDirection);
			itemSerialList.Add((int) itemSerial);
			mobileSerialList.Add((int) mobileSerial);
			indexList.Add((int) index);
			amountList.Add((int) amount);
			runList.Add((bool) run);

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

	    	grpcPlayerObject = new GrpcPlayerObject();

	    	worldItemObjectList.Clear();
	    	worldMobileObjectList.Clear();

	        equippedItemSerialList.Clear();
	        backpackItemSerialList.Clear();
	        bankItemSerialList.Clear();
	        vendorItemSerialList.Clear();

	        openedCorpseDataList.Clear();
	        grpcClilocDataList.Clear();

	        grpcPlayerSkillListList.Clear();
	        grpcPlayerStatus = new GrpcPlayerStatus();

        	grpcStaticObjectGameXs.Clear();
	        grpcStaticObjectGameYs.Clear();
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

					Console.WriteLine("itemDropableLandSimpleList.Count: {0}", itemDropableLandSimpleList.Count);
	        		int index = RNG.Next(itemDropableLandSimpleList.Count);
	        		try
	        		{   
	        			Console.WriteLine("index: {0}", index);
	        			Vector2 selected = itemDropableLandSimpleList[index];
	        			Console.WriteLine("selected: {0}", selected);
	        			GameActions.DropItem((uint) ItemHold.Serial, (int) selected.X, (int) selected.Y, 0, 0xFFFF_FFFF);
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
	        	// 
	        	if (World.Player != null) {
                    Console.WriteLine("actions.ActionType == 9");
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
	        		// Buy the item from vendor by selecting the item from shop gump
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
	        		// Sell the item to vendor by selecting the item from shop gump
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

    		grpcPlayerObject = new GrpcPlayerObject();

    		worldItemObjectList.Clear();
	    	worldMobileObjectList.Clear();

	        equippedItemSerialList.Clear();
	        backpackItemSerialList.Clear();
	        bankItemSerialList.Clear();
	        vendorItemSerialList.Clear();

	        openedCorpseDataList.Clear();
	        grpcClilocDataList.Clear();

	        grpcPlayerSkillListList.Clear();
	        grpcPlayerStatus = new GrpcPlayerStatus();

        	grpcStaticObjectGameXs.Clear();
	        grpcStaticObjectGameYs.Clear();

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