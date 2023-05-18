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
using ClassicUO.Game.UI.Controls;
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
        public override Task<States> Reset(Config config, ServerCallContext context)
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

            //states.ScreenImage = new ScreenImage { Image = byteString };

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<States> ReadObs(Config config, ServerCallContext context)
        {
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);
            List<GrpcMobileData> grpcMobileDataList = new List<GrpcMobileData>();
            try
            {
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
	        }
	        catch (Exception ex)
            {
            	Console.WriteLine("Failed to load the mobile: " + ex.Message);
            	grpcMobileDataList.Add(new GrpcMobileData{ Name = "base", 
	                									   X = (float) 500.0, 
	                									   Y = (float) 500.0,
	                									   Race = (uint) 0,
	                									   Serial = (uint) 1234 });
            }

            List<GrpcItemData> grpcItemDataList = new List<GrpcItemData>();
            foreach (Item item in World.Items.Values)
            {
            	// Name: Valorite Longsword, Amount: 1, Serial: 1073933224
            	//Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
            	//																	 item.Amount, item.Serial);
            	grpcItemDataList.Add(new GrpcItemData{ Name = !string.IsNullOrEmpty(item.Name) ? item.Name : "Null", 
	            									   Layer = (uint) item.Layer,
	              									   Serial = (uint) item.Serial,
	            									   Amount = (uint) item.Amount });


            }

            Layer[] _layerOrder =
	        {
	            Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
	            Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
	            Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
	        };

            Layer[] _layerOrder_quiver_fix =
	        {
	            Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
	            Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Cloak, Layer.Waist,
	            Layer.Necklace,
	            Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
	        };

	        if (World.Player != null) {
	        	Item equipItem = World.Player.FindItemByLayer(Layer.Cloak);
	        	Item arms = World.Player.FindItemByLayer(Layer.Arms);

	        	//UIManager.GetGump<PaperDollGump>(World.Player.Serial).GetEquipmentSlot();
	        	//PaperdollGump.GetEquipmentSlot();
	        	//Console.WriteLine("equipItem: {0}, arms: {1}", equipItem, arms);

	        	foreach (Layer layer in _layerOrder) {
		            equipItem = World.Player.FindItemByLayer(layer);
		            try {
		            	Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", equipItem.Name, equipItem.Layer, 
	            																			 equipItem.Amount, equipItem.Serial);
		            }
		            catch (Exception ex)
		            {
		            	Console.WriteLine("Failed to load the equipped item: " + ex.Message);
		            }
		        }
	        }

            /*
            try
            {
	            foreach (Item item in World.Items.Values)
	            {
	            	// Name: Valorite Longsword, Amount: 1, Serial: 1073933224
	            	Console.WriteLine("Name: {0}, Layer: {1}, Amount: {2}, Serial: {3}", item.Name, item.Layer, 
	            																		 item.Amount, item.Serial);
	            	//Thread.Sleep(100);
	            	//grpcItemDataList.Add(new GrpcItemData{ Name = item.Name, 
	                //									   Layer = (uint) item.Layer,
	                //									   Serial = (uint) item.Serial,
	                //									   Amount = (uint) item.Amount });
	            }
	        }
	        catch (Exception ex)
            {
            	Console.WriteLine("Failed to load the item: " + ex.Message);
            }
            */

            States states = new States();

            GrpcMobileList grpcMobileList = new GrpcMobileList();
            grpcMobileList.Mobile.AddRange(grpcMobileDataList);
            states.MobileList = grpcMobileList;

            GrpcItemList grpcItemList = new GrpcItemList();
            grpcItemList.Item.AddRange(grpcItemDataList);
            states.ItemList = grpcItemList;

            //Console.WriteLine("Name: {0}, Race: {1}", mob.Name, mob.Race);
            states.Rewards = new Rewards { AttackMonster = _controller.attackMonsterReward,
            							   KillMonster = _controller.killMonsterReward};

            return Task.FromResult(states);
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> WriteAct(Actions actions, ServerCallContext context)
        {
        	//Console.WriteLine("actions.ActionType: {0}", actions.ActionType);
        	//Console.WriteLine("actions.MobileSerial: {0}", actions.MobileSerial);
        	//Console.WriteLine("actions.WalkDirection.Direction: {0}", actions.WalkDirection.Direction);

            _controller.action_1 = actions.ActionType;

            if (actions.ActionType == 1) {
            	if (World.InGame == true) {
            		if (actions.WalkDirection.Direction == 0) {
	            		World.Player.Walk(Direction.Up, true);
	            	}
	            	else if (actions.WalkDirection.Direction == 2) {
	            		World.Player.Walk(Direction.Right, true);
	            	}	
	            	else if (actions.WalkDirection.Direction == 3) {
	            		World.Player.Walk(Direction.Left, true);
	            	}
	            	else if (actions.WalkDirection.Direction == 4) {
	            		World.Player.Walk(Direction.Down, true);
	            	}
            	}
	        }
	        else if (actions.ActionType == 2) {
	        	if (World.Player != null) {
        			GameActions.DoubleClick(actions.MobileSerial);
	        	}
	        }
	        else if (actions.ActionType == 3) {
	        	if (World.Player != null) {
        			
	        	}
	        }

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ActSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            _controller.sem_action.Release();

            return Task.FromResult(new Empty {});
        }

        public override Task<Empty> ObsSemaphoreControl(SemaphoreAction action, ServerCallContext context)
        {
            //Console.WriteLine("action.Mode: {0}", action.Mode);
            _controller.sem_observation.WaitOne();

            return Task.FromResult(new Empty {});
        }
    }
}