#ifndef AZUREFUNCTION
#define AZUREFUNCTION

#include <curl/curl.h>
#include <stdlib.h>
#include <stdio.h>
#include <applibs/log.h>
#include <applibs/gpio.h>
#include "secrets.h"

void ExtendWatchdogExpiry(void);

static void Send(char* sensor, double value)
{
	CURL* curl;
	CURLcode res;

	curl = curl_easy_init();

	if (curl) {
		/* First set the URL that is about to receive our POST. This URL can
		   just as well be a https:// URL if that is what should receive the
		   data. */
		curl_easy_setopt(curl, CURLOPT_URL, MY_CONNECTION_STRING);

		/* Now specify the POST data */
		char* postdata = (char*)malloc(50 * sizeof(char));
		sprintf(postdata, "%s=%.2f", sensor, value);
		curl_easy_setopt(curl, CURLOPT_POSTFIELDS, postdata);
		curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, false);

		/* Perform the request, res will get the return code */
		res = curl_easy_perform(curl);
		/* Check for errors */
		if (res != CURLE_OK)
		{
			Log_Debug("curl_easy_perform() failed: %s\n",
				curl_easy_strerror(res));
		}
		else
		{
			ExtendWatchdogExpiry();
		}

		/* always cleanup */
		curl_easy_cleanup(curl);

		free(postdata);
	}
}
#endif
