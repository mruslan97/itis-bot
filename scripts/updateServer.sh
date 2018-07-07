#!/bin/bash

cd ./repos/itis-bot/
git checkout dev
git pull
cd ./ScheduleBot/ScheduleBot.AspHost/
systemctl stop itis-bot.service
dotnet publish -c Release -o /srv/dotnet/bot/ /property:PublishWithAspNetCoreTargetManifest=false
systemctl start itis-bot.service