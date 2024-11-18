# Prepare

Create a new file `secrets` to store the secrets with the following content:

```javascript
module.exports = {
   mqttUsr: '',
   mqttPwd: '',
   mqttHost: '',
   mqttPort: 0
}
```

# Upload the scripts

Place the following files in the Intel Galileo board at `/usr/bin/src/backend`:

- `secrets`
- `p1.js`
- `DSRMPacketParser.js`

# Useful commands

SSH into the Intel Galileo board:

### Stop the service

```bash
systemctl stop p1.service
```

### Start the service

```bash
systemctl start p1.service
```

### Check the status of the service

```bash
systemctl status p1.service -l
```