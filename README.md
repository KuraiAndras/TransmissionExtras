# TransmissionExtras
![Docker Image Version (latest semver)](https://img.shields.io/docker/v/huszky/transmission-extras) ![Docker Image Size (tag)](https://img.shields.io/docker/image-size/huszky/transmission-extras/latest) ![Docker Pulls](https://img.shields.io/docker/pulls/huszky/transmission-extras)

## Extra functionalities for [Transmission](https://transmissionbt.com/)

This project aims to augment Transmission with extra capabilities.

## Installation

docker-compose:

```yml
version: "3.8"

services:

  transmission:
    container_name: transmission
    image: lscr.io/linuxserver/transmission:latest
    environment:
      USER: transmission-user
      PASS: MySuperStrongPassword1234!
    ports:
      - 9095:9091

  transmission-extras:
    container_name: transmission-extras
    image: huszky/transmission-extras:latest
    depends_on:
      - transmission
    environment:
      Transmission__Url: http://localhost:9095
      Transmission__User: transmission-user # Optional. Only required when authentication is enabled
      Transmission__Password: MySuperStrongPassword1234! # Optional. Only required when authentication is enabled
      Transmission__RetryTimeout: 00:05:00 # Optional. Retry timeout for failed jobs. Defaults to 1 minute
    volumes:
      - ./transmission-extras/jobs.json:/app/jobs.json
```

Job configuration:
```json
[
    {
        "id": "remove-after-seed-time",
        "dryRun": true,
        "cron": "0 0 0/1 1/1 * ? *",
        "runOnStartup": true,
        "after": "11.00:00:00",
        "deleteData": true
    },
    {
        "id": "remove-after-added-time",
        "dryRun": true,
        "cron": "0 0 0/1 1/1 * ? *",
        "runOnStartup": true,
        "after": "15.00:00:00",
        "deleteData": true
    },
    {
        "id": "verify",
        "dryRun": true,
        "cron": "0 0 0/1 1/1 * ? *",
        "runOnStartup": true
    }
]
```

For creating cron schedules it is advised to use [CronMaker](http://www.cronmaker.com/).

For the elapsed time syntax refer to [this](https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse) documentation.

## Features

### Remove torrent after seed time

Automatically remove torrents after seeding set amount of time. Might not be accurate:
- https://github.com/transmission/transmission/issues/870
- https://github.com/linuxserver/docker-transmission/issues/262

### Remover torrent after added time

Automatically remove torrents after added date plus a set amount of time. Can be used to overcome the issues with seed time.

### Torrent verification

Automatically verify torrents which need verification.
