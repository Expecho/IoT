#include "SPI.h"
#include "driver/rtc_io.h"
#include "esp_camera.h"
#include "esp_timer.h"
#include "img_converters.h"
#include "Arduino.h"
#include "fb_gfx.h"
#include "soc/soc.h" //disable brownout problems
#include "soc/rtc_cntl_reg.h"  //disable brownout problems
#include "esp_http_server.h"
#include <PubSubClient.h>
#include <ArduinoOTA.h>
#include "secrets.h"

#define wifi_ssid wifi_ssid_secret
#define wifi_password wifi_password_secret

#define mqtt_server "farmer.cloudmqtt.com"
#define mqtt_port 11245
#define mqtt_user mqtt_user_secret
#define mqtt_password mqtt_password_secret
#define motion_topic "sensor/motion"

#define ESP_INTR_FLAG_DEFAULT 0

const int STATUS_LED = 4;
const int LED_OFF = LOW;
const int LED_ON = HIGH;
const int MOTION_DETECTOR = 13;

bool motionDetected = false;

WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);

void setup() {
  Serial.begin(115200);
  Serial.println("Setting things up.");

  enableInterrupt();
  delay(1000);

  pinMode(STATUS_LED, OUTPUT);

  connectWifi();

  configureOTA();

  mqttClient.setServer(mqtt_server, mqtt_port);

  initCameraServer();

  Serial.println("Ready to serve!");
}

void loop() {
  ArduinoOTA.handle();

  if (!mqttClient.connected()) {
    connectMqtt();
  }
  mqttClient.loop();

  if (motionDetected) {
    mqttClient.publish("motion/status", "connected", false);
    mqttClient.publish(motion_topic, "ON", true);

    //digitalWrite(STATUS_LED, LED_ON);

    delay(1);

    //digitalWrite(STATUS_LED, LED_OFF);

    motionDetected = false;
  }
}
