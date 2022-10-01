FROM ubuntu:jammy

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://*:5000"

RUN \
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
  dpkg -i packages-microsoft-prod.deb && \
  rm packages-microsoft-prod.deb

RUN \
  apt-get -y update && \
  apt-get -y install \
  apt-utils \
  wget \
  apt-transport-https \
  unzip \
  acl \
  libssl1.0
  

RUN \
  apt-get -y install aspnetcore-runtime-6.0

RUN \
  mkdir -p /var/www/remotely && \
  mkdir /config && \
  wget -q https://github.com/almir1904/remotely/releases/latest/download/Remotely_Server_Linux-x64.zip && \
  unzip -o Remotely_Server_Linux-x64.zip -d /var/www/remotely && \
  rm Remotely_Server_Linux-x64.zip

RUN \
  mkdir -p /remotely-data && \
  sed -i 's/DataSource=Remotely.db/DataSource=\/remotely-data\/Remotely.db/' /var/www/remotely/appsettings.json

VOLUME "/remotely-data"

WORKDIR /var/www/remotely

COPY DockerMain.sh /

RUN chmod 755 /DockerMain.sh

ENTRYPOINT ["/DockerMain.sh"]
