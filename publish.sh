#!/usr/bin/env bash

dotnet publish -r linux-x64 -c Release -f netcoreapp3.1 -p:PublishSingleFile=true
