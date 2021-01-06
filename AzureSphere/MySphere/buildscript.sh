#!/bin/sh
sed "s|MY_CONNECTION_STRING|$MY_CONNECTION_STRING_ENV|g" /src/azurefunction.h > /src/transformed.h
