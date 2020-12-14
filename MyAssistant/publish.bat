@echo off
cd %1
dotnet publish  -c Release -p:PublishProfile=FolderProfile