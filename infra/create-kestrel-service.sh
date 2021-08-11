#!/bin/sh

cd /home/dave

# To debug Env variables on live remember to login as www-data
# sudo -u www-data bash

# EOT is End of Transmission
cat <<EOT >> kestrel.service
[Unit]
Description=Website running on ASP.NET 5 

[Service]
WorkingDirectory=/var/www
ExecStart=/usr/bin/dotnet TusTest.dll --urls "http://*:5000"

Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10

# copied from dotnet documentation at
# https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-3.1#code-try-7
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

SyslogIdentifier=dotnet-App
User=www-data

[Install]
WantedBy=multi-user.target
EOT

sudo cp /home/dave/kestrel.service /etc/systemd/system/kestrel.service
