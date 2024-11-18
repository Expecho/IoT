void connectWifi() {
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(wifi_ssid);

//  WiFi.setHostname("IoT ESP32-Cam");
  WiFi.begin(wifi_ssid, wifi_password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
  Serial.println("WiFi signal strength: ");
  Serial.println(WiFi.RSSI());
}

void connectMqtt() {
  while (!mqttClient.connected()) {
    Serial.print("Attempting MQTT connection...");
    if (mqttClient.connect("ESP8266Client", mqtt_user, mqtt_password, "motion/status", 0, 0, "disconnected")) {
      Serial.println("connected");
      mqttClient.publish("motion/status", "connected", false);
    } else {
      Serial.print("failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" trying again in 5 seconds");
      delay(5000);
    }
  }
}
