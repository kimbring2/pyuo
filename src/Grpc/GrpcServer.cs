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
        List<GrpcItemObject> worldItemObjectList = new List<GrpcItemObject>();
        List<GrpcMobileObject> worldMobileObjectList = new List<GrpcMobileObject>();
        List<GrpcPopupMenu> grpcPopupMenuList = new List<GrpcPopupMenu>();
        List<GrpcCliloc> grpcClilocList = new List<GrpcCliloc>();
        List<GrpcVendor> grpcVendorList = new List<GrpcVendor>();
        List<Vector2> itemDropableLandSimpleList = new List<Vector2>();
        GrpcPlayerStatus grpcPlayerStatus = new GrpcPlayerStatus();
        List<GrpcSkill> grpcPlayerSkillList = new List<GrpcSkill>();
        List<GrpcBuff> grpcPlayerBuffList = new List<GrpcBuff>();
        List<uint> grpcDeleteItemSerials = new List<uint>();
        List<uint> grpcDeleteMobileSerials = new List<uint>();
        List<GrpcMenuControl> grpcMenuControlList = new List<GrpcMenuControl>();

        GrpcAction grpcAction = new GrpcAction();

        uint _totalStepScale;
        int _envStep;
        string _replayName;
        string _replayPath;
        int _updateWorldItemsTimer;
        int _updatePlayerObjectTimer;

        int _checkUpdatedObjectTimer;

        uint _minTileX;
        uint _minTileY;
        uint _maxTileX;
        uint _maxTileY;

        uint _usedLandIndex;

        uint _gumpLocalSerial;
        uint _gumpServerSerial;
        uint _gumpWidth;
        uint _gumpHeight;
        uint _gumpMaxPage;

        // ##################################################################################
        List<int> playerObjectArrayLengthList = new List<int>();
        List<int> worldItemArrayLengthList = new List<int>();
        List<int> worldMobileArrayLengthList = new List<int>();
    	List<int> popupMenuArrayLengthList = new List<int>();
    	List<int> clilocArrayLengthList = new List<int>();
    	List<int> vendorListArrayLengthList = new List<int>();
    	List<int> playerStatusArrayLengthList = new List<int>();
        List<int> playerSkillListArrayLengthList = new List<int>();
        List<int> playerBuffListArrayLengthList = new List<int>();
        List<int> deleteItemSerialsArrayLengthList = new List<int>();
        List<int> deleteMobileSerialsArrayLengthList = new List<int>();
        List<int> actionArraysLengthList = new List<int>();

        List<uint> updatedObjectSerialList = new List<uint>();

        // ##################################################################################
        byte[] playerObjectArrays;
        byte[] worldItemArrays;
        byte[] worldMobileArrays;
		byte[] popupMenuArrays;
		byte[] clilocArrays;
		byte[] vendorListArrays;
        byte[] playerStatusArrays;
        byte[] playerSkillListArrays;
        byte[] playerBuffListArrays;
        byte[] deleteItemSerialsArrays;
        byte[] deleteMobileSerialsArrays;
        byte[] actionArrays;

        // ##################################################################################
        byte[] playerObjectArraysTemp;
        byte[] worldItemArraysTemp;
        byte[] worldMobileArraysTemp;
		byte[] popupMenuArraysTemp;
		byte[] clilocArraysTemp;
		byte[] vendorListArraysTemp;
		byte[] playerStatusArraysTemp;
        byte[] playerSkillListArraysTemp;
        byte[] playerBuffListArraysTemp;
        byte[] deleteItemSerialsArraysTemp;
        byte[] deleteMobileSerialsArraysTemp;
        byte[] actionArraysTemp;

        public void AddUpdatedObjectSerial(uint serial)
        {
        	updatedObjectSerialList.Add((uint) serial);
        }

        public void SetMinMaxTile(uint minX, uint minY, uint maxX, uint maxY)
	    {
	        _minTileX = minX;
	        _minTileY = minY;
	        _maxTileX = maxX;
	        _maxTileY = maxY;
	    }

	    public void SetGumpData(uint localSerial, uint serverSerial, uint width, uint height, uint maxPage)
	    {
	        _gumpLocalSerial = localSerial;
	        _gumpServerSerial = serverSerial;
	        _gumpWidth = width;
        	_gumpHeight = height;
        	_gumpMaxPage = maxPage;
	    }

	    public void SetUpdatedObjectTimer(int time)
	    {
	        _checkUpdatedObjectTimer = time;
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

	    public void SetUsedLand(int gameX, int gameY)
	    {
	    	//Console.WriteLine("SetUsedLand()");

        	_usedLandIndex = GetLandIndex((uint) gameX, (uint) gameY);
        	SetIndex(_usedLandIndex);
	    }

	    public uint GetLandIndex(uint gameX, uint gameY)
	    {
	    	//Console.WriteLine("GetLandIndex()");

	    	uint x_relative = 0;
	    	for (uint i = _minTileX; i < _maxTileX; i++)
            {
            	if (gameX == i)
                {
                	x_relative = i - _minTileX;
                }
            }

            uint y_relative = 0;

            for (uint i = _minTileY; i < _maxTileY; i++)
            {
            	if (gameY == i)
                {
                	y_relative = i - _minTileY;
                }
            }

            uint index = x_relative * (_maxTileX - _minTileX) + y_relative;

            return index;
	    }

	    public Vector2 GetLandPosition(uint index)
	    {
	    	//Console.WriteLine("GetLandPosition()");

	    	uint gameX = index / (_maxTileX - _minTileX);
	    	uint gameY = index % (_maxTileX - _minTileX);
	    	//Console.WriteLine("gameX: {0}, gameY: {1}", gameX, gameY);

	    	Vector2 landPosition = new Vector2();
	    	landPosition.X = (uint) (gameX + _minTileX);
	    	landPosition.Y = (uint) (gameY + _minTileY);

            return landPosition;
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
    		_totalStepScale = Settings.ReplayLengthScale;
    		//_totalStepScale = 4;
	        _updateWorldItemsTimer = -1;
	        _updatePlayerObjectTimer = -1;
	        _checkUpdatedObjectTimer = -1;
        	_usedLandIndex = 0;
        	_gumpLocalSerial = 0;
        	_gumpServerSerial = 0;
        	_gumpWidth = 0;
        	_gumpHeight = 0;
        	_gumpMaxPage = 0;

        	Console.WriteLine("_totalStepScale: {0}", _totalStepScale);
        }

        private void Reset()
        {
        	//Console.WriteLine("Reset()");

        	// Clear all List and Array before using them
        	playerObjectArrayLengthList.Clear();
        	worldItemArrayLengthList.Clear();
        	worldMobileArrayLengthList.Clear();
	    	popupMenuArrayLengthList.Clear();
	    	clilocArrayLengthList.Clear();
	    	vendorListArrayLengthList.Clear();
	    	playerStatusArrayLengthList.Clear();
	        playerSkillListArrayLengthList.Clear();
			playerBuffListArrayLengthList.Clear();
			deleteItemSerialsArrayLengthList.Clear();
        	deleteMobileSerialsArrayLengthList.Clear();

	        // ##################################################################################
	        Array.Clear(playerObjectArrays, 0, playerObjectArrays.Length);
	        Array.Clear(worldItemArrays, 0, worldItemArrays.Length);
	        Array.Clear(worldMobileArrays, 0, worldMobileArrays.Length);
	        Array.Clear(popupMenuArrays, 0, popupMenuArrays.Length);
	        Array.Clear(clilocArrays, 0, clilocArrays.Length);
	        Array.Clear(vendorListArrays, 0, vendorListArrays.Length);
	        Array.Clear(playerStatusArrays, 0, playerStatusArrays.Length);
	        Array.Clear(playerSkillListArrays, 0, playerSkillListArrays.Length);
	        Array.Clear(playerBuffListArrays, 0, playerBuffListArrays.Length);
	        Array.Clear(deleteItemSerialsArrays, 0, deleteItemSerialsArrays.Length);
	        Array.Clear(deleteMobileSerialsArrays, 0, deleteMobileSerialsArrays.Length);

	        // ##################################################################################
	        Array.Clear(playerObjectArraysTemp, 0, playerObjectArraysTemp.Length);
	        Array.Clear(worldItemArraysTemp, 0, worldItemArraysTemp.Length);
	        Array.Clear(worldMobileArraysTemp, 0, worldMobileArraysTemp.Length);
	        Array.Clear(popupMenuArraysTemp, 0, popupMenuArraysTemp.Length);
	        Array.Clear(clilocArraysTemp, 0, clilocArraysTemp.Length);
	        Array.Clear(vendorListArraysTemp, 0, vendorListArraysTemp.Length);
	        Array.Clear(playerStatusArraysTemp, 0, playerStatusArraysTemp.Length);
	        Array.Clear(playerSkillListArraysTemp, 0, playerSkillListArraysTemp.Length);
	        Array.Clear(playerBuffListArraysTemp, 0, playerBuffListArraysTemp.Length);
	        Array.Clear(deleteItemSerialsArraysTemp, 0, deleteItemSerialsArraysTemp.Length);
	        Array.Clear(deleteMobileSerialsArraysTemp, 0, deleteMobileSerialsArraysTemp.Length);

	        // ##################################################################################
	        //Console.WriteLine("actionArray reset / _envStep: {0}", _envStep);
	        //actionArraysLengthList.Clear();
	        //Array.Clear(actionArrays, 0, actionArrays.Length);
	        //Array.Clear(actionArraysTemp, 0, actionArraysTemp.Length);

    		// ##################################################################################
    		_envStep = 0;
    		_totalStepScale = Settings.ReplayLengthScale;
	        _updateWorldItemsTimer = -1;
	        _updatePlayerObjectTimer = -1;
	        _checkUpdatedObjectTimer = -1;
        	_usedLandIndex = 0;
        	_gumpLocalSerial = 0;
        	_gumpServerSerial = 0;
        	_gumpWidth = 0;
        	_gumpHeight = 0;
        	_gumpMaxPage = 0;
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
            byte[] clilocArrayLengthArray = ConvertIntListToByteArray(clilocArrayLengthList);
            byte[] vendorListArrayLengthArray = ConvertIntListToByteArray(vendorListArrayLengthList);
	    	byte[] playerStatusArrayLengthArray = ConvertIntListToByteArray(playerStatusArrayLengthList);
	        byte[] playerSkillListArrayLengthArray = ConvertIntListToByteArray(playerSkillListArrayLengthList);
	        byte[] playerBuffListArrayLengthArray = ConvertIntListToByteArray(playerBuffListArrayLengthList);
	        byte[] deleteItemSerialsArrayLengthArray = ConvertIntListToByteArray(deleteItemSerialsArrayLengthList);
	        byte[] deleteMobileSerialsArrayLengthArray = ConvertIntListToByteArray(deleteMobileSerialsArrayLengthList);
	        byte[] actionArraysLengthArray = ConvertIntListToByteArray(actionArraysLengthList);

	    	// ##################################################################################
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.playerObjectLen", playerObjectArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.worldItemLen", worldItemArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.worldMobileLen", worldMobileArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.popupMenuLen", popupMenuArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.clilocLen", clilocArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.vendorListLen", vendorListArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.playerStatusLen", playerStatusArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.playerSkillListLen", playerSkillListArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.playerBuffListLen", playerBuffListArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.deleteItemSerialsLen", deleteItemSerialsArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.deleteMobileSerialsLen", deleteMobileSerialsArrayLengthArray);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.meta.actionArraysLen", actionArraysLengthArray);

            // ##################################################################################
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.playerObject", playerObjectArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.worldItems", worldItemArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.worldMobiles", worldMobileArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.popupMenu", popupMenuArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.cliloc", clilocArrays);
			WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.vendorList", vendorListArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.playerStatus", playerStatusArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.playerSkillList", playerSkillListArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.playerBuffList", playerBuffListArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.deleteItemSerials", deleteItemSerialsArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.deleteMobileSerials", deleteMobileSerialsArrays);
            WrtieToMpqArchive(_replayPath + _replayName + ".uoreplay", "replay.actionArrays", actionArrays);

            Console.WriteLine("playerObjectArrays.Length: {0}", playerObjectArrays.Length);
            Console.WriteLine("worldItemArrays.Length: {0}", worldItemArrays.Length);
            Console.WriteLine("worldMobileArrays.Length: {0}", worldMobileArrays.Length);
            Console.WriteLine("popupMenuArrays.Length: {0}", popupMenuArrays.Length);
            Console.WriteLine("clilocArrays.Length: {0}", clilocArrays.Length);
            Console.WriteLine("vendorListArrays.Length: {0}", vendorListArrays.Length);
            Console.WriteLine("playerStatusArrays.Length: {0}", playerStatusArrays.Length);
            Console.WriteLine("playerSkillListArrays.Length: {0}", playerSkillListArrays.Length);
            Console.WriteLine("playerBuffListArrays.Length: {0}", playerBuffListArrays.Length);
            Console.WriteLine("deleteMobileSerialsArrays.Length: {0}", deleteMobileSerialsArrays.Length);
            Console.WriteLine("deleteItemSerialsArrays.Length: {0}", deleteItemSerialsArrays.Length);
            Console.WriteLine("actionArrays.Length: {0}", actionArrays.Length);
        }

        public void UpdatePlayerObject()
        {
        	//Console.WriteLine("UpdatePlayerObject()");
	        if ((World.Player != null) && (World.InGame == true)) 
	        {
	        	try
	        	{
	        		List<uint> activeGumps = new List<uint>();
	        		for (LinkedListNode<Gump> last = UIManager.Gumps.Last; last != null; last = last.Previous)
		            {
			            Control g = last.Value;
			            //Console.WriteLine("g.LocalSerial: {0}", g.LocalSerial);

			            if (g.LocalSerial != 0)
			            {
			            	activeGumps.Add((uint) g.LocalSerial);
			            }
		            }
		            //Console.WriteLine("");

	        		//Console.WriteLine("World.Player.X: {0}, World.Player.Y: {1}", World.Player.X, World.Player.Y);
			        grpcPlayerObject = new GrpcPlayerObject{ GameX=(uint) World.Player.X, GameY=(uint) World.Player.Y, 
			                    							 Serial=World.Player.Serial, Name=World.Player.Name, 
			                    							 Title=World.Player.Title, HoldItemSerial=(uint) ItemHold.Serial, 
			                    							 WarMode=(bool) World.Player.InWarMode, 
			                    							 TargetingState=(int) TargetManager.TargetingState,
			                    							 MinTileX=_minTileX, MinTileY=_minTileY, MaxTileX=_maxTileX, MaxTileY=_maxTileY };

			        grpcPlayerObject.ActiveGumps.AddRange(activeGumps);
			    }
			    catch (Exception ex)
	            {
	            	//Console.WriteLine("Failed to add the player object: " + ex.Message);
	            }
		    }
		}

		public void AddMenuControl(string name, uint x, uint y, uint page, string text, uint id=0)
        {
        	grpcMenuControlList.Add(new GrpcMenuControl{ Name=name, X=x, Y=y, Page=page, Text=text, Id=id });
        }

        public void AddCliloc(uint serial, string text, string affix, string name)
        {
        	if (grpcClilocList.Count >= 50) 
        	{
        		//Console.WriteLine("grpcClilocList.Count: {0}", grpcClilocList.Count);
        	}

        	grpcClilocList.Add(new GrpcCliloc{ Serial=serial, Text=text, Affix=affix, Name=name });
        }

        public void AddVendor(uint vendorSerial, uint itemSerial, uint itemGraphic, uint itemHue, uint itemAmount, 
        					  uint itemPrice, string itemName)
        {
        	//Console.WriteLine("AddVendor()");
        	//Console.WriteLine("VendorSerial: {0}, ItemSerial: {1}, ItemGraphic: {2}, ItemHue: {3}, ItemAmount: {4}, ItemPrice: {5}, itemName: {6}", 
        	//				   vendorSerial, itemSerial, itemGraphic, itemHue, itemAmount, itemPrice, itemName);

        	grpcVendorList.Add(new GrpcVendor{ VendorSerial=vendorSerial, ItemSerial=itemSerial, ItemGraphic=itemGraphic,
        									   ItemHue=itemHue, ItemAmount=itemAmount, ItemPrice=itemPrice, ItemName=itemName });
        	//grpcVendorList.Add(new GrpcVendor{ VendorSerial=1, ItemSerial=1, ItemGraphic=1,
        	//								   ItemHue=1, ItemAmount=1, ItemPrice=1, ItemName="test" });
        	Console.WriteLine("");
        }

        public void AddItemObject(uint distance, uint game_x, uint game_y, uint serial, string name, bool is_corpse, 
        						  uint amount, uint price, uint layer, uint container, string data, bool opened)
        {
        	try 
        	{
        		worldItemObjectList.Add(new GrpcItemObject{ Distance=distance, GameX=game_x, GameY=game_y, 
	                    									Serial=serial, Name=name, IsCorpse=is_corpse,
	                    									Amount=amount, Price=price, Layer=layer,
	                    									Container=container, Data=data, Opened=opened});
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the object: " + ex.Message);
            }
        }

        public void AddMobileObject(uint hits, uint hitsMax, uint race, uint distance, uint game_x, uint game_y, 
        							uint serial, string name, string title, uint notorietyFlag, bool is_dead)
        {
        	try 
        	{
        		worldMobileObjectList.Add(new GrpcMobileObject{ Hits=hits, HitsMax=hitsMax, Race=race, Distance=distance, 
        														GameX=game_x, GameY=game_y, Serial=serial, Name=name, 
        														Title=title, NotorietyFlag=notorietyFlag, IsDead=is_dead });
	        }
	        catch (Exception ex)
            {
                //Console.WriteLine("Failed to add the mobile object: " + ex.Message);
            }
        }

        public void AddDeleteItemSerial(uint serial)
        {
        	grpcDeleteItemSerials.Add(serial);
        }

        public void AddDeleteMobileSerial(uint serial)
        {
        	grpcDeleteMobileSerials.Add(serial);
        }

        public void UpdatePlayerSkills()
        {
        	// Add player skill 
	        if ((World.Player != null) && (World.InGame == true))
	        {
	        	//Console.WriteLine("UpdatePlayerSkills()");
		        for (int i = 0; i < World.Player.Skills.Length; i++)
	            {
	            	Skill skill = World.Player.Skills[i];
	            	//Console.WriteLine("Name: {0}, Index: {1}, IsClickable: {2}, Value: {3}, Base: {4}, Cap: {5}, Lock: {6}", 
	            	//	skill.Name, skill.Index, skill.IsClickable, skill.Value, skill.Base, skill.Cap, skill.Lock);
	            	grpcPlayerSkillList.Add(new GrpcSkill{ Name=skill.Name, Index=(uint) skill.Index, 
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

	    public void UpdatePlayerBuffs()
        {
        	//Console.WriteLine("UpdatePlayerBuffs()");

        	// Add player status 
	        if ((World.Player != null) && (World.InGame == true))
            {
            	foreach (KeyValuePair<BuffIconType, BuffIcon> k in World.Player.BuffIcons)
		        {
		            uint buffIconType = (uint) k.Key;
		            uint type = (uint) k.Value.Type;
		            uint graphic = (uint) k.Value.Graphic;
		            uint timer = (uint) k.Value.Timer;
		            string text = k.Value.Text;

		            uint delta = (uint) (k.Value.Timer - Time.Ticks);
		            //Console.WriteLine("buffIconType: {0}, type: {1}, graphic: {2}, delta: {3}, text: {4}", 
		            //				   buffIconType, type, graphic, delta, text);

	            	grpcPlayerBuffList.Add(new GrpcBuff{ Type=(uint) k.Value.Type, Delta= delta, Text=k.Value.Text });
		        }

		        //Console.WriteLine("");
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

            //Console.WriteLine("Reset()");

            return Task.FromResult(grpcStates);
        }

        public GrpcStates ReadObs(bool config_init)
        {
        	//Console.WriteLine("ReadObs() _envStep: {0}", _envStep);

        	GrpcStates grpcStates = new GrpcStates();
        	
        	if (config_init == true)
        	{
        		//Console.WriteLine("config_init == true");
        		UpdatePlayerObject();
        		UpdatePlayerStatus();
        		UpdatePlayerSkills();
        		UpdatePlayerBuffs();
        	}

        	if (_checkUpdatedObjectTimer == 0) 
        	{
				foreach (uint itemSerial in updatedObjectSerialList) 
				{
		            World.OPL.TryGetNameAndData(itemSerial, out string name, out string data);
		            Item item = World.Items.Get(itemSerial);
		            Console.WriteLine("name: {0}, container: {1}", name, item.Container);
				}

				//Console.WriteLine("");
				updatedObjectSerialList.Clear();

        		_checkUpdatedObjectTimer = -1;
        	}
        	else if (_checkUpdatedObjectTimer > 0)
        	{
        		_checkUpdatedObjectTimer -= 1;
        	}

        	if (_updatePlayerObjectTimer == 0) 
        	{
        		UpdatePlayerObject();
        		_updatePlayerObjectTimer = -1;
        	}
        	else if (_updatePlayerObjectTimer > 0)
        	{
        		_updatePlayerObjectTimer -= 1;
        	}

        	if (_envStep % 2000 == 0)
        	{
        		Console.WriteLine("_envStep: {0}", _envStep);
        	}

        	if ((World.Player != null) && (World.InGame == true)) 
	        {
		        foreach (Item item in World.Items.Values)
	            {
	            	World.OPL.TryGetNameAndData(item.Serial, out string name, out string data);
	            	//Console.WriteLine("world item, name: {0}", item.Name);
	            	if (name != null)
	            	{
	            		if ( (name.Contains("Door") == false) && (name.Contains("Vendors") == false) ) 
	            		{
	                    	//Console.WriteLine("world item, name: {0}, opened: {1}", name, item.Opened);
	                    }
	            	}
	            }
	            //Console.WriteLine("");
	        }

	        if ((World.Player != null) && (World.InGame == true)) 
	        {
		        foreach (Mobile mobile in World.Mobiles.Values)
	            {
	            	World.OPL.TryGetNameAndData(mobile.Serial, out string name, out string data);
	            	//Console.WriteLine("world item, name: {0}", item.Name);
	            	if (name != null)
	            	{
	                	//Console.WriteLine("world mobile, name: {0}, isDead: {1}", name, mobile.IsDead);
	            	}
	            }

	            //Console.WriteLine("");
	        }

        	//Console.WriteLine("TargetingState: {0}", TargetManager.TargetingState);

        	//UpdatePlayerBuffs();
        	//UpdatePlayerObject();

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

            GrpcClilocList clilocList = new GrpcClilocList();
            clilocList.Clilocs.AddRange(grpcClilocList);
            grpcStates.ClilocList = clilocList;

            GrpcVendorList vendorList = new GrpcVendorList();

            if (grpcVendorList.Count != 0) 
	        {
	        	//Console.WriteLine("grpcVendorList()");
		        foreach (GrpcVendor v in grpcVendorList)
	            {
	            	//grpcVendorList.Add(new GrpcVendor{ VendorSerial=1, ItemSerial=1, ItemGraphic=1,
        			//								     ItemHue=1, ItemAmount=1, ItemPrice=1, ItemName="test" });

            		//Console.WriteLine("VendorSerial: {0}, ItemSerial: {1}, ItemGraphic: {2}, ItemHue: {3}, ItemAmount: {4}, ItemPrice: {5}, itemName: {6}", 
        			//		   		   v.VendorSerial, v.ItemSerial, v.ItemGraphic, v.ItemHue, v.ItemAmount, v.ItemPrice, v.ItemName);
            	}
            }

            vendorList.Vendors.AddRange(grpcVendorList);
            grpcStates.VendorList = vendorList;

            GrpcSkillList playerSkillList = new GrpcSkillList();
            playerSkillList.Skills.AddRange(grpcPlayerSkillList);
            grpcStates.PlayerSkillList = playerSkillList;

            grpcStates.PlayerStatus = grpcPlayerStatus;

            GrpcBuffList playerBuffList = new GrpcBuffList();
            playerBuffList.Buffs.AddRange(grpcPlayerBuffList);
            grpcStates.PlayerBuffList = playerBuffList;

            GrpcDeleteItemSerialList deleteItemSerialList = new GrpcDeleteItemSerialList();
            deleteItemSerialList.Serials.AddRange(grpcDeleteItemSerials);
            grpcStates.DeleteItemSerialList = deleteItemSerialList;

            GrpcDeleteMobileSerialList deleteMobileSerialList = new GrpcDeleteMobileSerialList();
            deleteMobileSerialList.Serials.AddRange(grpcDeleteMobileSerials);
            grpcStates.DeleteMobileSerialList = deleteMobileSerialList;

            GrpcMenuControlList menuControlList = new GrpcMenuControlList();
            menuControlList.LocalSerial = _gumpLocalSerial;
            menuControlList.ServerSerial = _gumpServerSerial;
            menuControlList.Width = _gumpWidth;
        	menuControlList.Height = _gumpHeight;
        	menuControlList.MaxPage = _gumpMaxPage;
            menuControlList.MenuControls.AddRange(grpcMenuControlList);
            grpcStates.MenuControlList = menuControlList;

            // ##################################################################################
            byte[] playerObjectArray = grpcPlayerObject.ToByteArray();

            byte[] worldItemArray = grpcWorldItemList.ToByteArray();
            byte[] worldMobileArray = grpcWorldMobileList.ToByteArray();

            byte[] popupMenuArray = popupMenuList.ToByteArray();
            byte[] clilocArray = clilocList.ToByteArray();
            byte[] vendorListArray = vendorList.ToByteArray();

        	byte[] playerStatusArray = grpcPlayerStatus.ToByteArray();
        	byte[] playerSkillListArray = playerSkillList.ToByteArray();
        	byte[] playerBuffListArray = playerBuffList.ToByteArray();

        	byte[] deleteItemSerialsArray = deleteItemSerialList.ToByteArray();
        	byte[] deleteMobileSerialsArray = deleteMobileSerialList.ToByteArray();

        	if (grpcVendorList.Count != 0) 
	        {
	        	//Console.WriteLine("vendorListArray.Length: {0}", vendorListArray.Length);
	        	//Console.WriteLine("");
	        }

        	if (_envStep == 0) 
        	{
        		playerObjectArraysTemp = playerObjectArray;
        		worldItemArraysTemp = worldItemArray;
        		worldMobileArraysTemp = worldMobileArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocArraysTemp = clilocArray;
        		vendorListArraysTemp = vendorListArray;
        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;
        		playerBuffListArraysTemp = playerBuffListArray;
        		deleteItemSerialsArraysTemp = deleteItemSerialsArray;
        		deleteMobileSerialsArraysTemp = deleteMobileSerialsArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
        		playerObjectArrays = playerObjectArraysTemp;
        		worldItemArrays = worldItemArraysTemp;
        		worldMobileArrays = worldMobileArraysTemp;
            	popupMenuArrays = popupMenuArraysTemp;
            	clilocArrays = clilocArraysTemp;
            	vendorListArrays = vendorListArraysTemp;
            	playerStatusArrays = playerStatusArraysTemp;
            	playerSkillListArrays = playerSkillListArraysTemp;
            	playerBuffListArrays = playerBuffListArraysTemp;
            	deleteItemSerialsArrays = deleteItemSerialsArraysTemp;
        		deleteMobileSerialsArrays = deleteMobileSerialsArraysTemp;

				// ##################################################################################
				playerObjectArraysTemp = playerObjectArray;
				worldItemArraysTemp = worldItemArray;
				worldMobileArraysTemp = worldMobileArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocArraysTemp = clilocArray;
        		vendorListArraysTemp = vendorListArray;
        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;
        		playerBuffListArraysTemp = playerBuffListArray;
        		deleteItemSerialsArraysTemp = deleteItemSerialsArray;
        		deleteMobileSerialsArraysTemp = deleteMobileSerialsArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
        		playerObjectArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);
        		worldItemArrays = ConcatByteArrays(worldItemArrays, worldItemArraysTemp);
        		worldMobileArrays = ConcatByteArrays(worldMobileArrays, worldMobileArraysTemp);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocArrays = ConcatByteArrays(clilocArrays, clilocArraysTemp);
            	vendorListArrays = ConcatByteArrays(vendorListArrays, vendorListArraysTemp);
            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);
            	playerBuffListArrays = ConcatByteArrays(playerBuffListArrays, playerBuffListArraysTemp);
            	deleteItemSerialsArrays = ConcatByteArrays(deleteItemSerialsArrays, deleteItemSerialsArraysTemp);
            	deleteMobileSerialsArrays = ConcatByteArrays(deleteMobileSerialsArrays, deleteMobileSerialsArraysTemp);

				// ##################################################################################
            	playerObjectArraysTemp = playerObjectArray;
            	worldItemArraysTemp = worldItemArray;
            	worldMobileArraysTemp = worldMobileArray;
        		popupMenuArraysTemp = popupMenuArray;
        		clilocArraysTemp = clilocArray;
        		vendorListArraysTemp = vendorListArray;
        		playerStatusArraysTemp = playerStatusArray;
        		playerSkillListArraysTemp = playerSkillListArray;
        		playerBuffListArraysTemp = playerBuffListArray;
        		deleteItemSerialsArraysTemp = deleteItemSerialsArray;
        		deleteMobileSerialsArraysTemp = deleteMobileSerialsArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
        		playerObjectArrays = ConcatByteArrays(playerObjectArrays, playerObjectArraysTemp);
        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);
        		worldMobileArraysTemp = ConcatByteArrays(worldMobileArraysTemp, worldMobileArray);
            	popupMenuArrays = ConcatByteArrays(popupMenuArrays, popupMenuArraysTemp);
            	clilocArrays = ConcatByteArrays(clilocArrays, clilocArraysTemp);
            	vendorListArrays = ConcatByteArrays(vendorListArrays, vendorListArraysTemp);
            	playerStatusArrays = ConcatByteArrays(playerStatusArrays, playerStatusArraysTemp);
            	playerSkillListArrays = ConcatByteArrays(playerSkillListArrays, playerSkillListArraysTemp);
            	playerBuffListArrays = ConcatByteArrays(playerBuffListArrays, playerBuffListArraysTemp);
            	deleteItemSerialsArrays = ConcatByteArrays(deleteItemSerialsArrays, deleteItemSerialsArraysTemp);
            	deleteMobileSerialsArrays = ConcatByteArrays(deleteMobileSerialsArrays, deleteMobileSerialsArraysTemp);
				
        		return grpcStates;
        	}
        	else
        	{
        		playerObjectArraysTemp = ConcatByteArrays(playerObjectArraysTemp, playerObjectArray);
        		worldItemArraysTemp = ConcatByteArrays(worldItemArraysTemp, worldItemArray);
        		worldMobileArraysTemp = ConcatByteArrays(worldMobileArraysTemp, worldMobileArray);
            	popupMenuArraysTemp = ConcatByteArrays(popupMenuArraysTemp, popupMenuArray);
            	clilocArraysTemp = ConcatByteArrays(clilocArraysTemp, clilocArray);
            	vendorListArraysTemp = ConcatByteArrays(vendorListArraysTemp, vendorListArray);
            	playerStatusArraysTemp = ConcatByteArrays(playerStatusArraysTemp, playerStatusArray);
            	playerSkillListArraysTemp = ConcatByteArrays(playerSkillListArraysTemp, playerSkillListArray);
            	playerBuffListArraysTemp = ConcatByteArrays(playerBuffListArraysTemp, playerBuffListArray);
            	deleteItemSerialsArraysTemp = ConcatByteArrays(deleteItemSerialsArraysTemp, deleteItemSerialsArray);
            	deleteMobileSerialsArraysTemp = ConcatByteArrays(deleteMobileSerialsArraysTemp, deleteMobileSerialsArray);
        	}

        	// ##################################################################################
        	playerObjectArrayLengthList.Add((int) playerObjectArray.Length);
        	worldItemArrayLengthList.Add((int) worldItemArray.Length);
        	worldMobileArrayLengthList.Add((int) worldMobileArray.Length);
			popupMenuArrayLengthList.Add((int) popupMenuArray.Length);
			clilocArrayLengthList.Add((int) clilocArray.Length);
			vendorListArrayLengthList.Add((int) vendorListArray.Length);
        	playerStatusArrayLengthList.Add((int) playerStatusArray.Length);
        	playerSkillListArrayLengthList.Add((int) playerSkillListArray.Length);
        	playerBuffListArrayLengthList.Add((int) playerBuffListArray.Length);
        	deleteItemSerialsArrayLengthList.Add((int) deleteItemSerialsArray.Length);
        	deleteMobileSerialsArrayLengthList.Add((int) deleteMobileSerialsArray.Length);
			
        	// ##################################################################################
        	_envStep++;

        	grpcPlayerObject = new GrpcPlayerObject();
	    	worldItemObjectList.Clear();
	    	worldMobileObjectList.Clear();
	    	grpcPopupMenuList.Clear();
	        grpcClilocList.Clear();
	        grpcVendorList.Clear();
	        grpcPlayerSkillList.Clear();
	        grpcPlayerStatus = new GrpcPlayerStatus();
	        grpcPlayerBuffList.Clear();
	        _usedLandIndex = 0;
	        grpcDeleteItemSerials.Clear();
	        grpcDeleteMobileSerials.Clear();
	        grpcMenuControlList.Clear();
	        _gumpLocalSerial = 0;
	        _gumpServerSerial = 0;
	        _gumpWidth = 0;
        	_gumpHeight = 0;

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
		    	//Console.WriteLine("Tick:{0}, Type:{1}, SourceSerial:{2}, TargetSerial:{3}, Index:{4}, Amount:{5}, Direction:{6}, Run:{7}", 
		    	//	_controller._gameTick, grpcAction.ActionType, grpcAction.SourceSerial, grpcAction.TargetSerial, 
		    	//	 grpcAction.Index, grpcAction.Amount, grpcAction.WalkDirection, grpcAction.Run);
		    }

		    if (grpcAction.ActionType == 0)
		    {
			    //grpcAction = new GrpcAction();
			}

			//Console.WriteLine("grpcAction.ActionType: {0}", grpcAction.ActionType);

			////grpcAction.ActionType = 1;
		    byte[] actionArray = grpcAction.ToByteArray();
		    //Console.WriteLine("actionArray.Length: {0}", actionArray.Length);

        	if (_envStep == 0) 
        	{
        		actionArraysTemp = actionArray;
        	}
        	else if (_envStep == 1001) 
        	{	
            	// ##################################################################################
        		actionArrays = actionArraysTemp;

				// ##################################################################################
				actionArraysTemp = actionArray;
        	}
        	else if ( (_envStep % 1001 == 0) && (_envStep != 1001 * _totalStepScale) )
        	{
				// ##################################################################################
        		actionArrays = ConcatByteArrays(actionArrays, actionArraysTemp);

				// ##################################################################################
            	actionArraysTemp = actionArray;
        	}
        	else if (_envStep == 1001 * _totalStepScale)
        	{
        		// ##################################################################################
        		actionArrays = ConcatByteArrays(actionArrays, actionArraysTemp);

        		// ##################################################################################
        		//Console.WriteLine("actionArray reset / _envStep: {0}", _envStep);
        		if (Settings.Replay == true)
	            {
	            	//Console.WriteLine("obs reset / _envStep: {0}", _envStep);
	                SaveReplayFile();
	        		CreateMpqFile();
	        		Reset();
	            }
	            else
	            {
	            	//Console.WriteLine("obs reset / _envStep: {0}", _envStep);
	            	Reset();
	            }

        		actionArraysLengthList.Clear();
		        Array.Clear(actionArrays, 0, actionArrays.Length);
		        Array.Clear(actionArraysTemp, 0, actionArraysTemp.Length);
        	}
        	else
        	{
        		actionArraysTemp = ConcatByteArrays(actionArraysTemp, actionArray);
        	}

        	//Console.WriteLine("actionArray.Length: {0}", actionArray.Length);

        	// ##################################################################################
        	actionArraysLengthList.Add((int) actionArray.Length);

	        grpcAction = new GrpcAction();
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(GrpcAction grpcAction, ServerCallContext context)
        {
        	//Console.WriteLine("WriteAct() _envStep: {0}", _envStep);

    		if (grpcAction.ActionType == 0) {
    			// Do nothing
            	if (World.InGame == true) {
            	}
	        }
            else if (grpcAction.ActionType == 1) {
            	// Walk to Direction
            	if (World.InGame == true) {
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
	        			GameActions.PickUp(grpcAction.TargetSerial, 0, 0, (int) grpcAction.Amount);
					}
	        		catch (Exception ex)
		            {
		            	//Console.WriteLine("Failed to parse the item info: " + ex.Message);
		            }
	        	}
	        }
	        else if (grpcAction.ActionType == 4) {
	        	// Drop the holded item
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 4");

	        		if (grpcAction.TargetSerial != 0)
	        		{
	        			//Console.WriteLine("grpcAction.TargetSerial: {0}", grpcAction.TargetSerial);
	        			if (grpcAction.Index == 0)
	        			{
	        				// Drop without selecting the position of the container
        					GameActions.DropItem((uint) ItemHold.Serial, 0xFFFF, 0xFFFF, 0, grpcAction.TargetSerial);
        				}
        				else if (grpcAction.Index == 1)
        				{
        					// Drop at the certain position at the container(Item will not be stacked)
        					if (SerialHelper.IsItem(grpcAction.TargetSerial))
        					{
        						Item containerItem = World.Items.Get(grpcAction.TargetSerial);
	        					World.OPL.TryGetNameAndData(grpcAction.TargetSerial, out string name, out string data);
            					//Console.WriteLine("containerItem / Graphic: {0}, Name: {1}", containerItem.Graphic, name);

            					int x = 0; 
            					int y = 0;
            					if (containerItem.Graphic == 3701)
            					{
            						// 1. Backpack
	            					//containerItem.Graphic: 3701, Name: Backpack
									//containerBounds X: 44, Y: 65, Width: 186, Height: 159
            						x = 100;
            						y = 100;
            					}
            					else if (containerItem.Graphic == 3702)
            					{
            						Console.WriteLine("containerItem.Graphic == 3702");
            						// 2. Bag
		            				//containerItem / Graphic: 3702, Name: Bag
									//containerBounds X: 29, Y: 34, Width: 137, Height: 128
									x = 60;
            						y = 60;
            					}
            					else if (containerItem.Graphic == 2475)
            					{
            						// 3. Metal Chest
									//containerItem / Graphic: 2475, Name: Metal Chest
									//containerBounds X: 18, Y: 105, Width: 162, Height: 178
									x = 50;
            						y = 120;
            					}

            					GameActions.DropItem((uint) ItemHold.Serial, x, y, 0, grpcAction.TargetSerial);
        					}
        				}
        			}
        			else if (grpcAction.TargetSerial == 0)
        			{
        				// Drop the holded item on the selected land
        				Console.WriteLine("grpcAction.Index: {0}", grpcAction.Index);

		        		uint index = grpcAction.Index;
		        		try
		        		{   
		        			Vector2 selected = GetLandPosition(index);

		        			GameActions.DropItem((uint) ItemHold.Serial, (int) selected.X, (int) selected.Y, 0, 0xFFFF_FFFF);
		        		}
		        		catch (Exception ex)
			            {
			            	Console.WriteLine("Failed to fine the item dropable land: " + ex.Message);
			            }
        			}
	        	}
	        }
	        else if (grpcAction.ActionType == 5) {
	        	// Use the activated skill to the target
	        	if (World.Player != null) 
	        	{
	        		Console.WriteLine("ActionType == 5");
	        		//Console.WriteLine("grpcAction.Index: {0}", grpcAction.Index);

	        		if (grpcAction.TargetSerial != 0)
        			{
        				// Use the abililty to the entity target
		        		TargetManager.Target(grpcAction.TargetSerial);
        			}
	        		else if (grpcAction.TargetSerial == 0)
	        		{ 
		        		// Use the abililty to the land target
		        		Vector2 targetPosition = GetLandPosition(grpcAction.Index);
		        		GameObject targetObject = World.Map.GetTile((int) targetPosition.X, (int) targetPosition.Y);
		        		Land targetLand = (Land) targetObject;

		        		TargetManager.Target
	                    (
	                        0, targetLand.X, targetLand.Y, targetLand.Z, targetLand.TileData.IsWet
	                    );
	                }
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
	        	// Change the war mode
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 7");
	        		bool boolIndex = Convert.ToBoolean(grpcAction.Index);

	        		//Console.WriteLine("boolIndex: {0}", boolIndex);
        			NetClient.Socket.Send_ChangeWarMode(boolIndex);
        			World.Player.InWarMode = boolIndex;

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
	        	if (World.Player != null) {
	        		// Send Gump Response
	        		Console.WriteLine("ActionType == 9");

	        		List<uint> switchesList = new List<uint>();
                	List<Tuple<ushort, string>> entriesList = new List<Tuple<ushort, string>>();

                	uint[] switchesArray = switchesList.ToArray();
                	Tuple<ushort, string>[] entriesArray = entriesList.ToArray();

                	//Console.WriteLine("SourceSerial: {0}, TargetSerial: {1}, Index: {2}", 
                	//					grpcAction.SourceSerial, grpcAction.TargetSerial, grpcAction.Index);
                	try
	        		{
	        			// Check the target gump exists
	        			for (LinkedListNode<Gump> last = UIManager.Gumps.Last; last != null; last = last.Previous)
			            {
				            Control g = last.Value;
				            if (g.LocalSerial != 0)
				            {
				            	//Console.WriteLine("g.LocalSerial: {0}, g.ServerSerial: {1}, SourceSerial: {2}, TargetSerial: {3}, Index: {4}", 
				            	//	g.LocalSerial, g.ServerSerial, grpcAction.SourceSerial, grpcAction.TargetSerial, grpcAction.Index);
				            	if ( (g.LocalSerial == grpcAction.SourceSerial) && (g.ServerSerial == grpcAction.TargetSerial) )
				            	{
				            		// Check the target button is inside of target gump
				            		foreach (Control control in g.Children)
					                {
					                	if (control is Button)
					                	{
					                		Button button = (Button) control;
					                    	if (button.ButtonID == grpcAction.Index)
					                    	{
					                    		//Console.WriteLine("Socket.Send_GumpResponse()");
					                    		Socket.Send_GumpResponse(grpcAction.SourceSerial, grpcAction.TargetSerial, 
				            								(int) grpcAction.Index, switchesArray, entriesArray);
					                    	}
					                    }
					                }
				            	}
				            	else
				            	{
				            		//Console.WriteLine("Gump is closed");
				            	}
				            }
			            }
					}
	        		catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to send the gump response: " + ex.Message);
		            }

		            Console.WriteLine("");
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
	        		// Select one of menu from the pop up menu
	        		Console.WriteLine("ActionType == 11");

	        		GameActions.ResponsePopupMenu(grpcAction.TargetSerial, (ushort) grpcAction.Index);
	        	}
	        }
	        else if (grpcAction.ActionType == 12) {
	        	if (World.Player != null) {
	        		// Buy the item from vendor by selecting the item from shop gump
	        		Console.WriteLine("ActionType == 12");

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint) grpcAction.SourceSerial, (ushort) grpcAction.Amount);
	        		NetClient.Socket.Send_BuyRequest(grpcAction.TargetSerial, items);
	        	}
	        }
	        else if (grpcAction.ActionType == 13) {
	        	if (World.Player != null) {
	        		// Sell the item to vendor by selecting the item from shop gump
	        		Console.WriteLine("ActionType == 13");

	        		Tuple<uint, ushort>[] items = new Tuple<uint, ushort>[1];
	        		items[0] = new Tuple<uint, ushort>((uint) grpcAction.SourceSerial, (ushort) grpcAction.Amount);
	        		NetClient.Socket.Send_SellRequest(grpcAction.TargetSerial, items);
	        	}
	        }
	        else if (grpcAction.ActionType == 14) {
	        	if (World.Player != null) {
	        		// Use the item to target
	        		Console.WriteLine("ActionType == 14");
	        		//Item bandage = World.Player.FindBandage();
	        		//if (bandage != null) 
	        		{
	        			Console.WriteLine("SourceSerial: {0}, TargetSerial: {1}", grpcAction.SourceSerial, 
	        																      grpcAction.TargetSerial);
	        			NetClient.Socket.Send_TargetSelectedObject((uint) grpcAction.SourceSerial, grpcAction.TargetSerial);
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
	        		Console.WriteLine("ActionType == 16");
	        	}
	        }
	        else if (grpcAction.ActionType == 17) {
	        	if (World.Player != null) {
	        		Console.WriteLine("ActionType == 17");
	        	}
	        }

    		grpcPlayerObject = new GrpcPlayerObject();
    		worldItemObjectList.Clear();
	    	worldMobileObjectList.Clear();
	    	grpcPopupMenuList.Clear();
	        grpcClilocList.Clear();
	        grpcVendorList.Clear();
	        grpcPlayerSkillList.Clear();
	        grpcPlayerStatus = new GrpcPlayerStatus();
	        grpcPlayerBuffList.Clear();
	        _usedLandIndex = 0;
	        grpcDeleteItemSerials.Clear();
	        grpcDeleteMobileSerials.Clear();
	        grpcMenuControlList.Clear();
	        _gumpLocalSerial = 0;
	        _gumpServerSerial = 0;
	        _gumpWidth = 0;
        	_gumpHeight = 0;

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