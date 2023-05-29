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

        int _replayLength;
    	int _replayStep;
    	string _replayName;

    	// ###############
    	int _mobileDataArrayOffset = 0;
    	//int _worldItemArrayOffset = 0;
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
    	int _itemDropableLandArrayOffset = 0;
    	int _vendorItemObjectArrayOffset = 0;

    	int _playerStatusArrayOffset = 0;

    	// ###############
        byte[] mobileDataArrayLengthArrRead;
        //byte[] worldItemArrayLengthArrRead;
        byte[] equippedItemArrayLengthArrRead;
        byte[] backpackItemArrayLengthArrRead;
        byte[] corpseItemArrayLengthArrRead;
        byte[] popupMenuArrayLengthArrRead;
        byte[] clilocDataArrayLengthArrRead;

        byte[] playerMobileObjectArrayLengthArrRead;
    	byte[] mobileObjectArrayLengthArrRead;
    	byte[] itemObjectArrayLengthArrRead;
    	byte[] itemDropableLandArrayLengthArrRead;
    	byte[] vendorItemObjectArrayLengthArrRead;

    	byte[] playerStatusArrayLengthArrRead;

    	// ###############
		byte[] mobileDataArrRead;
		//byte[] worldItemArrRead;
		byte[] equippedItemArrRead;
		byte[] backpackItemArrRead;
		byte[] corpseItemArrRead;
		byte[] popupMenuArrRead;
		byte[] clilocDataArrRead;

        byte[] playerMobileObjectArrRead;
    	byte[] mobileObjectArrRead;
    	byte[] itemObjectArrRead;
    	byte[] itemDropableLandArrRead;
    	byte[] vendorItemObjectArrRead;

    	// ###############
    	byte[] actionTypeArrRead;
    	byte[] walkDirectionArrRead;

    	// ###############
    	public GrpcMobileList grpcMobileDataReplay;
    	//public GrpcItemList grpcWorldItemReplay;
    	public GrpcItemList grpcEquippedItemReplay;
    	public GrpcItemList grpcBackpackItemReplay;
    	public GrpcItemList grpcCorpseItemReplay;
    	public GrpcPopupMenuList grpcPopupMenuReplay;
    	public GrpcClilocDataList grpcClilocDataReplay;
    	public PlayerStatus grpcPlayerStatusReplay;

    	// ###############
    	public GrpcGameObjectList grpcPlayerMobileObjectReplay;
    	public GrpcGameObjectList grpcMobileObjectReplay;
    	public GrpcGameObjectList grpcItemObjectReplay;
    	public GrpcGameObjectSimpleList grpcItemDropableLandReplay;
    	public GrpcGameObjectList grpcVendorItemObjectReplay;

    	// ###############
        List<int> mobileDataArrayLengthListRead;
        //List<int> worldItemArrayLengthListRead;
        List<int> equippedItemArrayLengthListRead;
        List<int> backpackItemArrayLengthListRead;
        List<int> corpseItemArrayLengthListRead;
        List<int> popupMenuArrayLengthListRead;
        List<int> clilocDataArrayLengthListRead;

        List<int> playerMobileObjectArrayLengthListRead;
        List<int> mobileObjectArrayLengthListRead;
        List<int> itemObjectArrayLengthListRead;
        List<int> itemDropableLandArrayLengthListRead;
        //List<int> vendorItemObjectArrayLengthListRead;

        //List<int> playerStatusArrayLengthListRead;

    	public UoServiceReplayImpl(int port)
        {
            _port = port;

            _grpcServer = new Server
	        {
	            Services = { UoService.BindService(this) },
	            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
	        };

    		_replayStep = 0;
        }

        public void Start() 
        {
        	_grpcServer.Start();
        }

        public override Task<Empty> ReadMPQFile(Config config, ServerCallContext context)
        {
            Console.WriteLine("ReadMPQFile()");

            string folderPath = config.Name;
            Console.WriteLine("folderPath: {0}", folderPath);

			string[] files = Directory.GetFiles(folderPath);
			Random random = new Random();
			int randomIndex = random.Next(files.Length);

			string randomFilePath = files[randomIndex];

			// Use the randomly selected file path as desired
			Console.WriteLine("Randomly selected file: {0}", randomFilePath);

            //_replayName = config.Name;
            _replayName = randomFilePath;
            Console.WriteLine("_replayName: {0}" + _replayName);
            
            try 
        	{
	            // ###############
	            mobileDataArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.mobileDataLen");
	            //worldItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.worldItemLen");
	            equippedItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.equippedItemLen");
	            backpackItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.backpackitemLen");
	            corpseItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.corpseItemLen");
	            popupMenuArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.popupMenuLen");
	            clilocDataArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.clilocDataLen");

	            playerMobileObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.playerMobileObjectLen");
	            mobileObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.mobileObjectLen");
	            itemObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.itemObjectLen");
	            itemDropableLandArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.itemDropableLandSimpleLen");
	            //vendorItemObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.vendorItemObjectLen");

	            playerStatusArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.playerStatusLen");

	            // ###############
		    	mobileDataArrRead = ReadFromMpqArchive(_replayName, "replay.data.mobileData");
				//worldItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.worldItem");
				equippedItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.equippedItem");
				backpackItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.backpackItem");
				corpseItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.corpseItem");
				popupMenuArrRead = ReadFromMpqArchive(_replayName, "replay.data.popupMenu");
				clilocDataArrRead = ReadFromMpqArchive(_replayName, "replay.data.clilocData");

		        playerMobileObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.playerMobileObject");
	            mobileObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.mobileObject");
	            itemObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.itemObject");
	            itemDropableLandArrRead = ReadFromMpqArchive(_replayName, "replay.data.itemDropableLandSimple");
	            //vendorItemObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.vendorItemObject");

	            // ###############
	        	actionTypeArrRead = ReadFromMpqArchive(_replayName, "replay.data.type");
				walkDirectionArrRead = ReadFromMpqArchive(_replayName, "replay.data.walkDirection");

				// ###############
				mobileDataArrayLengthListRead = ConvertByteArrayToIntList(mobileDataArrayLengthArrRead);
		        //worldItemArrayLengthListRead = ConvertByteArrayToIntList(worldItemArrayLengthArrRead);
		        equippedItemArrayLengthListRead = ConvertByteArrayToIntList(equippedItemArrayLengthArrRead);
		        backpackItemArrayLengthListRead = ConvertByteArrayToIntList(backpackItemArrayLengthArrRead);
		        corpseItemArrayLengthListRead = ConvertByteArrayToIntList(corpseItemArrayLengthArrRead);
		        popupMenuArrayLengthListRead = ConvertByteArrayToIntList(popupMenuArrayLengthArrRead);
		        clilocDataArrayLengthListRead = ConvertByteArrayToIntList(clilocDataArrayLengthArrRead);

		        playerMobileObjectArrayLengthListRead = ConvertByteArrayToIntList(playerMobileObjectArrayLengthArrRead);
		        mobileObjectArrayLengthListRead = ConvertByteArrayToIntList(mobileObjectArrayLengthArrRead);
		        itemObjectArrayLengthListRead = ConvertByteArrayToIntList(itemObjectArrayLengthArrRead);
		        itemDropableLandArrayLengthListRead = ConvertByteArrayToIntList(itemDropableLandArrayLengthArrRead);
		        //vendorItemObjectArrayLengthListRead = ConvertByteArrayToIntList(vendorItemObjectArrayLengthArrRead);

		        //playerStatusArrayLengthListRead = ConvertByteArrayToIntList(playerStatusArrayLengthArrRead);
		        _replayLength = mobileDataArrayLengthListRead.Count;
			}
			catch (Exception ex)
            {
                Console.WriteLine("Failed to read from the MPQ file: " + ex.Message);
            }

            return Task.FromResult(new Empty {});
        }

        public byte[] GetSubsetArray(int index, List<int> lengthListRead, ref int offset, byte[] arrRead) 
        {	
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
        	//Console.WriteLine("ReadReplay()");
        	Console.WriteLine("_replayStep: {0}", _replayStep);

        	/*
        	if (_replayStep % 1000 == 0)
        	{
        		_mobileDataArrayOffset = 0;
		    	_worldItemArrayOffset = 0;
		    	_equippedItemArrayOffset = 0;
		    	_backpackItemArrayOffset = 0;
		    	_corpseItemArrayOffset = 0;
		    	_popupMenuArrayOffset = 0;
		    	_clilocDataArrayOffset = 0;

		    	//_landObjectArrayOffset = 0;
		    	_playerMobileObjectArrayOffset = 0;
		    	_mobileObjectArrayOffset = 0;
		    	_itemObjectArrayOffset = 0;
		    	//_staticObjectArrayOffset = 0;
		    	_itemDropableLandArrayOffset = 0;
		    	//_vendorItemObjectArrayOffset = 0;

		    	_playerStatusArrayOffset = 0;
        	}
        	*/
        	
        	int index = _replayStep;

            // ###############
			byte[] mobileDataSubsetArray = GetSubsetArray(index, mobileDataArrayLengthListRead, ref _mobileDataArrayOffset, mobileDataArrRead);
			//byte[] worldItemSubsetArray = GetSubsetArray(index, worldItemArrayLengthListRead, ref _worldItemArrayOffset, worldItemArrRead);
			byte[] equippedItemSubsetArray = GetSubsetArray(index, equippedItemArrayLengthListRead, ref _equippedItemArrayOffset, equippedItemArrRead);
			byte[] backpackItemSubsetArray = GetSubsetArray(index, backpackItemArrayLengthListRead, ref _backpackItemArrayOffset, backpackItemArrRead);
			//byte[] corpseItemSubsetArray = GetSubsetArray(index, corpseItemArrayLengthListRead, ref _corpseItemArrayOffset, corpseItemArrRead);
			//byte[] popupMenuSubsetArray = GetSubsetArray(index, popupMenuArrayLengthListRead, ref _popupMenuArrayOffset, popupMenuArrRead);
			byte[] clilocDataSubsetArray = GetSubsetArray(index, clilocDataArrayLengthListRead, ref _clilocDataArrayOffset, clilocDataArrRead);

            byte[] playerMobileObjectSubsetArray = GetSubsetArray(index, playerMobileObjectArrayLengthListRead, ref _playerMobileObjectArrayOffset, playerMobileObjectArrRead);
            byte[] mobileObjectSubsetArray = GetSubsetArray(index, mobileObjectArrayLengthListRead, ref _mobileObjectArrayOffset, mobileObjectArrRead);
            byte[] itemObjectSubsetArray = GetSubsetArray(index, itemObjectArrayLengthListRead, ref _itemObjectArrayOffset, itemObjectArrRead);
            byte[] itemDropableLandSubsetArray = GetSubsetArray(index, itemDropableLandArrayLengthListRead, ref _itemDropableLandArrayOffset, itemDropableLandArrRead);
            //byte[] vendorItemObjectSubsetArray = GetSubsetArray(index, vendorItemObjectArrayLengthListRead, ref _vendorItemObjectArrayOffset, vendorItemObjectArrRead);
			
            try 
            {
            	// ###############
            	grpcMobileDataReplay = GrpcMobileList.Parser.ParseFrom(mobileDataSubsetArray);
	    		//grpcWorldItemReplay = GrpcItemList.Parser.ParseFrom(worldItemSubsetArray);
		    	grpcEquippedItemReplay = GrpcItemList.Parser.ParseFrom(equippedItemSubsetArray);
		    	grpcBackpackItemReplay = GrpcItemList.Parser.ParseFrom(backpackItemSubsetArray);
		    	//grpcCorpseItemReplay = GrpcItemList.Parser.ParseFrom(corpseItemSubsetArray);
		    	//grpcPopupMenuReplay = GrpcPopupMenuList.Parser.ParseFrom(popupMenuSubsetArray);
		    	grpcClilocDataReplay = GrpcClilocDataList.Parser.ParseFrom(clilocDataSubsetArray);

		    	// ###############
            	grpcPlayerMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(playerMobileObjectSubsetArray);
            	grpcMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(mobileObjectSubsetArray);
            	grpcItemObjectReplay = GrpcGameObjectList.Parser.ParseFrom(itemObjectSubsetArray);
            	grpcItemDropableLandReplay = GrpcGameObjectSimpleList.Parser.ParseFrom(itemDropableLandSubsetArray);
            	//grpcVendorItemObjectReplay = GrpcGameObjectList.Parser.ParseFrom(vendorItemObjectSubsetArray);
            	
            }
            catch (Exception ex)
            {
            	//Console.WriteLine("Failed to parser the GrpcMobileList from Byte array: " + ex.Message);
            }

            List<int> actionTypeList = ConvertByteArrayToIntList(actionTypeArrRead);
            List<int> walkDirectionList = ConvertByteArrayToIntList(walkDirectionArrRead);

			// ###############
	        States states = new States();

            states.MobileList = grpcMobileDataReplay;
            //states.WorldItemList = grpcWorldItemReplay;
            states.EquippedItemList = grpcEquippedItemReplay;
            states.BackpackItemList = grpcBackpackItemReplay;
            //states.CorpseItemList = grpcCorpseItemReplay;
            //states.PopupMenuList = grpcPopupMenuReplay;
            states.ClilocDataList = grpcClilocDataReplay;

            states.PlayerMobileObjectList = grpcPlayerMobileObjectReplay;
            states.MobileObjectList = grpcMobileObjectReplay;
            states.ItemObjectList = grpcItemObjectReplay;
            states.ItemDropableLandList = grpcItemDropableLandReplay;
            //states.VendorItemObjectList = grpcVendorItemObjectReplay;
			
            _replayStep++;

            if (_replayStep >= _replayLength) {
            	Console.WriteLine("_replayStep >= _replayLength");
            }
           
            return Task.FromResult(states);
        }
    }
}