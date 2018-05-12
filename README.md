# Itis Schedule Tg-bot - next gen.

## [Try it](https://t-do.ru/itis_scheduleBot)

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
* After adding `client_secret.json` and neccesary `appsettings.json` publish app from repo
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

### Windows part

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