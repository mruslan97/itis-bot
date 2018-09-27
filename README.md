# Itis Schedule Tg-bot - next gen.

## [Try it (unavailable till october)](https://t-do.ru/itis_scheduleBot)

### [Try dev build (available for old copy of schedule)](https://t-do.ru/itis_testBot)

## How to run by yourself

### Cross-platform part

* Clone this repo
* Obtain `client_secret.json` as described [here](https://developers.google.com/sheets/api/quickstart/dotnet) and add it to `ScheduleBot/ScheduleBot.AspHost` directory (It's neccessary for access to Google Sheets)
* Running config is defined by `appsetting.{ASPNETCORE_ENVIRONMENT}.json` files, based on `appsetting.json` at `ScheduleBot/ScheduleBot.AspHost` directory. Settings include:
   - `UseWebHook` - set up `true` for handle requests with webhook (required specified `WebhookUrl` param, we use Ngrok for this feature), `false` for Long-pooling (request to tg-server every sec)
   - `ScheduleBot` - specify your bot api token and username
   - `GoogleApi` - specify your application name for `client_secret.json` and spreadsheet id
   - `NotificationsSecret` - passphrase to send notification to all bot's subscribers (to do it you need create Supergroup with someone with bot as admin, delegate him access to delete messages and write to this chat message in next format: `/sendsudo key=PASSWORDHERE your text here to all` )
* Install [dotnet](https://www.microsoft.com/net/download/) for your platform

### Linux part

* Look at [this  article](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-2.1&tabs=aspnetcore2x), it makes things clear
* After adding `client_secret.json` and required `appsettings.{environment}.json` publish app from repo
   ````
   cd /root/repos/itis-bot/ScheduleBot/ScheduleBot.AspHost/
   dotnet publish -c Release -o /srv/dotnet/bot/
   ````

   where `-o /PATH` defines path to working directory where result `ScheduleBot.AspHost.dll` will appear.
* You can test build with run (default `ASPNETCORE_ENVIRIONMENT=Production`)
   ````
   cd /srv/dotnet/bot/
   dotnet ScheduleBot.AspHost.dll
   ````
   - If You don't have GUI app will fail to run, because Google API needs to authentificate app via browser (see `logs` directory). But there is little trick to walk it around, if You have only CLI. Run app on another host with GUI, authentificate yourself via browser and copy-paste `sheets.googleapis.com-dotnet-quickstart.json/` directory on this GUI-host into `/home/APP_USER/.credentials/` ( `~/.credentials/` by default) on your Linux-CLI-host. Example, on Windows, after launch, You can find it here `C:\Users\YOURUSER\Documents\.credentials\sheets.googleapis.com-dotnet-quickstart.json\`
* Check logs, if all right, add service.
   ````
   sudo nano /etc/systemd/system/itis-bot.service
   ````

   Config example:
  * run as root, so `/root/.credentials/sheets.googleapis.com-dotnet-quickstart.json/` required
  * changed `ASPNETCORE_ENVIRIONMENT=Release` to use `appsettings.Release.json` config
   ````
   [Unit]
   Description=ASP.NET Core based telegram bot for schedule delivering

   [Service]
   WorkingDirectory=/srv/dotnet/bot
   ExecStart=/usr/bin/dotnet /srv/dotnet/bot/ScheduleBot.AspHost.dll
   Restart=always
   RestartSec=40  # Restart service after 40 seconds if dotnet service crashes
   SyslogIdentifier=itis-bot-service
   User=root
   Environment=ASPNETCORE_ENVIRONMENT=Release
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target
   ````
   then
   ````
   systemctl enable itis-bot.service
   systemctl start itis-bot.service
   systemctl status itis-bot.service
   ````
* Using webhook with [Ngrok](https://ngrok.com)

   * Download and unpackage ngrok (signup required, see website)
   ````
   wget "https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-amd64.zip"
   unzip ./ngrok-stable-linux-amd64.zip
   ./ngrok authtoken YOUR_NGROK_TOKEN_HERE   
   ````
   * Run it in background
   ```
   ./ngrok http 8443 -region=eu  > /dev/null &
   ```
   * Find up https tunnel address
   ```
   curl localhost:4040/api/tunnels
   > bla-bla { "public_url":"https://XXXXXX.eu.ngrok.io","proto":"https", } bla-bla
   ```
   * Add address to `appsettings.{ASPNETCORE_ENVIRONMENT}.json` (`Release` here) and enable Webhook. Don't forget add `api/update` uri part to webhook url.
   ````
   nano ~/repos/itis-bot/ScheduleBot/ScheduleBot.AspHost/appsettings.Release.json
   ````
   ````
   {
      "UseWebHook": true,
      ....
      "ScheduleBot": {
         ....
         "WebhookUrl": "https://XXXXXX.eu.ngrok.io/api/update"
      },
      ...
   }
   ````
   * Stop service, rebuild and run again. 

     /property option used only for running under core 2.1 sdk: [SO answer](https://stackoverflow.com/questions/46491957/asp-net-core-2-missing-applicationinsights)
   ````
   cd ~/repos/itis-bot/ScheduleBot/ScheduleBot.AspHost/
   systemctl stop itis-bot.service
   dotnet publish -c Release -o /srv/dotnet/bot/ /property:PublishWithAspNetCoreTargetManifest=false
   systemctl start itis-bot.service
   ````

### Windows part

>Important for developers in Russian Federation: please ensure You have access to telegram.org directly or througth system VPN. If you'r using system proxy, it's not guaranteed that bot will serve requests. If you have Fiddler, it could help to bypass blocks, when running (with system proxy). If you have a suggestion, how to reach tg.org on your local while development, welcome to #9

* Set up environment
   * Power-Shell
   ````
   $Env:ASPNETCORE_ENVIRONMENT = "Development"
   ````
   * Git-bash
   ````
   $ ASPNETCORE_ENVIRONMENT=Development
   ````
   * cmd
   ````
   > set ASPNETCORE_ENVIRONMENT=Development
   ````
*  At repo:
   ````
   > cd .\ScheduleBot\ScheduleBot.AspHost
   > dotnet build -c Release
   > cd .\bin\Release\netcoreapp2.0
   > dotnet ScheduleBot.AspHost.dll
   ````
* Using webhook with [Ngrok](https://ngrok.com)

   * in progress
