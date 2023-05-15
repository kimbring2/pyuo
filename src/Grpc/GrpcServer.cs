using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf;
using Uoservice;
using System.Threading;


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
        public override Task<ImageResponse> reset(ImageRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            return Task.FromResult(new ImageResponse { Data = byteString });
        }

        // Server side handler of the SayHello RPC
        public override Task<ImageResponse> step(ImageRequest request, ServerCallContext context)
        {
            //Console.WriteLine(request.Name);;
            ByteString byteString = ByteString.CopyFrom(_controller.byteArray);

            return Task.FromResult(new ImageResponse { Data = byteString });
        }

        // Server side handler of the SayHello RPC
        public override Task<Empty> act(Actions actions, ServerCallContext context)
        {
            //Console.WriteLine(actions.action);
            _controller.action_1 = actions.Action;
            //Console.WriteLine("_controller.action_1: {0}", _controller.action_1);

            return Task.FromResult(new Empty {});
        }
    }
}