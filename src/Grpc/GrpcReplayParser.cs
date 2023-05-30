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
    	string folderPath;

    	// ###############
    	int _mobileDataArrayOffset;
    	int _equippedItemArrayOffset;
    	int _backpackItemArrayOffset;
    	int _corpseItemArrayOffset;
    	int _popupMenuArrayOffset;
    	int _clilocDataArrayOffset;

    	int _playerMobileObjectArrayOffset;
    	int _mobileObjectArrayOffset;
    	int _itemObjectArrayOffset;
    	int _itemDropableLandArrayOffset;
    	int _vendorItemObjectArrayOffset;

    	int _playerStatusArrayOffset;

    	// ###############
        byte[] mobileDataArrayLengthArrRead;
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

        // ###############
        List<int> actionTypeList;
        List<int> walkDirectionList;

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

            folderPath = config.Name;
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
            	
            _replayStep = 0;
            _replayStep = 0;
    		_mobileDataArrayOffset = 0;
	    	_equippedItemArrayOffset = 0;
	    	_backpackItemArrayOffset = 0;
	    	_corpseItemArrayOffset = 0;
	    	_popupMenuArrayOffset = 0;
	    	_clilocDataArrayOffset = 0;

	    	_playerMobileObjectArrayOffset = 0;
	    	_mobileObjectArrayOffset = 0;
	    	_itemObjectArrayOffset = 0;
	    	//_staticObjectArrayOffset = 0;
	    	_itemDropableLandArrayOffset = 0;
	    	//_vendorItemObjectArrayOffset = 0;

            try 
        	{
	            // ###############
	            mobileDataArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.mobileDataLen");
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

		        int sum_length = 0;
				for (int i = 0; i < mobileObjectArrayLengthListRead.Count; i++)
		        {
		        	sum_length += mobileObjectArrayLengthListRead[i];
		        }

		        // ###############
		        actionTypeList = ConvertByteArrayToIntList(actionTypeArrRead);
            	walkDirectionList = ConvertByteArrayToIntList(walkDirectionArrRead);

				Console.WriteLine("actionTypeList.Count: {0}", actionTypeList.Count);
				Console.WriteLine("walkDirectionList.Count: {0}", walkDirectionList.Count);
			}
			catch (Exception ex)
            {
                Console.WriteLine("Failed to read from the MPQ file: " + ex.Message);
            }

            return Task.FromResult(new Empty {});
        }

        public void Reset() 
        {
        	Console.WriteLine("Reset()");

			string[] files = Directory.GetFiles(folderPath);
			Random random = new Random();
			int randomIndex = random.Next(files.Length);

			string randomFilePath = files[randomIndex];

			// Use the randomly selected file path as desired
			Console.WriteLine("Randomly selected file: {0}", randomFilePath);

            //_replayName = config.Name;
            _replayName = randomFilePath;
            Console.WriteLine("_replayName: {0}" + _replayName);
            	
            _replayStep = 0;

    		_mobileDataArrayOffset = 0;
	    	_equippedItemArrayOffset = 0;
	    	_backpackItemArrayOffset = 0;
	    	_corpseItemArrayOffset = 0;
	    	_popupMenuArrayOffset = 0;
	    	_clilocDataArrayOffset = 0;

	    	_playerMobileObjectArrayOffset = 0;
	    	_mobileObjectArrayOffset = 0;
	    	_itemObjectArrayOffset = 0;
	    	_itemDropableLandArrayOffset = 0;
	    	//_vendorItemObjectArrayOffset = 0;

	    	_playerStatusArrayOffset = 0;
            try 
        	{
	            // ###############
	            mobileDataArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.mobileDataLen");
	            //equippedItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.equippedItemLen");
	            //backpackItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.backpackitemLen");
	            //corpseItemArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.corpseItemLen");
	            //popupMenuArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.popupMenuLen");
	            //clilocDataArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.clilocDataLen");

	            playerMobileObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.playerMobileObjectLen");
	            mobileObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.mobileObjectLen");
	            //itemObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.itemObjectLen");
	            //itemDropableLandArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.itemDropableLandSimpleLen");
	            //vendorItemObjectArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.vendorItemObjectLen");

	            playerStatusArrayLengthArrRead = ReadFromMpqArchive(_replayName, "replay.metadata.playerStatusLen");

	            // ###############
		    	mobileDataArrRead = ReadFromMpqArchive(_replayName, "replay.data.mobileData");
				//equippedItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.equippedItem");
				//backpackItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.backpackItem");
				//corpseItemArrRead = ReadFromMpqArchive(_replayName, "replay.data.corpseItem");
				//popupMenuArrRead = ReadFromMpqArchive(_replayName, "replay.data.popupMenu");
				//clilocDataArrRead = ReadFromMpqArchive(_replayName, "replay.data.clilocData");

		        playerMobileObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.playerMobileObject");
	            //mobileObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.mobileObject");
	            //itemObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.itemObject");
	            //itemDropableLandArrRead = ReadFromMpqArchive(_replayName, "replay.data.itemDropableLandSimple");
	            //vendorItemObjectArrRead = ReadFromMpqArchive(_replayName, "replay.data.vendorItemObject");

	            // ###############
	        	actionTypeArrRead = ReadFromMpqArchive(_replayName, "replay.data.type");
				walkDirectionArrRead = ReadFromMpqArchive(_replayName, "replay.data.walkDirection");

				// ###############
				mobileDataArrayLengthListRead = ConvertByteArrayToIntList(mobileDataArrayLengthArrRead);
		        //equippedItemArrayLengthListRead = ConvertByteArrayToIntList(equippedItemArrayLengthArrRead);
		        //backpackItemArrayLengthListRead = ConvertByteArrayToIntList(backpackItemArrayLengthArrRead);
		        //corpseItemArrayLengthListRead = ConvertByteArrayToIntList(corpseItemArrayLengthArrRead);
		        //popupMenuArrayLengthListRead = ConvertByteArrayToIntList(popupMenuArrayLengthArrRead);
		        //clilocDataArrayLengthListRead = ConvertByteArrayToIntList(clilocDataArrayLengthArrRead);

		        playerMobileObjectArrayLengthListRead = ConvertByteArrayToIntList(playerMobileObjectArrayLengthArrRead);
		        //mobileObjectArrayLengthListRead = ConvertByteArrayToIntList(mobileObjectArrayLengthArrRead);
		        //itemObjectArrayLengthListRead = ConvertByteArrayToIntList(itemObjectArrayLengthArrRead);
		        //itemDropableLandArrayLengthListRead = ConvertByteArrayToIntList(itemDropableLandArrayLengthArrRead);
		        //vendorItemObjectArrayLengthListRead = ConvertByteArrayToIntList(vendorItemObjectArrayLengthArrRead);

		        //playerStatusArrayLengthListRead = ConvertByteArrayToIntList(playerStatusArrayLengthArrRead);
		        _replayLength = mobileDataArrayLengthListRead.Count;

		        int sum_length = 0;
				for (int i = 0; i < mobileObjectArrayLengthListRead.Count; i++)
		        {
		        	sum_length += mobileObjectArrayLengthListRead[i];
		        }

		        Console.WriteLine("sum_length: {0}", sum_length);
				Console.WriteLine("mobileObjectArrRead.Length: {0}", mobileObjectArrRead.Length);
			}
			catch (Exception ex)
            {
                Console.WriteLine("Failed to read from the MPQ file: " + ex.Message);
            }

        }

        public byte[] GetSubsetArray(int index, List<int> lengthListRead, ref int offset, byte[] arrRead) 
        {	
        	int item = lengthListRead[index];

        	int startIndex = offset; 
        	int length = offset + item; 

        	byte[] subsetArray = new byte[item];

        	Array.Copy(arrRead, startIndex, subsetArray, 0, item);

        	/*
        	try 
            {
				Array.Copy(arrRead, startIndex, subsetArray, 0, item);
			}
			catch (Exception ex)
            {
            	Console.WriteLine("Failed to parser the GetSubsetArray from original array 2: " + ex.Message);
            	//Console.WriteLine("arrRead.Length: {0}", arrRead.Length);
            	//Console.WriteLine("item: {0}", item);
            	//Console.WriteLine("startIndex: {0}", startIndex);
            	//Console.WriteLine("\n");
            }
            */

            offset += item;
            //Console.WriteLine("item: {0}, offset: {1}", item, offset);

            return subsetArray;
        }

    	// Server side handler of the SayHello RPC
        public override Task<States> ReadReplay(Config config, ServerCallContext context)
        {
        	//Console.WriteLine("ReadReplay()");
        	Console.WriteLine("_replayStep: {0}", _replayStep);

        	int index = _replayStep;

        	//Console.WriteLine("actionTypeList[{0}]: {1}", index, actionTypeList[index]);
        	//Console.WriteLine("walkDirectionList[{0}]: {1}", index, walkDirectionList[index]);
        	//Console.WriteLine("");

        	byte[] mobileDataSubsetArray = GetSubsetArray(index, mobileDataArrayLengthListRead, ref _mobileDataArrayOffset, mobileDataArrRead);
        	byte[] equippedItemSubsetArray = GetSubsetArray(index, equippedItemArrayLengthListRead, ref _equippedItemArrayOffset, equippedItemArrRead);
        	byte[] backpackItemSubsetArray = GetSubsetArray(index, backpackItemArrayLengthListRead, ref _backpackItemArrayOffset, backpackItemArrRead);
        	//byte[] clilocDataSubsetArray = GetSubsetArray(index, clilocDataArrayLengthListRead, ref _clilocDataArrayOffset, clilocDataArrRead);

			byte[] playerMobileObjectSubsetArray = GetSubsetArray(index, playerMobileObjectArrayLengthListRead, ref _playerMobileObjectArrayOffset, playerMobileObjectArrRead);
			byte[] mobileObjectSubsetArray = GetSubsetArray(index, mobileObjectArrayLengthListRead, ref _mobileObjectArrayOffset, mobileObjectArrRead);


			try
			{
				byte[] itemObjectSubsetArray = GetSubsetArray(index, itemObjectArrayLengthListRead, ref _itemObjectArrayOffset, itemObjectArrRead);
			}
            catch (Exception ex)
            {
            	Console.WriteLine("Failed to parser the GetSubsetArray from original array: " + ex.Message);
            	Console.WriteLine("itemObjectArrayLengthListRead[{0}]: {1}", index, itemObjectArrayLengthListRead[index]);
            	Console.WriteLine("itemObjectArrRead: {0}\n", itemObjectArrRead);
            }

			byte[] itemDropableLandSubsetArray = GetSubsetArray(index, itemDropableLandArrayLengthListRead, ref _itemDropableLandArrayOffset, itemDropableLandArrRead);

			// ###############
	        States states = new States();

	        grpcMobileDataReplay = GrpcMobileList.Parser.ParseFrom(mobileDataSubsetArray);
	        grpcEquippedItemReplay = GrpcItemList.Parser.ParseFrom(equippedItemSubsetArray);
	        grpcBackpackItemReplay = GrpcItemList.Parser.ParseFrom(backpackItemSubsetArray);

	        grpcPlayerMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(playerMobileObjectSubsetArray);
	        grpcMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(mobileObjectSubsetArray);
	        //grpcItemObjectReplay = GrpcGameObjectList.Parser.ParseFrom(itemObjectSubsetArray);
	        grpcItemDropableLandReplay = GrpcGameObjectSimpleList.Parser.ParseFrom(itemDropableLandSubsetArray);

        	try 
            {
	            // ###############
				//byte[] corpseItemSubsetArray = GetSubsetArray(index, corpseItemArrayLengthListRead, ref _corpseItemArrayOffset, corpseItemArrRead);
				//byte[] popupMenuSubsetArray = GetSubsetArray(index, popupMenuArrayLengthListRead, ref _popupMenuArrayOffset, popupMenuArrRead);

	            //byte[] itemDropableLandSubsetArray = GetSubsetArray(index, itemDropableLandArrayLengthListRead, ref _itemDropableLandArrayOffset, itemDropableLandArrRead);
	            //byte[] vendorItemObjectSubsetArray = GetSubsetArray(index, vendorItemObjectArrayLengthListRead, ref _vendorItemObjectArrayOffset, vendorItemObjectArrRead);

	            // ###############
            	//grpcMobileDataReplay = GrpcMobileList.Parser.ParseFrom(mobileDataSubsetArray);
		    	//grpcEquippedItemReplay = GrpcItemList.Parser.ParseFrom(equippedItemSubsetArray);
		    	//grpcBackpackItemReplay = GrpcItemList.Parser.ParseFrom(backpackItemSubsetArray);
		    	//grpcCorpseItemReplay = GrpcItemList.Parser.ParseFrom(corpseItemSubsetArray);
		    	//grpcPopupMenuReplay = GrpcPopupMenuList.Parser.ParseFrom(popupMenuSubsetArray);
		    	//grpcClilocDataReplay = GrpcClilocDataList.Parser.ParseFrom(clilocDataSubsetArray);

		    	// ###############
            	//grpcPlayerMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(playerMobileObjectSubsetArray);
            	//grpcMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(mobileObjectSubsetArray);
            	//grpcItemObjectReplay = GrpcGameObjectList.Parser.ParseFrom(itemObjectSubsetArray);
            	//grpcItemDropableLandReplay = GrpcGameObjectSimpleList.Parser.ParseFrom(itemDropableLandSubsetArray);
            	//grpcVendorItemObjectReplay = GrpcGameObjectList.Parser.ParseFrom(vendorItemObjectSubsetArray);

            	//states.MobileList = grpcMobileDataReplay;
            	//states.PlayerMobileObjectList = grpcPlayerMobileObjectReplay;
			}
            catch (Exception ex)
            {
            	Console.WriteLine("Failed to parser the GetSubsetArray from original array 1: " + ex.Message);
            }

            states.MobileList = grpcMobileDataReplay;
            states.EquippedItemList = grpcEquippedItemReplay;
            states.BackpackItemList = grpcBackpackItemReplay;
            //states.CorpseItemList = grpcCorpseItemReplay;
            //states.PopupMenuList = grpcPopupMenuReplay;
            //states.ClilocDataList = grpcClilocDataReplay;

            states.PlayerMobileObjectList = grpcPlayerMobileObjectReplay;
            states.MobileObjectList = grpcMobileObjectReplay;
            //states.ItemObjectList = grpcItemObjectReplay;
            states.ItemDropableLandList = grpcItemDropableLandReplay;
            //states.VendorItemObjectList = grpcVendorItemObjectReplay;
			
            _replayStep++;

            if (_replayStep >= _replayLength) {
            	Console.WriteLine("_replayStep >= _replayLength");
            	Reset();
            	//Environment.Exit(0);
            }
           
            return Task.FromResult(states);
        }
    }
}