#!/usr/bin/env bash

dotnet publish -r linux-x64 -c Release -f netcoreapp3.1
rem /p:PublishSingleFile=true 
rem dotnet publish -r linux-x64 -c Release -f netcoreapp3.1 --self-contained
rem dotnet publish -r linux-arm -c Release -f netcoreapp3.1 --self-contained
