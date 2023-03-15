#!/bin/bash

echo "Entered main script."

RemotelyData=/remotely-data

AppSettingsVolume=/remotely-data/appsettings.json
AppSettingsSrc=/app/appsettings.json

AppSettingsVolume=/remotely-data/Remotely.db
AppSettingsSrc=/app/Remotely.db

if [ ! -f "$DBSettingsVolume" ]; then
	echo "Copying RemotelyDB to volume."
	cp "$DBettingsSrc" "$DBSettingsVolume"
fi

if [ -f "$DBSettingsSrc" ]; then
	rm "$DBSettingsSrc"
fi

if [ ! -f "$AppSettingsVolume" ]; then
	echo "Copying appsettings.json to volume."
	cp "$AppSettingsSrc" "$AppSettingsVolume"
fi

if [ -f "$AppSettingsSrc" ]; then
	rm "$AppSettingsSrc"
fi

ln -s "$AppSettingsVolume" "$AppSettingsSrc"
ln -s "$DBSettingsVolume" "$DBSettingsSrc"

echo "Starting Remotely server."
exec /usr/bin/dotnet /app/Remotely_Server.dll
