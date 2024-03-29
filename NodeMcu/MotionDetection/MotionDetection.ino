#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <ESP8266mDNS.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include "secrets.h"

#define wifi_ssid wifi_ssid_secret
#define wifi_password wifi_password_secret

#define mqtt_server "farmer.cloudmqtt.com"
#define mqtt_port 11245
#define mqtt_user mqtt_user_secret
#define mqtt_password mqtt_password_secret

#define motion_topic "sensor/motion"

const int STATUS_LED = 2;
const int MOTION_DETECTOR = 12;

const int LED_ON = LOW;
const int LED_OFF = HIGH;

WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);

void setup() {
  Serial.begin(115200);
  Serial.println("Setting things up.");

  pinMode(STATUS_LED, OUTPUT);

  connectWifi();

  configureOTA();

  mqttClient.setServer(mqtt_server, mqtt_port);

  pinMode(STATUS_LED, OUTPUT);
  pinMode(MOTION_DETECTOR, INPUT);

  Serial.println("Ready to serve!");
}

long lastState = LOW;

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("Reconnecting to WiFi...");
    WiFi.disconnect();
    WiFi.reconnect();
  }
  
  ArduinoOTA.handle();

  if (!mqttClient.connected()) {
    connectMqtt();
  }
  mqttClient.loop();

  long state = digitalRead(MOTION_DETECTOR);

  if (state == lastState)
  {
    return;
  }

  if (state == HIGH)
  {
    mqttClient.publish("motion/status", "connected", false);
    mqttClient.publish(motion_topic, "ON", true);
  }

  digitalWrite(STATUS_LED, LED_OFF);

  lastState = state;

  delay(1000);
}
