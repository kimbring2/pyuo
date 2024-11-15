# Introduction
The code of this repository is the modified [ClassicUO 0.1.9.0](https://github.com/ClassicUO/ClassicUO/tree/0.1.9.0) client. It is made to be used with [UoService](https://github.com/kimbring2/uoservice). The gRPC part to commucated with Python, and Semephroe to sync the state and action for Machine Learning training, and the replay record system is added to the original client.

# Requirement
- Ubuntu 22.04
- .NET 8.0([sudo apt-get install -y dotnet-runtime-8.0](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-2204))
- [Ultima Online Client 7.0.95.0 version](https://drive.google.com/drive/folders/1Lv838NH_KkxU7rEnaFAFHLzC-o5V4pfz?usp=sharing): You need to use the older version than the [client of official website](https://downloads.eamythic.com/uo/installers/UOClassicSetup_7_0_24_0.exe) because there was a recent update.

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
Unlike the original client, the client for UoService must operate most of the operations through an argument setting.

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
e.g.  $ ./ClassicUO -username kimbring2 -password kimbring2 -window_width 1370 -window_height 1280 -grpc_port 60051
```

- No gRPC communication with Python, Replay saving
```
$ ./ClassicUO -username [Account ID] -password [Account PWD] -window_width [Screen Width] -window_height [Screen Height] -replay -replay_length_scale [Length scale of replay step, total value * 1000 step will be recorded]
e.g. ./ClassicUO -username kimbring2 -password kimbring2 -human_play -window_width 1370 -window_height 1280 -replay -replay_length_scale 4
```

- Run [UoService](https://github.com/kimbring2/uoservice/blob/main/README.md#run-an-agent) after that.
