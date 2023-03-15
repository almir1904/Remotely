FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://*:5000"


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
  mkdir -p /var/www/remotely && \
  mkdir /config && \
  arch=$(arch | sed s/aarch64/arm64/ | sed s/x86_64/x64/) && \
  wget -q https://github.com/almir1904/remotely/releases/latest/download/Remotely_Server_Linux-${arch}.zip && \
  unzip -o Remotely_Server_Linux-${arch}.zip -d /var/www/remotely && \
  rm Remotely_Server_Linux-${arch}.zip


RUN \
  mkdir -p /remotely-data && \
  sed -i 's/DataSource=Remotely.db/DataSource=\/remotely-data\/Remotely.db/' /var/www/remotely/appsettings.json

VOLUME "/remotely-data"

WORKDIR /var/www/remotely

COPY DockerMain.sh /

RUN chmod 755 /DockerMain.sh

ENTRYPOINT ["/DockerMain.sh"]
HEALTHCHECK CMD curl --fail http://localhost:5000/ || exit 1
