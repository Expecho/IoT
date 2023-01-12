FROM mcr.microsoft.com/azurespheresdk:latest AS dev

ARG FUNCTION_CONNECTION_STRING

RUN echo '#define MY_CONNECTION_STRING "'${FUNCTION_CONNECTION_STRING}'"'
RUN echo '#define MY_CONNECTION_STRING "'${FUNCTION_CONNECTION_STRING}'"' > AzureSphere/MySphere/secrets.h
RUN chmod +r AzureSphere/MySphere/secrets.h

RUN cat AzureSphere/MySphere/secrets.h

FROM dev AS build
COPY AzureSphere/MySphere/ /src/

RUN chmod +x /src/buildscript.sh
RUN /src/buildscript.sh

WORKDIR /out

RUN cmake -G "Ninja" -DCMAKE_TOOLCHAIN_FILE="/opt/azurespheresdk/CMakeFiles/AzureSphereToolchain.cmake" \
    -DAZURE_SPHERE_TARGET_API_SET="15" -DCMAKE_BUILD_TYPE="Release" "/src"
ENTRYPOINT [ "ninja" ]
