static void IRAM_ATTR detectsMovement(void * arg) {
  motionDetected = true;
}

void enableInterrupt() {
  gpio_install_isr_service(ESP_INTR_FLAG_DEFAULT);

  esp_err_t err = gpio_isr_handler_add(GPIO_NUM_14, &detectsMovement, (void *) 1);
  if (err != ESP_OK) {
    Serial.printf("handler add failed with error 0x%x \r\n", err);
  }

  err = gpio_set_intr_type(GPIO_NUM_14, GPIO_INTR_POSEDGE);
  if (err != ESP_OK) {
    Serial.printf("set intr type failed with error 0x%x \r\n", err);
  }
}
