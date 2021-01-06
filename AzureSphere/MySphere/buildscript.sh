#!/bin/sh
sed -i "s|MY_CONNECTION_STRING|$MY_CONNECTION_STRING_ENV|g" /src/azurefunction.h
