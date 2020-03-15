#ifndef AZUREFUNCTION
#define AZUREFUNCTION

#include <curl/curl.h>
#include <stdlib.h>
#include <stdio.h>
#include <applibs/log.h>

static void Send(char* sensor, double value)
{
   CURL* curl;
   CURLcode res;

   curl = curl_easy_init();

   if (curl) {
      /* First set the URL that is about to receive our POST. This URL can
         just as well be a https:// URL if that is what should receive the
         data. */
      curl_easy_setopt(curl, CURLOPT_URL, "https://azuresphere-exp.azurewebsites.net/api/AzureSphereTrigger?code=979eR8ajLcv2HECtDX5pKee89rOSA3og3avmu7HVagcBHNyfsD2dbg==");

      /* Now specify the POST data */
      char* postdata = (char*)malloc(50 * sizeof(char));
      sprintf(postdata, "%s=%.2f", sensor, value);
      curl_easy_setopt(curl, CURLOPT_POSTFIELDS, postdata);

      /* Perform the request, res will get the return code */
      res = curl_easy_perform(curl);
      /* Check for errors */
      if (res != CURLE_OK)
         Log_Debug("curl_easy_perform() failed: %s\n",
            curl_easy_strerror(res));

      /* always cleanup */
      curl_easy_cleanup(curl);

      free(postdata);
   }
}
#endif