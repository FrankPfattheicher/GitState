#!/usr/bin/env bash

dotnet publish -r linux-x64 -c Release -f net6.0 -p:PublishSingleFile=true
