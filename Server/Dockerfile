FROM mcr.microsoft.com/dotnet/aspnet:10.0.1
ARG TARGETPLATFORM
ARG TARGETARCH
ARG TARGETVARIANT
EXPOSE ${ASPNETCORE_HTTP_PORTS}

RUN apt -y update && apt -y install curl wget unzip
RUN \
  if [ "$TARGETARCH" = "amd64" ]; \
    then export ARCH=x64; \
    else export ARCH=$TARGETARCH; \
    fi; \
  wget -q https://github.com/almir1904/remotely/releases/latest/download/Remotely_Server_Linux-${ARCH}.zip && \
  mkdir -p /temp && \
  mkdir -p /app && \
  unzip -o Remotely_Server_Linux-${ARCH}.zip -d /temp && \
  mv /temp/Remotely_Server_Linux-${ARCH}/* /app/ && \
  rm -rf /temp/Remotely_Server_Linux-${ARCH}.zip
  
WORKDIR /app
ENTRYPOINT ["dotnet", "Remotely_Server.dll"]
HEALTHCHECK --interval=5m --timeout=3s \
  CMD curl -f http://localhost:${ASPNETCORE_HTTP_PORTS}/api/healthcheck || exit 1
