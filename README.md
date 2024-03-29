# Hosts file web editor

provides web interface to edit dnsmasq custom hosts file

## Requirements

* .NET 6 SDK
* Dnsmasq
* Systemd
* Reverse proxy (nginx, apache httpd, etc...)

## Installation

1. create these files owned by dnsmasq user
   * `/usr/local/etc/hosts`
   * `/etc/dnsmasq.d/cname`
2. configure dnsmasq.conf to use the custom hosts file
   ```
   addn-hosts=/usr/local/etc/hosts
   ```
3. start dnsmasq before make
4. `make && sudo make install`
5. `sudo systemctl start hosts.web`
6. set your reverse proxy upstream to localhost:5053
   ```nginx
   location / {
       proxy_pass http://localhost:5053;
   }
   ```

## Troubleshooting

* put `/usr/local/etc/hosts.web/appsettings.Production.json`
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug"
       }
     }
   }
   ```
* `journalctl -fu hosts.web`
