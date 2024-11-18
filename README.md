# IoT

Code running on my home automation IoT devices.

- The [Azure Sphere MT3620](https://www.avnet.com/shop/us/products/avnet-engineering-services/aes-ms-mt3620-sk-g-3074457345636825680/) sends data using its luminosity sensor and barometer. Since Azure Sphere cannot use mqtt directly data is sent to an [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/) which forwards the data to an mqtt broker ([CloudMQTT](https://www.cloudmqtt.com/)).
- The [NodeMCU V3 device](https://www.instructables.com/id/Getting-Started-With-ESP8266LiLon-NodeMCU-V3Flashi/) detects motion using an attached motion sensor. Data is send using mqtt.
- A [Intel Galileo Gen 2](https://www.arduino.cc/en/ArduinoCertified/IntelGalileoGen2) is used for P1 port readings (gas and electricity consumption). It is connected to an mqtt broker.

All data is received by Home Assistant using [mqtt](http://mqtt.org/). See my [Home Assistant config repo](https://github.com/Expecho/HomeAssistant) for additional details.

