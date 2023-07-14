# Introduction
Modifed [ClassicUO](https://github.com/ClassicUO/ClassicUO) client to be used with [UoService](https://github.com/kimbring2/uoservice). The gRPC part to commucated with Python, Semephroe to sync the state and action for Machine Learning training, and replay record system are added to the original client.

# Requirement
- Ubuntu 22.04

# Build and publish code
- Build
```
$ dotnet build "src/ClassicUO.csproj" -c Release --runtime linux-x64
```

- Publish
```
$ dotnet publish --configuration Release --runtime linux-x64 src/ClassicUO.csproj
```

Check the execution file is generated under the ```bin/dist``` folder.

# Run code
Unlike the original client, the client for UoService must operate most of the operations through argument setting.

- No gRPC communication with Python, No replay saving
Run the C# Client. You must enter the ID and pwd of the previously created account as parameters. Login, shard selection, and character selection windows are omitted.
```
$ ./ClassicUO -username [Account ID] -password [Account PWD] -human_play -window_width [Screen Width] -window_height [Screen Height]
e.g. $ ./ClassicUO -username kimbring2 -password kimbring2 -human_play -window_width 1370 -window_height 1280
```

- Communication with Python, No replay saving
Run the C# Client. Here, you need to enter the port for gRPC communication with Python.
```
$ ./ClassicUO -username [Account ID] -password [Account PWD] -grpc_port [Port Number]
e.g.  $ ./ClassicUO -username kimbring2 -password kimbring2 -human_play -window_width 1370 -window_height 1280 -grpc_port 60051
```

- No gRPC communication with Python, Replay saving
```
$ ./ClassicUO -username [Account ID] -password [Account PWD] -window_width [Screen Width] -window_height [Screen Height] -replay
e.g. ./ClassicUO -username kimbring2 -password kimbring2 -human_play -window_width 1370 -window_height 1280 -replay
```

- Run [UoService](https://github.com/kimbring2/uoservice/blob/main/README.md#run-an-agent) after that.
