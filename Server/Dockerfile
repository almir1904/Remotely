FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy

ARG TARGETPLATFORM
ARG TARGETARCH
ARG TARGETVARIANT

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://*:5000"

WORKDIR /app

RUN \
  apt-get -y update && \
  apt-get -y install \
  apt-utils \
  wget \
  apt-transport-https \
  unzip \
  acl \
  curl \
  libssl1.0

RUN \
  if [ "$TARGETARCH" = "amd64" ]; \
    then export ARCH=x64; \
    else export ARCH=$TARGETARCH; \
    fi; \
  mkdir -p /var/www/remotely && \
  mkdir /config && \
  wget -q https://github.com/almir1904/remotely/releases/latest/download/Remotely_Server_Linux-${ARCH}.zip && \
  unzip -o Remotely_Server_Linux-${ARCH}.zip -d /app && \
  rm Remotely_Server_Linux-${ARCH}.zip


RUN \
  mkdir -p /remotely-data && \
  sed -i 's/DataSource=Remotely.db/DataSource=\/remotely-data\/Remotely.db/' ./appsettings.json

VOLUME "/remotely-data"

WORKDIR /app

COPY DockerMain.sh /

RUN chmod 755 /DockerMain.sh

ENTRYPOINT ["/DockerMain.sh"]
HEALTHCHECK CMD curl --fail http://localhost:5000/ || exit 1

