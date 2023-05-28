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
        Channel _grpcChannel;

    	int _arrayOffset = 0;
    	int _replayStep = 0;
    	string _replayName;

    	byte[] mobileObjectLengthArrRead;
    	byte[] mobileObjectArrRead;

    	byte[] actionTypeArrRead;
    	byte[] walkDirectionArrRead;

    	public GrpcGameObjectList grpcMobileObjectReplay;

    	public UoServiceReplayImpl(int port)
        {
            _port = port;

            _grpcChannel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
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

    	public void ReadMPQFile(string replayName) 
        {
        	Console.WriteLine("ReadMPQFile()");
            	
        	mobileObjectLengthArrRead = ReadFromMpqArchive("Replay/" + replayName + ".uoreplay", "replay.metadata.length");
        	mobileObjectArrRead = ReadFromMpqArchive("Replay/" + replayName + ".uoreplay", "replay.object.mobile");

        	actionTypeArrRead = ReadFromMpqArchive("Replay/" + replayName + ".uoreplay", "replay.actionType");
			walkDirectionArrRead = ReadFromMpqArchive("Replay/" + replayName + ".uoreplay", "replay.walkDirection");
        }

    	// Server side handler of the SayHello RPC
        public override Task<States> ReadReplay(Config config, ServerCallContext context)
        {
        	Console.WriteLine("ReadReplay()");

        	if (_replayStep % 1000 == 0)
        	{
        		_arrayOffset = 0;
        	}
        	
        	int index = _replayStep % 1000;
            List<int> list_read_1 = ConvertByteArrayToIntList(mobileObjectLengthArrRead);

        	int item = list_read_1[index];
        	//Console.WriteLine("item: {0}\n", item);

        	int startIndex = _arrayOffset; 
        	int length = _arrayOffset + item; 

        	byte[] subsetArray = new byte[item];

			Array.Copy(mobileObjectArrRead, startIndex, subsetArray, 0, item);
            _arrayOffset += item;

            try 
            {
            	grpcMobileObjectReplay = GrpcGameObjectList.Parser.ParseFrom(subsetArray);
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

	        States states = new States();
	        states.MobileObjectList = grpcMobileObjectReplay;

            _replayStep++;
           
            return Task.FromResult(states);
        }
    }
}