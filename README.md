# Introduction
Modifed [ClassicUO](https://github.com/ClassicUO/ClassicUO) client to be used with [UoService](https://github.com/kimbring2/uoservice). The gRPC part to commucated with Python, Semephroe to sync the state and action for Machine Learning training, and replay record system are added to the original client.

# Requirement
- Ubuntu 22.04

# Build and publish code
- Build
```$ dotnet build "src/ClassicUO.csproj" -c Release --runtime linux-x64```

- Publish
```$ dotnet publish --configuration Release --runtime linux-x64 src/ClassicUO.csproj```

Check the execution file is generatef under the ```bin/dist``` folder 

- Run
```$ ./ClassicUO -username kimbring2 -password kimbring2 -human_play -window_width 1370 -window_height 1280```
