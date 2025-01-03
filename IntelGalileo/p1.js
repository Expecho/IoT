var Serialport = require("serialport").SerialPort;
var five = require("johnny-five");
var Galileo = require("galileo-io");
var secrets = require('./secrets');
var parsePacket = require('./DSMRPacketParser');
var mqtt = require('mqtt');
var ping = require('ping');

// Define clients
var mqttClient = mqtt.connect({
  host: secrets.mqttHost,
  port: secrets.mqttPort,
  username: secrets.mqttUsr,
  password: secrets.mqttPwd,
  protocol: 'mqtts',
  will: {
    topic: 'galileo/status',
    payload: "disconnected"
  },
  rejectUnauthorized: false
});

console.log("using host: " + secrets.mqttHost);

// State variables
var received = '';
var lastGasReading = 0;
var keepAliveCounter = 0;

// Init Intel Galileo Gen 2 board
var board = new five.Board({
  io: new Galileo(),
  repl: false
});

board.on("ready", function () {
  var serialPort = new Serialport("/dev/ttyUSB0", {
    baudRate: 115200
  });

  serialPort.on("open", function () {
    console.log("Port /dev/ttyUSB0 is open!");

    mqttClient.publish("galileo/status", "connected");

    serialPort.on("data", function (data) {
      received += data.toString();

      const startCharPos = received.indexOf("/");
      const endCharPos = received.indexOf("!");

      var packageCompleted = startCharPos >= 0 && endCharPos >= 0
      if (packageCompleted) {
        var cfg = {
          deadline: 5,
        };

        ping.sys.probe("192.168.1.1", function (isAlive) {
          if (isAlive) {
            console.log("alive");
          }
          else {
            console.log("dead");
            require('reboot').reboot();
          }

          sendDeviceOnlineMessage();

          const packet = received.substr(startCharPos, endCharPos - startCharPos);
          const parsedPacket = parsePacket(packet);

          received = '';

          if (parsedPacket.timestamp == null) {
            console.log("Invalid reading: " + JSON.stringify(parsedPacket));
            return;
          }

          var jsonMessage = createJsonFromParsedData(parsedPacket);
          mqttClient.publish("sensor/p1", jsonMessage);

          console.log("Published to sensor/p1");
        }, cfg);
      }
    });
  });
});

// publish a device online message to the designated mqtt status topic
function sendDeviceOnlineMessage() {
  keepAliveCounter++;

  if (keepAliveCounter > 60) { // ~10 minutes
    keepAliveCounter = 0;
    mqttClient.publish("galileo/status", "connected");
  }
}

function createJsonFromParsedData(parsedPacket) {
  var gasActual = 0;

  if (lastGasReading > 0) {
    gasActual = parsedPacket.gas.reading - lastGasReading;
  }

  var message = JSON.stringify(
    {
      timestamp: parsedPacket.timestamp,
      electricity_tariff1_received: parsedPacket.electricity.received.tariff1.reading,
      electricity_tariff2_received: parsedPacket.electricity.received.tariff2.reading,
      electricity_actual_received: parsedPacket.electricity.received.actual.reading,
      electricity_tariff1_delivered: parsedPacket.electricity.delivered.tariff1.reading,
      electricity_tariff2_delivered: parsedPacket.electricity.delivered.tariff2.reading,
      electricity_actual_delivered: parsedPacket.electricity.delivered.actual.reading,
      electricity_actual: parsedPacket.electricity.received.actual.reading > 0 ?  parsedPacket.electricity.received.actual.reading : (parsedPacket.electricity.delivered.actual.reading * -1),
      electricity_tariffIndicator: parsedPacket.electricity.tariffIndicator,
      gas_received: parsedPacket.gas.reading,
      gas_actual_received: gasActual
    }, null, 4);

  lastGasReading = parsedPacket.gas.reading;

  return message;
}
