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
using ClassicUO.Game.Map;
using ClassicUO.Input;
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
        List<GrpcPopupMenu> grpcPopupMenuList = new List<GrpcPopupMenu>();
        List<GrpcClilocData> grpcClilocDataList = new List<GrpcClilocData>();
        List<Vector2> itemDropableLandSimpleList = new List<Vector2>();
        GrpcPlayerStatus grpcPlayerStatus = new GrpcPlayerStatus();
        List<GrpcSkill> grpcPlayerSkillListList = new List<GrpcSkill>();

        List<GrpcLandObjectData> landObjectList = new List<GrpcLandObjectData>();
        List<uint> grpcStaticObjectGameXs = new List<uint>();
        List<uint> grpcStaticObjectGameYs = new List<uint>();
        List<uint> grpcLandRockObjectGameXs = new List<uint>();
        List<uint> grpcLandRockObjectGameYs = new List<uint>();

        GrpcAction grpcAction = new GrpcAction();

        int _totalStepScale;
        int _envStep;
        string _replayName;
        string _replayPath;
        int _updateWorldItemsTimer;
        int _updatePlayerObjectTimer;

        bool _mapLoad = true;
        Map map;

        //Land _usedLand;
        uint _usedLandGameX;
        uint _usedLandGameY;
        List<Land> landList = new List<Land>();

        uint _minTileX;
        uint _minTileY;
        uint _maxTileX;
        uint _maxTileY;

        // ##################################################################################
        List<int> playerObjectArrayLengthList = new List<int>();
        List<int> worldItemArrayLengthList = new List<int>();
        List<int> worldMobileArrayLengthList = new List<int>();
    	List<int> popupMenuArrayLengthList = new List<int>();
    	List<int> clilocDataArrayLengthList = new List<int>();
    	List<int> playerStatusArrayLengthList = new List<int>();
        List<int> playerSkillListArrayLengthList = new List<int>();
        List<int> staticObjectInfoListArraysLengthList = new List<int>();
        List<int> landRockObjectInfoListArraysLengthList = new List<int>();
        List<int> actionArraysLengthList = new List<int>();

        // ##################################################################################
        byte[] playerObjectArrays;
        byte[] worldItemArrays;
        byte[] worldMobileArrays;
		byte[] popupMenuArrays;
		byte[] clilocDataArrays;
        byte[] playerStatusArrays;
        byte[] playerSkillListArrays;
        byte[] staticObjectInfoListArrays;
        byte[] landRockObjectInfoListArrays;
        byte[] actionArrays;

        // ##################################################################################
        byte[] playerObjectArraysTemp;
        byte[] worldItemArraysTemp;
        byte[] worldMobileArraysTemp;
		byte[] popupMenuArraysTemp;
		byte[] clilocDataArraysTemp;
		byte[] playerStatusArraysTemp;
        byte[] playerSkillListArraysTemp;
        byte[] staticObjectInfoListArraysTemp;
        byte[] landRockObjectInfoListArraysTemp;
        byte[] actionArraysTemp;

        public void SetMinMaxTile(uint minX, uint minY, uint maxX, uint maxY)
	    {
	        _minTileX = minX;
	        _minTileY = minY;
	        _maxTileX = maxX;
	        _maxTileY = maxY;
	    }

    	public void SetUpdateWorldItemsTimer(int time)
	    {
	        _updateWorldItemsTimer = time;
	    }

	    public void SetUpdatePlayerObjectTimer(int time)
	    {
	        _updatePlayerObjectTimer = time;
	    }

    	public void SetActionType(uint value)
	    {
	        grpcAction.ActionType = value;
	    }

	    public void SetWalkDirection(uint value)
	    {
	        grpcAction.WalkDirection = value;
	    }

	    public void SetSourceSerial(uint value)
	    {
	        grpcAction.SourceSerial = value;
	    }

	    public void SetTargetSerial(uint value)
	    {
	        grpcAction.TargetSerial = value;
	    }

	    public void SetIndex(uint value)
	    {
	        grpcAction.Index = value;
	    }

	    public void SetAmount(uint value)
	    {
	        grpcAction.Amount = value;
	    }

	    public void SetRun(bool value)
	    {
	        grpcAction.Run = value;
	    }

	    public int GetEnvStep()
	    {
	        return _envStep;
	    }

	    //public void SetUsedLand(Land usedLand)
	    public void SetUsedLand(uint gameX, uint gameY)
	    {
	    	//Console.WriteLine("SetUsedLand()");
	    	//Console.WriteLine("gameX: {0}, gameY: {1}", gameX, gameY);
	    	//_usedLand = usedLand;
	        _usedLandGameX = gameX;
        	_usedLandGameY = gameY;
	    }

	    public List<GrpcLandObjectData> GetLandObjectList()
	    {
	        return landObjectList;
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
        }

        private void Reset()
        {
        	Console.WriteLine("Reset()");

        	// Clear all List and Array before using them
        	playerObjectArrayLengthList.Clear();
        	worldItemArrayLengthList.Clear();
        	worldMobileArrayLengthList.Clear();
	    	popupMenuArrayLengthList.Clear();
	    	clilocDataArrayLengthList.Clear();
	    	playerStatusArrayLengthList.Clear();
	        playerSkillListArrayLengthList.Clear();
	        staticObjectInfoListArraysLengthList.Clear();
	        landRockObjectInfoListArraysLengthList.Clear();

	        landList.Clear();

	        // ##################################################################################
	        Array.Clear(playerObjectArrays, 0, playerObjectArrays.Length);
	        Array.Clear(worldItemArrays, 0, worldItemArrays.Length);
	        Array.Clear(worldMobileArrays, 0, worldMobileArrays.Length);
	        Array.Clear(popupMenuArrays, 0, popupMenuArrays.Length);
	        Array.Clear(clilocDataArrays, 0, clilocDataArrays.Length);
	        Array.Clear(playerStatusArrays, 0, playerStatusArrays.Length);
	        Array.Clear(playerSkillListArrays, 0, playerSkillListArrays.Length);
	        Array.Clear(staticObjectInfoListArrays, 0, staticObjectInfoListArrays.Length);
	        Array.Clear(landRockObjectInfoListArrays, 0, landRockObjectInfoListArrays.Length);

	        // ##################################################################################
	        Array.Clear(playerObjectArraysTemp, 0, playerObjectArraysTemp.Length);
	        Array.Clear(worldItemArraysTemp, 0, worldItemArraysTemp.Length);
	        Array.Clear(worldMobileArraysTemp, 0, worldMobileArraysTemp.Length);
	        Array.Clear(popupMenuArraysTemp, 0, popupMenuArraysTemp.Length);
	        Array.Clear(clilocDataArraysTemp, 0, clilocDataArraysTemp.Length);
	        Array.Clear(playerStatusArraysTemp, 0, playerStatusArraysTemp.Length);
	        Array.Clear(playerSkillListArraysTemp, 0, playerSkillListArraysTemp.Length);
	        Array.Clear(staticObjectInfoListArraysTemp, 0, staticObjectInfoListArraysTemp.Length);
	        Array.Clear(landRockObjectInfoListArraysTemp, 0, landRockObjectInfoListArraysTemp.Length);

	        //actionArraysLengthList.Clear();
	        //Array.Clear(actionArrays, 0, actionArrays.Length);
	        //Array.Clear(actionArraysTemp, 0, actionArraysTemp.Length);

    		// ##################################################################################
	        map = World.Map;
	        _mapLoad = true;

    		_envStep = 0;
    		_totalStepScale = 2;
	        _updateWorldItemsTimer = -1;
	        _updatePlayerObjectTimer = -1;
	        _usedLandGameX = 0;
        	_usedLandGameY = 0;
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
        	Console.WriteLine("SaveReplayFile()");

        	byte[] playerObjectArrayLengthArray = ConvertIntListToByteArray(playerObjectArrayLengthList);
        	byte[] worldItemArrayLengthArray = ConvertIntListToByteArray(worldItemArrayLengthList);
        	byte[] worldMobileArrayLengthArray = ConvertIntListToByteArray(worldMobileArrayLengthList);
            byte[] popupMenuArrayLengthArray = ConvertIntListToByteArray(popupMenuArrayLengthList);
            byte[] clilocDataArrayLengthArray = ConvertIntListToByteArray(clilocDataArrayLengthList);
	    	byte[] playerStatusArrayLengthArray = ConvertIntListToByteArray(playerStatusArrayLengthList);
	        byte[] playerSkillListArrayLengthArray = ConvertIntListToByteArray(playerSkillListArrayLengthList);
	        byte[] staticObjectInfoListArraysLengthArray = ConvertIntListToByteArray(staticObjectInfoListArraysLengthList);
	        byte[] landRockObjectInfoListArraysLengthArray = ConvertIntListToByteArray(landRockObjectInfoListArraysLengthList);
	        byte[] actionArraysLengthArray = ConvertIntListToByteArray(actionArraysLengthList);

	    	// ##################################################################################
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerObjectLen", playerObjectArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.worldItemLen", worldItemArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.worldMobileLen", worldMobileArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.popupMenuLen", popupMenuArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.clilocDataLen", clilocDataArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerStatusLen", playerStatusArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.playerSkillListLen", playerSkillListArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.staticObjectInfoListArraysLen", staticObjectInfoListArraysLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.landRockObjectInfoListArraysLen", landRockObjectInfoListArraysLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.metadata.actionArraysLen", actionArraysLengthArray);

            // ##################################################################################
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerObject", playerObjectArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.worldItems", worldItemArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.worldMobiles", worldMobileArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.popupMenu", popupMenuArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.clilocData", clilocDataArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerStatus", playerStatusArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.playerSkillList", playerSkillListArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.staticObjectInfoList", staticObjectInfoListArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.landRockObjectInfoList", landRockObjectInfoListArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.data.actionArrays", actionArrays);

            Console.WriteLine("playerObjectArrays.Length: {0}", playerObjectArrays.Length);
            Console.WriteLine("worldItemArrays.Length: {0}", worldItemArrays.Length);
            Console.WriteLine("worldMobileArrays.Length: {0}", worldMobileArrays.Length);
            Console.WriteLine("popupMenuArrays.Length: {0}", popupMenuArrays.Length);
            Console.WriteLine("clilocDataArrays.Length: {0}", clilocDataArrays.Length);
            Console.WriteLine("playerStatusArrays.Length: {0}", playerStatusArrays.Length);
            Console.WriteLine("playerSkillListArrays.Length: {0}", playerSkillListArrays.Length);
            Console.WriteLine("staticObjectInfoListArrays.Length: {0}", staticObjectInfoListArrays.Length);
            Console.WriteLine("landRockObjectInfoListArrays.Length: {0}", landRockObjectInfoListArrays.Length);
            Console.WriteLine("actionArrays.Length: {0}", actionArrays.Length);
        }

        public void UpdatePlayerObject()
        {
        	//Console.WriteLine("UpdatePlayerObject()");

        	// Add player game object 
	        if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	try
	        	{
	        		//Console.WriteLine("_minTileX: {0}, _minTileY: {1}, _maxTileX: {2}, _maxTileY: {3}", 
        			//				    _minTileX, _minTileY, _maxTileX, _maxTileY);

	        		//Console.WriteLine("World.Player.X: {0}, World.Player.Y: {1}", World.Player.X, World.Player.Y);
			        grpcPlayerObject = new GrpcPlayerObject{ GameX=(uint) World.Player.X, GameY=(uint) World.Player.Y, 
			                    							 Serial=World.Player.Serial, Name=World.Player.Name, 
			                    							 Title=World.Player.Title, HoldItemSerial=(uint) ItemHold.Serial, 
			                    							 WarMode=(bool) World.Player.InWarMode, 
			                    							 TargetingState=(uint) TargetManager.TargetingState,
			                    							 MinTileX=_minTileX, MinTileY=_minTileY, MaxTileX=_maxTileX, MaxTileY=_maxTileY };
			    }
			    catch (Exception ex)
	            {
	            	Console.WriteLine("Failed to add the player object: " + ex.Message);
	            }
		    }
		}

		public void AddLandData(Land land)
        {
        	try 
        	{
        		//Console.WriteLine("AddGameObjectInfo()");
        		if (land.Distance <= 12) 
        		{	
        			//Console.WriteLine("name: {0}", name);
    				bool can_drop = (land.Distance >= 1) && (land.Distance < Constants.DRAG_ITEMS_DISTANCE);
    				if (can_drop)
	            	{
		        		landList.Add(land);
		        		//_landObjectAddCount += 1;
		        	}
        		}
	        }
	        catch (Exception ex)
            {
                Console.WriteLine("Failed to add the land object: " + ex.Message);
            }
        }

        public void AddClilocData(uint serial, string text, string affix, string name)
        {
        	if (grpcClilocDataList.Count >= 50) 
        	{
        		//Console.WriteLine("grpcClilocDataList.Count: {0}", grpcClilocDataList.Count);
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
        						  uint amount, uint price, uint layer, uint container)
        {
        	try 
        	{
        		worldItemObjectList.Add(new GrpcItemObjectData{ Distance=distance, GameX=game_x, GameY=game_y, 
	                    										Serial=serial, Name=name, IsCorpse=is_corpse,
	                    										Amount=amount, Price=price, Layer=layer,
	                    										Container=container});
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object: " + ex.Message);
            }
        }

        public void AddMobileObject(uint hits, uint hitsMax, uint race, uint distance, uint game_x, uint game_y, 
        							uint serial, string name, string title, uint notorietyFlag)
        {
        	try 
        	{
        		worldMobileObjectList.Add(new GrpcMobileObjectData{ Hits=hits, HitsMax=hitsMax, Race=race, Distance=distance, 
        															GameX=game_x, GameY=game_y, Serial=serial, Name=name, 
        															Title=title, NotorietyFlag=notorietyFlag });
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the mobile object: " + ex.Message);
            }
        }

        public void AddSimpleObjectInfo(uint game_x, uint game_y, uint distance, string name)
        {
        	try 
        	{
        		//Console.WriteLine("AddGameObjectInfo()");
        		if (distance <= 12) 
        		{	
        			if (name == "Static")
        			{
	        			grpcStaticObjectGameXs.Add(game_x);
			        	grpcStaticObjectGameYs.Add(game_y);
			        }
			        else if (name == "LandRock")
        			{
	        			grpcLandRockObjectGameXs.Add(game_x);
			        	grpcLandRockObjectGameYs.Add(game_y);
			        }
        		}
	        }
	        catch (Exception ex)
            {
                Console.WriteLine("Failed to add the object serial: " + ex.Message);
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
	            		try
		                {
	                    	AddItemObject((uint) item.Distance, (uint) item.X, (uint) item.Y, item.Serial, item.Name, 
	                    			   	item.IsCorpse, item.Amount, item.Price, (uint) item.Layer, (uint) item.Container);
	                    }
	                    catch (Exception ex) 
		                {
		                	Console.WriteLine("serial: {0}, name: {1}", item.Serial, name);
		                    Console.WriteLine("Failed to update the world items: " + ex.Message);
		                }
	            	}
	            }
	        }
        }

        public void UpdateWorldMobiles()
        {
        	//Console.WriteLine("UpdateWorldMobiles()");
        	if ((World.Player != null) && (World.InGame == true)) 
	        {
		        foreach (Mobile mobile in World.Mobiles.Values)
	            {
	            	World.OPL.TryGetNameAndData(mobile.Serial, out string name, out string data);
	            	string title = "None";

	            	if ( (name != null) && (mobile.Serial != World.Player.Serial) )
	            	{
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

	                    AddMobileObject((uint) mobile.Hits, (uint) mobile.HitsMax, (uint) mobile.Race, (uint) mobile.Distance,
	                    				(uint) mobile.X, (uint) mobile.Y, mobile.Serial, name, title, (uint) mobile.NotorietyFlag);
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

	    public void AddToPopupMenuList(string text, bool active)
	    {
	        grpcPopupMenuList.Add(new GrpcPopupMenu { Text=(string) text, Active=(bool) active });
	    }

	    public void ClearPopupMenuList()
	    {
	        grpcPopupMenuList.Clear();
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

        public override Task<GrpcStates> Reset(Config config, ServerCallContext context)
        {
            GrpcStates grpcStates = new GrpcStates();

            Console.WriteLine("Reset()");

            return Task.FromResult(grpcStates);
        }

        public GrpcStates ReadObs(bool config_init)
        {
        	GrpcStates grpcStates = new GrpcStates();
        	
        	//if ( (config_init == true) || (Settings.Replay == false) )
        	if (config_init == true)
        	{
        		//Console.WriteLine("config_init == true");
        		UpdatePlayerObject();
        		UpdateWorldItems();
        		UpdatePlayerStatus();
        		UpdatePlayerSkills();
        	}

        	//Console.WriteLine("_minTileX: {0}, _minTileY: {1}, _maxTileX: {2}, _maxTileY: {3}", 
        	//				  _minTileX, _minTileY, _maxTileX, _maxTileY);

        	if (_updatePlayerObjectTimer == 0) 
        	{
        		UpdatePlayerObject();
        		_updatePlayerObjectTimer = -1;
        	}
        	else if (_updatePlayerObjectTimer > 0)
        	{
        		_updatePlayerObjectTimer -= 1;
        	}

        	if (_updateWorldItemsTimer == 0) 
        	{
        		UpdateWorldItems();
        		UpdateWorldMobiles();
        		UpdatePlayerObject();

        		_updateWorldItemsTimer = -1;
        	}
        	else if (_updateWorldItemsTimer > 0)
        	{
        		_updateWorldItemsTimer -= 1;
        	}

        	if (_envStep % 1000 == 0)
        	{
        		Console.WriteLine("_envStep: {0}", _envStep);
        	}

        	//for (int i = 0; i < landList.Count; i++) 
            //{
            //	Land land = landList[i];
            //    landObjectList.Add(new GrpcLandObjectData{ Index=(uint) i, GameX=(uint) land.X, GameY=(uint) land.Y, 
            //    										   Distance=(uint) land.Distance});
            //}

		    grpcStates.PlayerObject = grpcPlayerObject;

	        GrpcItemObjectList grpcWorldItemList = new GrpcItemObjectList();
            grpcWorldItemList.ItemObjects.AddRange(worldItemObjectList);
            grpcStates.WorldItemList = grpcWorldItemList;

            GrpcMobileObjectList grpcWorldMobileList = new GrpcMobileObjectList();
            grpcWorldMobileList.MobileObjects.AddRange(worldMobileObjectList);
            grpcStates.WorldMobileList = grpcWorldMobileList;

            GrpcPopupMenuList popupMenuList = new GrpcPopupMenuList();
            popupMenuList.Menus.AddRange(grpcPopupMenuList);
            grpcStates.PopupMenuList = popupMenuList;

            GrpcClilocDataList clilocDataList = new GrpcClilocDataList();
            clilocDataList.ClilocDatas.AddRange(grpcClilocDataList);
            grpcStates.ClilocDataList = clilocDataList;

            GrpcSkillList playerSkillList = new GrpcSkillList();
            playerSkillList.Skills.AddRange(grpcPlayerSkillListList);
            grpcStates.PlayerSkillList = playerSkillList;

            grpcStates.PlayerStatus = grpcPlayerStatus;

            //GrpcLandObjectList grpcLandObjectList = new GrpcLandObjectList();
            //GrpcSimpleObjectInfoList staticObjectInfoList = new GrpcSimpleObjectInfoList();
            //GrpcSimpleObjectInfoList landRockObjectInfoList = new GrpcSimpleObjectInfoList();

            // ##################################################################################
            byte[] playerObjectArray = grpcPlayerObject.ToByteArray();

            byte[] worldItemArray = grpcWorldItemList.ToByteArray();
            byte[] worldMobileArray = grpcWorldMobileList.ToByteArray();

            byte[] popupMenuArray = popupMenuList.ToByteArray();
            byte[] clilocDataArray = clilocDataList.ToByteArray();

        	byte[] playerStatusArray = grpcPlayerStatus.ToByteArray();
        	byte[] playerSkillListArray = playerSkillList.ToByteArray();

        	/*
        	if (playerObjectArray.Length != 0)
        	{
        		// ###################################################################
        		grpcLandObjectList.LandObjects.AddRange(landObjectList);
            	grpcStates.LandObjectList = grpcLandObjectList;

        		// ###################################################################
        		staticObjectInfoList.GameXs.AddRange(grpcStaticObjectGameXs);
	            staticObjectInfoList.GameYs.AddRange(grpcStaticObjectGameYs);
	            grpcStates.StaticObjectInfoList = staticObjectInfoList;

	            // ###################################################################
	            landRockObjectInfoList.GameXs.AddRange(grpcLandRockObjectGameXs);
	            landRockObjectInfoList.GameYs.AddRange(grpcLandRockObjectGameYs);
	            grpcStates.LandRockObjectInfoList = landRockObjectInfoList;
        	}

        	byte[] landObjectArray = grpcLandObjectList.ToByteArray();
        	byte[] staticObjectInfoListArray = staticObjectInfoList.ToByteArray();
        	byte[] landRockObjectInfoListArray = landRockObjectInfoList.ToByteArray();
			*/

        	//Console.WriteLine("playerObjectArray.Length: {0}", playerObjectArray.Length);

        	if (_envStep == 0) 
        	{
        		playerObjectArraysTemp = playerObjectArray;
        		worldItemArraysTemp = worldItemArray;
        		worldMobileArraysTemp = worldMobileArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;
        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;
        		//staticObjectInfoListArraysTemp = staticObjectInfoListArray;
        		//landRockObjectInfoListArraysTemp = landRockObjectInfoListArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
        		playerObjectArrays = playerObjectArraysTemp;
        		worldItemArrays = worldItemArraysTemp;
        		worldMobileArrays = worldMobileArraysTemp;
            	popupMenuArrays = popupMenuArraysTemp;
            	clilocDataArrays = clilocDataArraysTemp;
            	playerStatusArrays = playerStatusArraysTemp;
            	playerSkillListArrays = playerSkillListArraysTemp;
				//staticObjectInfoListArrays = staticObjectInfoListArraysTemp;
				//landRockObjectInfoListArrays = landRockObjectInfoListArraysTemp;

				// ##################################################################################
				playerObjectArraysTemp = playerObjectArray;
				worldItemArraysTemp = worldItemArray;
				worldMobileArraysTemp = worldMobileArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;
        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;
        		//staticObjectInfoListArraysTemp = staticObjectInfoListArray;
        		//landRockObjectInfoListArraysTemp = landRockObjectInfoListArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
        		playerObjectArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);
        		worldItemArrays = ConcatByteArrays(worldItemArrays, worldItemArraysTemp);
        		worldMobileArrays = ConcatByteArrays(worldMobileArrays, worldMobileArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);
            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);
            	//staticObjectInfoListArrays = ConcatByteArrays(staticObjectInfoListArrays, staticObjectInfoListArraysTemp);
            	//landRockObjectInfoListArrays = ConcatByteArrays(landRockObjectInfoListArrays, landRockObjectInfoListArraysTemp);

				// ##################################################################################
            	playerObjectArraysTemp = playerObjectArray;
            	worldItemArraysTemp = worldItemArray;
            	worldMobileArraysTemp = worldMobileArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocDataArraysTemp = clilocDataArray;
        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;
        		//staticObjectInfoListArraysTemp = staticObjectInfoListArray;
        		//landRockObjectInfoListArraysTemp = landRockObjectInfoListArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
        		playerObjectArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);
        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);
        		worldMobileArraysTemp = ConcatByteArrays(worldMobileArraysTemp, worldMobileArray);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocDataArrays = ConcatByteArrays(clilocDataArrays, clilocDataArraysTemp);
            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);
            	//staticObjectInfoListArrays = ConcatByteArrays(staticObjectInfoListArrays, staticObjectInfoListArraysTemp);
            	//landRockObjectInfoListArrays = ConcatByteArrays(landRockObjectInfoListArrays, landRockObjectInfoListArraysTemp);
				
            	if (Settings.Replay == true)
	            {
	            	//Console.WriteLine("obs reset / _envStep: {0}", _envStep);
	                SaveReplayFile();
	        		CreateMpqFile();
	        		Reset();
	            }
	            else
	            {
	            	Reset();
	            }

        		return grpcStates;
        	}
        	else
        	{
        		playerObjectArraysTemp = ConcatByteArrays(playerObjectArraysTemp, playerObjectArray);
        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);
        		worldMobileArraysTemp = ConcatByteArrays(worldMobileArraysTemp, worldMobileArray);
            	popupMenuArraysTemp = ConcatByteArrays(popupMenuArraysTemp, popupMenuArray);
            	clilocDataArraysTemp = ConcatByteArrays(clilocDataArraysTemp, clilocDataArray);
            	playerStatusArraysTemp = ConcatByteArrays(playerStatusArraysTemp, playerStatusArray);
            	playerSkillListArraysTemp = ConcatByteArrays(playerSkillListArraysTemp, playerSkillListArray);
            	//staticObjectInfoListArraysTemp = ConcatByteArrays(staticObjectInfoListArraysTemp, staticObjectInfoListArray);
            	//landRockObjectInfoListArraysTemp = ConcatByteArrays(landRockObjectInfoListArraysTemp, landRockObjectInfoListArray);
        	}

        	// ##################################################################################
        	playerObjectArrayLengthList.Add((int) playerObjectArray.Length);
        	worldItemArrayLengthList.Add((int) worldItemArray.Length);
        	worldMobileArrayLengthList.Add((int) worldMobileArray.Length);
			popupMenuArrayLengthList.Add((int) popupMenuArray.Length);
			clilocDataArrayLengthList.Add((int) clilocDataArray.Length);
        	playerStatusArrayLengthList.Add((int) playerStatusArray.Length);
        	playerSkillListArrayLengthList.Add((int) playerSkillListArray.Length);
        	//staticObjectInfoListArraysLengthList.Add((int) staticObjectInfoListArray.Length);
        	//landRockObjectInfoListArraysLengthList.Add((int) landRockObjectInfoListArray.Length);
			
        	// ##################################################################################
        	_envStep++;

	        return grpcStates;
        }

        public override Task<GrpcStates> ReadObs(Config config, ServerCallContext context)
        {
        	//Console.WriteLine("config.Init: {0}", config.Init);
        	GrpcStates obs = ReadObs(config.Init);

            return Task.FromResult(obs);
        }

        public void WriteAct()
        {
        	//Console.WriteLine("WriteAct() _envStep: {0}", _envStep);
            if ( (grpcAction.ActionType != 1) && (grpcAction.ActionType != 0) ) 
		    {
		    	Console.WriteLine("Tick:{0}, Type:{1}, SourceSerial:{2}, TargetSerial:{3}, Index:{4}, Amount:{5}, Direction:{6}, Run:{7}", 
		    		_controller._gameTick, grpcAction.ActionType, grpcAction.SourceSerial, grpcAction.TargetSerial, 
		    		 grpcAction.Index, grpcAction.Amount, grpcAction.WalkDirection, grpcAction.Run);
		    }

		    if (_usedLandGameX != 0)
		    {
		    	Console.WriteLine("_usedLandGameX != 0");
		    	Console.WriteLine("landObjectList.Count: {0}", landObjectList.Count);

        		foreach (GrpcLandObjectData landObject in landObjectList)
                {
                    if ( (landObject.GameX == _usedLandGameX) && (landObject.GameY == _usedLandGameY) )
                    {
                        int index = (int) landObject.Index;
                        Console.WriteLine("GameX: {0}, GameY: {1}, index:{2}", _usedLandGameX, _usedLandGameY, index);
                    }
                }

                _usedLandGameX = 0;
        		_usedLandGameY = 0;
		    }

		    if (grpcAction.ActionType == 0)
		    {
			    grpcAction = new GrpcAction();
			}

			//Console.WriteLine("grpcAction.ActionType: {0}", grpcAction.ActionType);
		    byte[] actionArray = grpcAction.ToByteArray();

        	if (_envStep == 0) 
        	{
        		actionArraysTemp = actionArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
        		actionArrays =actionArraysTemp;

				// ##################################################################################
				actionArraysTemp = actionArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
        		actionArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);

				// ##################################################################################
            	actionArraysTemp = actionArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
        		actionArrays = ConcatByteArrays(actionArrays, actionArraysTemp);

        		// ##################################################################################
        		//Console.WriteLine("action reset / _envStep: {0}", _envStep);
        		actionArraysLengthList.Clear();
		        Array.Clear(actionArrays, 0, actionArrays.Length);
		        Array.Clear(actionArraysTemp, 0, actionArraysTemp.Length);
        	}
        	else
        	{
        		actionArraysTemp = ConcatByteArrays(actionArraysTemp, actionArray);
        	}

        	// ##################################################################################
        	actionArraysLengthList.Add((int) actionArray.Length);

	    	grpcPlayerObject = new GrpcPlayerObject();
	    	worldItemObjectList.Clear();
	    	worldMobileObjectList.Clear();
	    	grpcPopupMenuList.Clear();
	        grpcClilocDataList.Clear();
	        grpcPlayerSkillListList.Clear();
	        grpcPlayerStatus = new GrpcPlayerStatus();
	        //landObjectList.Clear();
        	//grpcStaticObjectGameXs.Clear();
	        //grpcStaticObjectGameYs.Clear();
	        //grpcLandRockObjectGameXs.Clear();
	        //grpcLandRockObjectGameYs.Clear();

	        _usedLandGameX = 0;
        	_usedLandGameY = 0;
	        landList.Clear();

	        grpcAction = new GrpcAction();
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(GrpcAction grpcAction, ServerCallContext context)
        {
    		if (grpcAction.ActionType == 0) {
    			// Do nothing
            	if (World.InGame == true) {
            	}
	        }
            else if (grpcAction.ActionType == 1) {
            	// Walk to Direction
            	if (World.InGame == true) {
            		//Console.WriteLine("grpcAction.WalkDirection: {0}", grpcAction.WalkDirection);

            		if (grpcAction.WalkDirection == 0x00) {
	            		World.Player.Walk(Direction.North, grpcAction.Run);
	            	}
	            	else if (grpcAction.WalkDirection == 0x01) {
	            		World.Player.Walk(Direction.Right, grpcAction.Run);
	            	}	
	            	else if (grpcAction.WalkDirection == 0x02) {
	            		World.Player.Walk(Direction.East, grpcAction.Run);
	            	}
	            	else if (grpcAction.WalkDirection == 0x03) {
	            		World.Player.Walk(Direction.Down, grpcAction.Run);
	            	}
	            	else if (grpcAction.WalkDirection == 0x04) {
	            		World.Player.Walk(Direction.South, grpcAction.Run);
	            	}
	            	else if (grpcAction.WalkDirection == 0x05) {
	            		World.Player.Walk(Direction.Left, grpcAction.Run);
	            	}	
	            	else if (grpcAction.WalkDirection == 0x06) {
	            		World.Player.Walk(Direction.West, grpcAction.Run);
	            	}
	            	else if (grpcAction.WalkDirection == 0x07) {
	            		World.Player.Walk(Direction.Up, grpcAction.Run);
	            	}
            	}
	        }
	        else if (grpcAction.ActionType == 2) {
	        	// Double click the target by it's serial
	        	if (World.Player != null) {
        			GameActions.DoubleClick(grpcAction.TargetSerial);
	        	}
	        }
	        else if (grpcAction.ActionType == 3) {
	        	// Pick up the amount of item by it's serial
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 3");

	        		try
	        		{
	        			//Item item = World.Items.Get(grpcAction.ItemSerial);
	        			//Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
            			//															     	 item.Amount, item.Serial);
	        			GameActions.PickUp(grpcAction.TargetSerial, 0, 0, (int) grpcAction.Amount);
					}
	        		catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to parse the item info: " + ex.Message);
		            }
	        	}
	        }
	        else if (grpcAction.ActionType == 4) {
	        	// Drop the holded item
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 4");
	        		//Console.WriteLine("grpcAction.Index: {0}", grpcAction.Index);

	        		if (grpcAction.TargetSerial != 0)
	        		{
        				GameActions.DropItem((uint) ItemHold.Serial, 0xFFFF, 0xFFFF, 0, grpcAction.TargetSerial);
        			}
        			else if (grpcAction.TargetSerial == 0)
        			{
        				// Drop the holded item on land around the player
		        		//int randomNumber;
						//Random RNG = new Random();
		        		//int index = RNG.Next(itemDropableLandSimpleList.Count);
        				Console.WriteLine("grpcAction.Index: {0}", grpcAction.Index);

		        		uint index = grpcAction.Index;
		        		try
		        		{   
		        			//Vector2 selected = itemDropableLandSimpleList[index];
		        			//GameActions.DropItem((uint) ItemHold.Serial, (int) selected.X, (int) selected.Y, 0, 0xFFFF_FFFF);
		        			GrpcLandObjectData selected = landObjectList[(int) index];
		        			Console.WriteLine("selected.GameX: {0}, selected.GameY: {1}", selected.GameX, selected.GameY);

		        			GameActions.DropItem((uint) ItemHold.Serial, (int) selected.GameX, (int) selected.GameY, 0, 0xFFFF_FFFF);

		        		}
		        		catch (Exception ex)
			            {
			            	Console.WriteLine("Failed to fine the item dropable land: " + ex.Message);
			            }
        			}
	        	}
	        }
	        else if (grpcAction.ActionType == 5) {
	        	if (World.Player != null) 
	        	{
	        		Console.WriteLine("ActionType == 5");
	        		Console.WriteLine("grpcAction.Index: {0}", grpcAction.Index);

	        		//GrpcLandObjectData landObject = landObjectList[grpcAction.Index];
	        		Land targetLand = landList[(int) grpcAction.Index];

	        		Console.WriteLine("targetLand: {0}: ", targetLand);

	        		// Use the abililty to the land target
	        		TargetManager.Target
                    (
                        0, targetLand.X, targetLand.Y, targetLand.Z, targetLand.TileData.IsWet
                    );
	        	}
	        } 
	        else if (grpcAction.ActionType == 6) {
	        	// Equip the holded item
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 6");
                    GameActions.Equip();
	        	}
	        }
	        else if (grpcAction.ActionType == 7) {
	        	// Open the corpse by it's serial
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 7");
                    try
                    {
                    	GameActions.OpenCorpse(grpcAction.TargetSerial);
			        }
			        catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to check the items of the corpse: " + ex.Message);
		            }
	        	}
	        }
	        else if (grpcAction.ActionType == 8) {
	        	// Change the lock status of skill
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 8");

	        		Skill skill = World.Player.Skills[grpcAction.Index];

                    byte newStatus = (byte) skill.Lock;
                    if (newStatus < 2)
                    {
                        newStatus++;
                    }
                    else
                    {
                        newStatus = 0;
                    }

                    //Console.WriteLine("actions.Index: {0}, newStatus: {1}", actions.Index, newStatus);
                    NetClient.Socket.Send_SkillStatusChangeRequest((ushort) grpcAction.Index, newStatus);
                    skill.Lock = (Lock) newStatus;
	        	}
	        }
	        else if (grpcAction.ActionType == 9) {
	        	// 
	        	if (World.Player != null) {
                    Console.WriteLine("ActionType == 9");
	        	}
	        }
	        else if (grpcAction.ActionType == 10) {
	        	if (World.Player != null) {
	        		// Open the pop up menu of the vendor/teacher
	        		Console.WriteLine("ActionType == 10");
	        		GameActions.OpenPopupMenu(grpcAction.TargetSerial);
	        	}
	        }
	        else if (grpcAction.ActionType == 11) {
	        	if (World.Player != null) {
	        		// Select one of menu from the pop up menu the vendor/teacher
	        		Console.WriteLine("ActionType == 11");

	        		GameActions.ResponsePopupMenu(grpcAction.TargetSerial, (ushort) grpcAction.Index);
	        		UIManager.ShowGamePopup(null);
	        	}
	        }
	        else if (grpcAction.ActionType == 12) {
	        	if (World.Player != null) {
	        		// Buy the item from vendor by selecting the item from shop gump
	        		Console.WriteLine("ActionType == 12");

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint) grpcAction.SourceSerial, (ushort) grpcAction.Amount);
	        		NetClient.Socket.Send_BuyRequest(grpcAction.TargetSerial, items);

	        		//UIManager.GetGump<ShopGump>(grpcAction.TargetSerial).CloseWindow();
	        	}
	        }
	        else if (grpcAction.ActionType == 13) {
	        	if (World.Player != null) {
	        		// Sell the item to vendor by selecting the item from shop gump
	        		Console.WriteLine("ActionType == 13");

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint) grpcAction.SourceSerial, (ushort) grpcAction.Amount);
	        		NetClient.Socket.Send_SellRequest(grpcAction.TargetSerial, items);

	        		//UIManager.GetGump<ShopGump>(grpcAction.TargetSerial).CloseWindow();
	        	}
	        }
	        else if (grpcAction.ActionType == 14) {
	        	if (World.Player != null) {
	        		// Use the bandage myself
	        		Console.WriteLine("ActionType == 14");
	        		Item bandage = World.Player.FindBandage();
	        		if (bandage != null) 
	        		{
	        			Console.WriteLine("Serial: {0}, Amount: {0}", bandage.Serial, bandage.Amount);
	        			NetClient.Socket.Send_TargetSelectedObject(bandage.Serial, grpcAction.TargetSerial);
	        		}
	        	}
	        }
	        else if (grpcAction.ActionType == 15) {
	        	if (World.Player != null) {
	        		// Open the door in front of player
	        		Console.WriteLine("ActionType == 15");
	        		GameActions.OpenDoor();
	        	}
	        }
	        else if (grpcAction.ActionType == 16) {
	        	if (World.Player != null) {

	        	}
	        }
	        else if (grpcAction.ActionType == 17) {
	        	if (World.Player != null) {
	        		// Close the pop up menu
	        		Console.WriteLine("ActionType == 17");
	        		UIManager.ShowGamePopup(null);
	        	}
	        }
	        else if (grpcAction.ActionType == 18) {
	        	if (World.Player != null) {
	        		
	        	}
	        }
	        else if (grpcAction.ActionType == 19) {
	        	if (World.Player != null) {
	        		// Change the war mode
	        		Console.WriteLine("ActionType == 19");
	        		bool boolIndex = Convert.ToBoolean(grpcAction.Index);

	        		//Console.WriteLine("boolIndex: {0}", boolIndex);
        			NetClient.Socket.Send_ChangeWarMode(boolIndex);
        			World.Player.InWarMode = boolIndex;
	        	}
	        }

    		grpcPlayerObject = new GrpcPlayerObject();
    		worldItemObjectList.Clear();
	    	worldMobileObjectList.Clear();
	    	grpcPopupMenuList.Clear();
	        grpcClilocDataList.Clear();
	        grpcPlayerSkillListList.Clear();
	        grpcPlayerStatus = new GrpcPlayerStatus();
	        //landObjectList.Clear();
        	//grpcStaticObjectGameXs.Clear();
	        //grpcStaticObjectGameYs.Clear();
	        //grpcLandRockObjectGameXs.Clear();
	        //grpcLandRockObjectGameYs.Clear();

	        _usedLandGameX = 0;
        	_usedLandGameY = 0;
	        landList.Clear();

	        grpcAction = new GrpcAction();

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