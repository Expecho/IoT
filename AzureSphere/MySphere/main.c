#include <errno.h>
#include <signal.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h> 
#include <time.h>
#include <unistd.h>
#include <stdio.h>
#include <math.h>

// applibs_versions.h defines the API struct versions to use for applibs APIs.
#include "applibs_versions.h"
#include "epoll_timerfd_utilities.h"
#include "i2c.h"
#include "hw/avnet_mt3620_sk.h"

#include "deviceTwin.h"
//#include "azure_iot_utilities.h"
//#include "connection_strings.h"
#include "build_options.h"

#include <applibs/log.h>
#include <applibs/i2c.h>
//#include <applibs/gpio.h>
#include <applibs/adc.h>
#include <applibs/wificonfig.h>


extern int accelTimerFd;

// Support functions.
static void TerminationHandler(int signalNumber);
static int InitPeripheralsAndHandlers(void);
static void ClosePeripheralsAndHandlers(void);

int epollFd = -1;

static int adcControllerFd = -1;
static int adcPollTimerFd = -1;

static int sampleBitCount = -1;
static float sampleMaxVoltage = 2.5f;

// Termination state
volatile sig_atomic_t terminationRequired = false;

/// <summary>
///     Signal handler for termination requests. This handler must be async-signal-safe.
/// </summary>
static void TerminationHandler(int signalNumber)
{
   // Don't use Log_Debug here, as it is not guaranteed to be async-signal-safe.
   terminationRequired = true;
}

static void AdcPollingEventHandler(EventData* eventData)
{
   if (ConsumeTimerFdEvent(adcPollTimerFd) != 0) {
      terminationRequired = true;
      return;
   }

   uint32_t value;
   int result = ADC_Poll(adcControllerFd, 0, &value);
   if (result < -1) {
      Log_Debug("ADC_Poll failed with error: %s (%d)\n", strerror(errno), errno);
      terminationRequired = true;
      return;
   }

   // get voltage (2.5adc_reading/4096)
   // divide by 3650 (3.65 kohm) to get current (A)
   // multiply by 1000000 to get uA
   // divide by 0.1428 to get Lux (based on fluorescent light Fig. 1 datasheet)
   // divide by 0.5 to get Lux (based on incandescent light Fig. 1 datasheet)
   float light_sensor = ((float)value * 2.5 / 4095) * 1000000 / (3650 * 0.1428);
   Log_Debug("ALS-PT19: Ambient Light[Lux] : %.2f", light_sensor);
}

// event handler data structures. Only the event handler field needs to be populated.
static EventData adcPollingEventData = { .eventHandler = &AdcPollingEventHandler };

/// <summary>
///     Set up SIGTERM termination handler, initialize peripherals, and set up event handlers.
/// </summary>
/// <returns>0 on success, or -1 on failure</returns>
static int InitPeripheralsAndHandlers(void)
{
   struct sigaction action;
   memset(&action, 0, sizeof(struct sigaction));
   action.sa_handler = TerminationHandler;
   sigaction(SIGTERM, &action, NULL);

   epollFd = CreateEpollFd();
   if (epollFd < 0) {
      return -1;
   }

   if (initI2c() == -1) {
      return -1;
   }

   adcControllerFd = ADC_Open(AVNET_AESMS_ADC_CONTROLLER0);
   if (adcControllerFd < 0) {
      Log_Debug("ADC_Open failed with error: %s (%d)\n", strerror(errno), errno);
      return -1;
   }

   sampleBitCount = ADC_GetSampleBitCount(adcControllerFd, AVNET_AESMS_ADC_CONTROLLER0);
   if (sampleBitCount == -1) {
      Log_Debug("ADC_GetSampleBitCount failed with error : %s (%d)\n", strerror(errno), errno);
      return -1;
   }
   if (sampleBitCount == 0) {
      Log_Debug("ADC_GetSampleBitCount returned sample size of 0 bits.\n");
      return -1;
   }

   int result = ADC_SetReferenceVoltage(adcControllerFd, AVNET_AESMS_ADC_CONTROLLER0,
      sampleMaxVoltage);
   if (result < 0) {
      Log_Debug("ADC_SetReferenceVoltage failed with error : %s (%d)\n", strerror(errno), errno);
      return -1;
   }

   // Set up a timer to poll the adc controller
   struct timespec adcControllerCheckPeriod = { 1, 0 };
   adcPollTimerFd =
      CreateTimerFdAndAddToEpoll(epollFd, &adcControllerCheckPeriod, &adcPollingEventData, EPOLLIN);
   if (adcPollTimerFd < 0) {
      return -1;
   }

   return 0;
}

/// <summary>
///     Close peripherals and handlers.
/// </summary>
static void ClosePeripheralsAndHandlers(void)
{
   Log_Debug("Closing file descriptors.\n");

   closeI2c();
   CloseFdAndPrintError(epollFd, "Epoll");
   CloseFdAndPrintError(adcPollTimerFd, "adcPoll");
   CloseFdAndPrintError(adcControllerFd, "adc");
}

/// <summary>
///     Main entry point for this application.
/// </summary>
int main(int argc, char* argv[])
{
   // Variable to help us send the version string up only once
   bool networkConfigSent = false;
   char ssid[128];
   uint32_t frequency;
   char bssid[20];

   // Clear the ssid array
   memset(ssid, 0, 128);

   Log_Debug("Version String: %s\n", argv[1]);
   Log_Debug("Avnet Starter Kit Simple Reference Application starting.\n");
   if (InitPeripheralsAndHandlers() != 0) {
      terminationRequired = true;
   }

   // Use epoll to wait for events and trigger handlers, until an error or SIGTERM happens
   while (!terminationRequired) {
      if (WaitForEventAndCallHandler(epollFd) != 0) {
         terminationRequired = true;
      }

      WifiConfig_ConnectedNetwork network;
      int result = WifiConfig_GetCurrentNetwork(&network);
      if (result < 0) {
         Log_Debug("INFO: Not currently connected to a WiFi network.\n");
      }
      else {

         frequency = network.frequencyMHz;
         snprintf(bssid, JSON_BUFFER_SIZE, "%02x:%02x:%02x:%02x:%02x:%02x",
            network.bssid[0], network.bssid[1], network.bssid[2],
            network.bssid[3], network.bssid[4], network.bssid[5]);

         if ((strncmp(ssid, (char*)&network.ssid, network.ssidLength) != 0) || !networkConfigSent) {

            memset(ssid, 0, 128);
            strncpy(ssid, network.ssid, network.ssidLength);
            Log_Debug("SSID: %s\n", ssid);
            Log_Debug("Frequency: %dMHz\n", frequency);
            Log_Debug("bssid: %s\n", bssid);
            networkConfigSent = true;
         }
      }
   }

   ClosePeripheralsAndHandlers();
   Log_Debug("Application exiting.\n");
   return 0;
}
