// protoc --csharp_out=. --grpc_out=. --plugin=protoc-gen-grpc=`which grpc_csharp_plugin` UoService.proto
// python3.7 -m grpc_tools.protoc -I ../ --python_out=. --grpc_python_out=. UoService.proto --proto_path /home/kimbring2/uoservice/uoservice/protos/

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf;
using Uoservice;
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
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Grpc
{
	internal sealed class UoServiceImpl : UoService.UoServiceBase
    {
        GameController _controller;
        int _port;
        Server _grpcServer;
        Channel _grpcChannel;

        public UoServiceImpl(GameController controller, int port)
        {
            _controller = controller;
            _port = port;

            _grpcChannel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            _grpcServer = new Server
	        {
	            Services = { UoService.BindService(this) },
	            Ports = { new ServerPort("localhost", _port, ServerCredentials.Insecure) }
	        };
        }

        public void Start() {
        	_grpcServer.Start();
        }

        public byte[] ReadImage(string imagePath)
        {
            try
            {
                return File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read the image: " + ex.Message);

                return null;
            }
        }

        // Server side handler of the SayHello RPC
        public override Task<States> Reset(ImageRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();

            foreach (Mobile mob in World.Mobiles.Values)
            {
            	Console.WriteLine("mob.GetScreenPosition().X: {0}, mob.GetScreenPosition().Y: {1}", 
            					     mob.GetScreenPosition().X, mob.GetScreenPosition().Y);
            	Console.WriteLine("Name: {0}, Race: {1}", mob.Name, mob.Race);

            	if ( (mob.GetScreenPosition().X <= 0.0) || (mob.GetScreenPosition().Y <= 0.0) ) 
            	{
            		continue;
            	}

                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, 
                										   X = (float) mob.GetScreenPosition().X, 
                										   Y = (float) mob.GetScreenPosition().Y,
                										   Race = (uint) mob.Race });
            }

            States states = new States();

            states.ScreenImage = new ScreenImage { Image = byteString };

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            //_controller.act_lock.Release();

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<States> ReadObs(ImageRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();

            //Console.WriteLine("World.Mobiles.Values: {0}", World.Mobiles.Values);
            foreach (Mobile mob in World.Mobiles.Values)
            {
            	//Console.WriteLine("mob.GetScreenPosition().X: {0}, mob.GetScreenPosition().Y: {1}", 
            	//				     mob.GetScreenPosition().X, mob.GetScreenPosition().Y);
            	//Console.WriteLine("Name: {0}, Race: {1}", mob.Name, mob.Race);

            	if ( (mob.GetScreenPosition().X <= 0.0) || (mob.GetScreenPosition().Y <= 0.0) ) 
            	{
            		continue;
            	}

                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, 
                										   X = (float) mob.GetScreenPosition().X, 
                										   Y = (float) mob.GetScreenPosition().Y,
                										   Race = (uint) mob.Race,
                										   Serial = (uint) mob.Serial });
            }

            States states = new States();

            states.ScreenImage = new ScreenImage { Image = byteString };

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(Actions actions, ServerCallContext context)
        {
        	//Console.WriteLine("actions.Action: {0}", actions.Action);
        	Console.WriteLine("actions.MousePoint.X: {0}", actions.MousePoint.X);
        	Console.WriteLine("actions.MousePoint.Y: {0}", actions.MousePoint.Y);
        	//Console.WriteLine("actions.Serial: {0}", actions.Serial);

        	//_controller.SetMousePosition(actions.MousePoint.X, actions.MousePoint.Y);

            _controller.action_1 = actions.Action;
            //Console.WriteLine("World.Player: {0}", World.Player);

            try
            {	
            	int x = ProfileManager.CurrentProfile.GameWindowPosition.X + (ProfileManager.CurrentProfile.GameWindowSize.X >> 1);
                int y = ProfileManager.CurrentProfile.GameWindowPosition.Y + (ProfileManager.CurrentProfile.GameWindowSize.Y >> 1);
            	
	            Direction direction = (Direction) GameCursor.GetMouseDirection
	            (
	            	x,
                    y,
	                (int) actions.MousePoint.X,
	                (int) actions.MousePoint.Y,
	                1
	            );
	            
	            //Direction direction = DirectionHelper.DirectionFromKeyboardArrows(true, false, false, false);
	            
	            if (World.Player != null) {
	            	Console.WriteLine("World.Player.Walk(direction, true)");
	            	World.Player.Walk(direction, true);
	            }
	        }
	        catch (Exception e)
            {
            	Console.WriteLine("Exception: {0}", e);

            	Direction direction = DirectionHelper.DirectionFromKeyboardArrows(true, false, false, false);

            	if (World.Player != null) {
	            	World.Player.Walk(direction, true);
	            }
            }   

            //Console.WriteLine("Pass check");
            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ActSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
        	//Console.WriteLine("action.Mode: {0}", action.Mode);

        	//_controller.act_lock = true;
        	_controller.sem_action.Release();

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ObsSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
        	//Console.WriteLine("action.Mode: {0}", action.Mode);

        	/*
        	while (_controller.obs_lock == false) 
            {
            	Console.WriteLine("_controller.obs_lock == false");
            	int milliseconds = 10;
            	Thread.Sleep(milliseconds);
            }
            _controller.obs_lock = false;
			*/
			_controller.sem_observation.WaitOne();

            return Task.FromResult(new Empty {});
        }

    }
}