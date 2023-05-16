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
        public override Task<States> reset(ImageRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();

            foreach (Mobile mob in World.Mobiles.Values)
            {
            	//Console.WriteLine("mob.GetScreenPosition().X: {0}, mob.GetScreenPosition().Y: {1}", 
            	//				     mob.GetScreenPosition().X, mob.GetScreenPosition().Y);
            	//Console.WriteLine("Name: {0}, Race: {1}", mob.Name, mob.Race);

                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, 
                										   X = (uint) mob.GetScreenPosition().X, 
                										   Y = (uint) mob.GetScreenPosition().Y,
                										   Race = (uint) mob.Race });
            }

            States states = new States();

            states.ScreenImage = new ScreenImage { Image = byteString };

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<States> step(ImageRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();

            foreach (Mobile mob in World.Mobiles.Values)
            {
            	//Console.WriteLine("Name: {0}, Race: {1}", mob.Name, mob.Race);

                grpcMobileDataList.Add(new GrpcMobileData{ Name = mob.Name, 
                										   X = (uint) mob.GetScreenPosition().X, 
                										   Y = (uint) mob.GetScreenPosition().Y,
                										   Race = (uint) mob.Race });
            }

            States states = new States();

            states.ScreenImage = new ScreenImage { Image = byteString };

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> act(Actions actions, ServerCallContext context)
        {
        	Console.WriteLine("actions.Action: {0}", actions.Action);
        	Console.WriteLine("actions.MousePoint: {0}", actions.MousePoint);

        	_controller.SetMousePosition(actions.MousePoint.X, actions.MousePoint.Y);

            _controller.action_1 = actions.Action;

            //Console.WriteLine("_flags[0]: {0}, _flags[1]: {1}, _flags[0]: {2}, _flags[0]: {3}", _flags[0], _flags[2], _flags[1], _flags[3]);
            Direction dir = DirectionHelper.DirectionFromKeyboardArrows(true, false, false, false);
            if (World.InGame && !Pathfinder.AutoWalking && dir != Direction.NONE)
            {
                //Console.WriteLine("World.Player.Walk");
                //World.Player.Walk(dir, ProfileManager.CurrentProfile.AlwaysRun);
                ;
            }

            return Task.FromResult(new Empty {});
        }
    }
}