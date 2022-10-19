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
#include <ArduinoOTA.h>
#include "secrets.h"

#define wifi_ssid wifi_ssid_secret
#define wifi_password wifi_password_secret

#define ESP_INTR_FLAG_DEFAULT 0

void setup() {
  Serial.begin(115200);
  Serial.println("Setting things up.");

  connectWifi();

  configureOTA();

  initCameraServer();

  Serial.println("Ready to serve!");
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("Reconnecting to WiFi...");
    WiFi.disconnect();
    connectWifi();
  }
  
  ArduinoOTA.handle();

  delay(1);
}
