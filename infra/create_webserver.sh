#!/bin/sh

# disable auto upgrades by apt
cd /home/dave

cat <<EOT >> 20auto-upgrades
APT::Periodic::Update-Package-Lists "0";
APT::Periodic::Download-Upgradeable-Packages "0";
APT::Periodic::AutocleanInterval "0";
APT::Periodic::Unattended-Upgrade "1";
EOT

sudo cp /home/dave/20auto-upgrades /etc/apt/apt.conf.d/20auto-upgrades

# go with newer apt which gets dependency updates too (like linux-azure)
sudo apt update -y
sudo apt upgrade -y
  
# Install .NET 5 on Ubutu 20.04 LTS
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0

# create document root for published files 
sudo mkdir /var/www

# create gitsource folder and clone
sudo mkdir /gitsource
cd /gitsource
sudo git clone https://github.com/djhmateer/tus-test .

# compile and publish the web app
sudo dotnet publish /gitsource/src --configuration Release --output /var/www

# change ownership of the published files to what it will run under
sudo chown -R www-data:www-data /var/www
# allow exective permissions
sudo chmod +x /var/www

# cookie keys to allow machine to restart and for it to 'remember' cookies
# todo - store these in blob storage?
# sudo mkdir /var/osr-cookie-keys
# sudo chown -R www-data:www-data /var/osr-cookie-keys
# allow read and write
# sudo chmod +rw /var/osr-cookie-keys

# auto start on machine reboot
sudo systemctl enable kestrel.service

# start the Kestrel web app using systemd using kestrel.service text files
sudo systemctl start kestrel.service

# redirect 5000 to port 80 internally
sudo iptables -t nat -A PREROUTING -p tcp --dport 80 -j REDIRECT --to-port 5000

# sudo snap install bpytop
