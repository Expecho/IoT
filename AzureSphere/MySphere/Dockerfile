FROM mcr.microsoft.com/azurespheresdk:latest AS dev

FROM dev AS build
COPY AzureSphere/MySphere/ /src/

ARG MY_CONNECTION_STRING

ENV MY_CONNECTION_STRING_ENV=$MY_CONNECTION_STRING

RUN echo '#define MY_CONNECTION_STRING "'$MY_CONNECTION_STRING'"' > /src/secrets.h

RUN chmod +x /src/buildscript.sh
RUN /src/buildscript.sh

WORKDIR /out

RUN cmake -G "Ninja" -DCMAKE_TOOLCHAIN_FILE="/opt/azurespheresdk/CMakeFiles/AzureSphereToolchain.cmake" \
    -DAZURE_SPHERE_TARGET_API_SET="15" -DCMAKE_BUILD_TYPE="Release" "/src"
ENTRYPOINT [ "ninja" ]
