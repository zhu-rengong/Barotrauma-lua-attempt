#!/bin/sh

cd ../../../Barotrauma

cd BarotraumaClient
dotnet publish WindowsClient.csproj -c Release -clp:"ErrorsOnly;Summary" --self-contained -r win-x64 \/p:Platform="x64" \/p:RollForward=Disable \/p:RuntimeFrameworkVersion=3.1.16

cd ..
cd BarotraumaServer
dotnet publish WindowsServer.csproj -c Release -clp:"ErrorsOnly;Summary" --self-contained -r win-x64 \/p:Platform="x64" \/p:RollForward=Disable \/p:RuntimeFrameworkVersion=3.1.16
