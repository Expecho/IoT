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

WiFiClient espClient;
PubSubClient client(espClient);

void connect_wifi() {
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(wifi_ssid);

  WiFi.begin(wifi_ssid, wifi_password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

void connect_mqtt() {
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    if (client.connect("ESP8266Client", mqtt_user, mqtt_password, "motion/status", 0, 0, "disconnected")) {
      Serial.println("connected");
      client.publish("motion/status", "connected", false);
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" trying again in 5 seconds");
      delay(5000);
    }
  }
}

void setup() {
  Serial.begin(115200);
  Serial.println("Setting things up.");

  connect_wifi();

  client.setServer(mqtt_server, mqtt_port);

  pinMode(STATUS_LED, OUTPUT);
  pinMode(MOTION_DETECTOR, INPUT);

  ArduinoOTA.setHostname("nodemcu");

  ArduinoOTA.onStart([]() {
    String type;
    if (ArduinoOTA.getCommand() == U_FLASH) {
      type = "sketch";
    } else { // U_FS
      type = "filesystem";
    }

    // NOTE: if updating FS this would be the place to unmount FS using FS.end()
    Serial.println("Start updating " + type);
  });
  ArduinoOTA.onEnd([]() {
    Serial.println("\nEnd");
  });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
  });
  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) {
      Serial.println("Auth Failed");
    } else if (error == OTA_BEGIN_ERROR) {
      Serial.println("Begin Failed");
    } else if (error == OTA_CONNECT_ERROR) {
      Serial.println("Connect Failed");
    } else if (error == OTA_RECEIVE_ERROR) {
      Serial.println("Receive Failed");
    } else if (error == OTA_END_ERROR) {
      Serial.println("End Failed");
    }
  });
  ArduinoOTA.begin();
}

long lastState = LOW;

void loop() {
  ArduinoOTA.handle();

  if (!client.connected()) {
    connect_mqtt();
  }
  client.loop();

  long state = digitalRead(MOTION_DETECTOR);

  if (state == lastState)
  {
    return;
  }

  if (state == HIGH)
  {
    client.publish("motion/status", "connected", false);
    client.publish(motion_topic, "ON", true);
  }

  digitalWrite(STATUS_LED, LED_OFF);

  lastState = state;

  delay(1000);
}
