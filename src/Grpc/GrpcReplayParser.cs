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
	internal partial class UoServiceReplayImpl : UoService.UoServiceBase
    {
    	int _port;
        Server _grpcServer;

    	int _replayStep = 0;
    	string _replayName;

    	// ###############
    	int _mobileDataArrayOffset = 0;
    	int _worldItemArrayOffset = 0;
    	int _equippedItemArrayOffset = 0;
    	int _backpackItemArrayOffset = 0;
    	int _corpseItemArrayOffset = 0;
    	int _popupMenuArrayOffset = 0;
    	int _clilocDataArrayOffset = 0;

    	int _landObjectArrayOffset = 0;
    	int _playerMobileObjectArrayOffset = 0;
    	int _mobileObjectArrayOffset = 0;
    	int _itemObjectArrayOffset = 0;
    	int _staticObjectArrayOffset = 0;
    	int _itemDropableLandObjectArrayOffset = 0;
    	int _vendorItemObjectArrayOffset = 0;

    	int _playerStatusArrayOffset = 0;

    	// ###############
        byte[] mobileDataArrayLengthArrRead;
        byte[] worldItemArrayLengthArrRead;
        byte[] equippedItemArrayLengthArrRead;
        byte[] backpackItemArrayLengthArrRead;
        byte[] corpseItemArrayLengthArrRead;
        byte[] popupMenuArrayLengthArrRead;
        byte[] clilocDataArrayLengthArrRead;

        byte[] landObjectArrayLengthArrRead;
        byte[] playerMobileObjectArrayLengthArrRead;
    	byte[] mobileObjectArrayLengthArrRead;
    	byte[] itemObjectArrayLengthArrRead;
    	byte[] staticObjectArrayLengthArrRead;
    	byte[] itemDropableLandObjectArrayLengthArrRead;
    	byte[] vendorItemObjectArrayLengthArrRead;

    	byte[] playerStatusArrayLengthArrRead;

    	// ###############
		byte[] mobileDataArrRead;
		byte[] worldItemArrRead;
		byte[] equippedItemArrRead;
		byte[] backpackItemArrRead;
		byte[] corpseItemArrRead;
		byte[] popupMenuArrRead;
		byte[] clilocDataArrRead;

        byte[] landObjectArrRead;
        byte[] playerMobileObjectArrRead;
    	byte[] mobileObjectArrRead;
    	byte[] itemObjectArrRead;
    	byte[] staticObjectArrRead;
    	byte[] itemDropableLandObjectArrRead;
    	byte[] vendorItemObjectArrRead;

    	// ###############
    	byte[] actionTypeArrRead;
    	byte[] walkDirectionArrRead;

    	// ###############
    	public GrpcMobileList grpcMobileDataReplay;
    	public GrpcItemList grpcWorldItemReplay;
    	public GrpcItemList grpcEquippedItemReplay;
    	public GrpcItemList grpcBackpackItemReplay;
    	public GrpcItemList grpcCorpseItemReplay;
    	public GrpcPopupMenuList grpcPopupMenuReplay;
    	public GrpcClilocDataList grpcClilocDataReplay;
    	public PlayerStatus grpcPlayerStatusReplay;

    	// ###############
    	public GrpcGameObjectList grpcLandObjectReplay;
    	public GrpcGameObjectList grpcPlayerMobileObjectReplay;
    	public GrpcGameObjectList grpcMobileObjectReplay;
    	public GrpcGameObjectList grpcItemObjectReplay;
    	public GrpcGameObjectList grpcStaticObjectReplay;
    	public GrpcGameObjectList grpcItemDropableLandObjectReplay;
    	public GrpcGameObjectList grpcVendorItemObjectReplay;

    	public UoServiceReplayImpl(int port)
        {
            _port = port;

            _grpcServer = new Server
	        {
	            Services = { UoService.BindService(this) },
	            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
	        };
        }

        public void Start() 
        {
        	_grpcServer.Start();
        }

        public override Task<Empty> ReadMPQFile(Config config, ServerCallContext context)
        {
            Console.WriteLine("ReadMPQFile()");

            Console.WriteLine("config.Name: {0}", config.Name);
            _replayName = config.Name;
            
            try 
        	{
	            // ###############
	            mobileDataArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.mobileDataLen");
	            /*
	            worldItemArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.worldItem.length");
	            equippedItemArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.equippedItem.length");
	            backpackItemArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.backpackitem.length");
	            corpseItemArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.corpseItem.length");
	            popupMenuArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.popupMenu.length");
	            clilocDataArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.clilocData.length");

	            landObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.landObject.length");
	            playerMobileObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerMobileObject.length");
	            mobileObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.mobileObject.length");
	            itemObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.itemObject.length");
	            staticObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.staticObject.length");
	            itemDropableLandObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.itemDropableLandObject.length");
	            vendorItemObjectArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.vendorItemObject.length");

	            playerStatusArrayLengthArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.metadata.playerStatus.length");

	            // ###############
		    	mobileDataArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.mobileData");
				worldItemArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.worldItem");
				equippedItemArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.equippedItem");
				backpackItemArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.backpackItem");
				corpseItemArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.corpseItem");
				popupMenuArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.popupMenu");
				clilocDataArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.data.clilocData");

		        landObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.landObject");
		        playerMobileObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.playerMobileObject");
	            mobileObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.mobileObject");
	            itemObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.itemObject");
	            staticObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.staticObject");
	            itemDropableLandObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.itemDropableLandObject");
	            vendorItemObjectArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.object.vendorItemObject");
				*/
	            // ###############
	        	//mobileObjectLengthArrRead = ReadFromMpqArchive("Replay/" + replayName + ".uoreplay", "replay.metadata.length");
	        	//mobileObjectArrRead = ReadFromMpqArchive("Replay/" + replayName + ".uoreplay", "replay.object.mobile");

	        	//actionTypeArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.actionType");
				//walkDirectionArrRead = ReadFromMpqArchive("Replay/" + _replayName + ".uoreplay", "replay.walkDirection");
			}
			catch (Exception ex)
            {
                Console.WriteLine("Failed to read from the MPQ file: " + ex.Message);
            }

            return Task.FromResult(new Empty {});
        }

        public byte[] GetSubsetArray(int index, List<int> lengthListRead, ref int offset, byte[] arrRead) 
        {	
        	/*
        	int item = mobileObjectArrayLengthListRead[index];
        	int startIndex = _arrayOffset; 
        	int length = _arrayOffset + item; 
        	byte[] subsetArray = new byte[item];
			Array.Copy(mobileObjectArrRead, startIndex, subsetArray, 0, item);
            _arrayOffset += item;
			*/

        	int item = lengthListRead[index];
        	int startIndex = offset; 
        	int length = offset + item; 

        	byte[] subsetArray = new byte[item];

			Array.Copy(arrRead, startIndex, subsetArray, 0, item);

            offset += item;

            return subsetArray;
        }

    	// Server side handler of the SayHello RPC
        public override Task<States> ReadReplay(Config config, ServerCallContext context)
        {
        	Console.WriteLine("ReadReplay()");

        	if (_replayStep % 1000 == 0)
        	{
        		_mobileDataArrayOffset = 0;
		    	_worldItemArrayOffset = 0;
		    	_equippedItemArrayOffset = 0;
		    	_backpackItemArrayOffset = 0;
		    	_corpseItemArrayOffset = 0;
		    	_popupMenuArrayOffset = 0;
		    	_clilocDataArrayOffset = 0;

		    	_landObjectArrayOffset = 0;
		    	_playerMobileObjectArrayOffset = 0;
		    	_mobileObjectArrayOffset = 0;
		    	_itemObjectArrayOffset = 0;
		    	_staticObjectArrayOffset = 0;
		    	_itemDropableLandObjectArrayOffset = 0;
		    	_vendorItemObjectArrayOffset = 0;

		    	_playerStatusArrayOffset = 0;
        	}
        	
        	int index = _replayStep % 1000;

        	List<int> mobileDataArrayLengthListRead = ConvertByteArrayToIntList(mobileDataArrayLengthArrRead);

        	foreach (int len in mobileDataArrayLengthListRead)
            { 
            	Console.WriteLine("len: {0}", len);
            }

            Console.WriteLine("\n");
        	/*
        	// ###############
            List<int> mobileDataArrayLengthListRead = ConvertByteArrayToIntList(mobileDataArrayLengthArrRead);
            List<int> worldItemArrayLengthListRead = ConvertByteArrayToIntList(worldItemArrayLengthArrRead);
            List<int> equippedItemArrayLengthListRead = ConvertByteArrayToIntList(equippedItemArrayLengthArrRead);
            List<int> backpackItemArrayLengthListRead = ConvertByteArrayToIntList(backpackItemArrayLengthArrRead);
            List<int> corpseItemArrayLengthListRead = ConvertByteArrayToIntList(corpseItemArrayLengthArrRead);
            List<int> popupMenuArrayLengthListRead = ConvertByteArrayToIntList(popupMenuArrayLengthArrRead);
            List<int> clilocDataArrayLengthListRead = ConvertByteArrayToIntList(clilocDataArrayLengthArrRead);

            List<int> landObjectArrayLengthListRead = ConvertByteArrayToIntList(landObjectArrayLengthArrRead);
            List<int> playerMobileObjectArrayLengthListRead = ConvertByteArrayToIntList(playerMobileObjectArrayLengthArrRead);
            List<int> mobileObjectArrayLengthListRead = ConvertByteArrayToIntList(mobileObjectArrayLengthArrRead);
            List<int> itemObjectArrayLengthListRead = ConvertByteArrayToIntList(itemObjectArrayLengthArrRead);
            List<int> staticObjectArrayLengthListRead = ConvertByteArrayToIntList(staticObjectArrayLengthArrRead);
            List<int> itemDropableLandObjectArrayLengthListRead = ConvertByteArrayToIntList(itemDropableLandObjectArrayLengthArrRead);
            List<int> vendorItemObjectArrayLengthListRead = ConvertByteArrayToIntList(vendorItemObjectArrayLengthArrRead);

            List<int> playerStatusArrayLengthListRead = ConvertByteArrayToIntList(playerStatusArrayLengthArrRead);

            // ###############
			byte[] mobileDataSubsetArray = GetSubsetArray(index, mobileDataArrayLengthListRead, ref _mobileDataArrayOffset, mobileDataArrRead);
			byte[] worldItemSubsetArray = GetSubsetArray(index, worldItemArrayLengthListRead, ref _worldItemArrayOffset, worldItemArrRead);
			byte[] equippedItemSubsetArray = GetSubsetArray(index, equippedItemArrayLengthListRead, ref _equippedItemArrayOffset, equippedItemArrRead);
			byte[] backpackItemSubsetArray = GetSubsetArray(index, backpackItemArrayLengthListRead, ref _backpackItemArrayOffset, backpackItemArrRead);
			byte[] corpseItemSubsetArray = GetSubsetArray(index, corpseItemArrayLengthListRead, ref _corpseItemArrayOffset, corpseItemArrRead);
			byte[] popupMenuSubsetArray = GetSubsetArray(index, popupMenuArrayLengthListRead, ref _popupMenuArrayOffset, popupMenuArrRead);
			byte[] clilocDataSubsetArray = GetSubsetArray(index, clilocDataArrayLengthListRead, ref _clilocDataArrayOffset, clilocDataArrRead);

			// ###############
            byte[] mobileObjectSubsetArray = GetSubsetArray(index, mobileObjectArrayLengthListRead, ref _mobileObjectArrayOffset, mobileObjectArrRead);

            try 
            {
            	// ###############
            	grpcMobileDataReplay = GrpcMobileList.Parser.ParseFrom(mobileDataSubsetArray);
	    		grpcWorldItemReplay = GrpcItemList.Parser.ParseFrom(worldItemSubsetArray);
		    	grpcEquippedItemReplay = GrpcItemList.Parser.ParseFrom(equippedItemSubsetArray);
		    	grpcBackpackItemReplay = GrpcItemList.Parser.ParseFrom(backpackItemSubsetArray);
		    	grpcCorpseItemReplay = GrpcItemList.Parser.ParseFrom(corpseItemSubsetArray);
		    	grpcPopupMenuReplay = GrpcPopupMenuList.Parser.ParseFrom(popupMenuSubsetArray);
		    	grpcClilocDataReplay = GrpcClilocDataList.Parser.ParseFrom(clilocDataSubsetArray);

		    	// ###############
            	grpcMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(mobileObjectSubsetArray);
            }
            catch (Exception ex)
            {
            	//Console.WriteLine("Failed to parser the GrpcMobileList from Byte array: " + ex.Message);
            }

            List<int> actionTypeList = ConvertByteArrayToIntList(actionTypeArrRead);
            List<int> walkDirectionList = ConvertByteArrayToIntList(walkDirectionArrRead);

            //foreach (int action_type in actionTypeList)
	        //{
	        //    Console.WriteLine("action_type: {0}", action_type);
	        //}

	        //Console.WriteLine("\n");
			*/
	        States states = new States();

	        /*
            states.MobileList = grpcMobileDataReplay;
            states.WorldItemList = grpcWorldItemReplay;
            states.EquippedItemList = grpcEquippedItemReplay;
            states.BackpackItemList = grpcBackpackItemReplay;
            states.CorpseItemList = grpcCorpseItemReplay;
            states.PopupMenuList = grpcPopupMenuReplay;
            states.ClilocDataList = grpcClilocDataReplay;

	        states.MobileObjectList = grpcMobileObjectReplay;
			*/
            _replayStep++;
           
            return Task.FromResult(states);
        }
    }
}