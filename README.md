# TransmissionExtras
![Docker Image Version (latest semver)](https://img.shields.io/docker/v/huszky/transmission-extras) ![Docker Image Size (tag)](https://img.shields.io/docker/image-size/huszky/transmission-extras/latest) ![Docker Pulls](https://img.shields.io/docker/pulls/huszky/transmission-extras)

## Extra functionalities for [Transmission](https://transmissionbt.com/)

This project aims to augment Transmission with extra capabilities.

## Installation

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
    environment:
      Transmission__Url: http://localhost:9095
      Transmission__User: transmission-user # Optional. Only required when authentication is enabled
      Transmission__Password: MySuperStrongPassword1234! # Optional. Only required when authentication is enabled

      RemoveTorrents__DryRun: false # Optional. By default, it's true
      RemoveTorrents__DeleteData: true # Optional. By default, it's false
      RemoveTorrents__CheckInterval: "00:30:00" # Optional. By default, it's 1 hour
      RemoveTorrents__RemoveAfter: "10:00:00:00" # Remve torrents after 10 days of seeding

      VerifyTorrents__DryRun: false # Optional. By default, it's true
      VerifyTorrents__CheckInterval: "00:30:00" # Optional. By default, it's 1 hour
```

## Features

### Torrent removal

Automatically remove torrents after a set amount of time.

### Torrent verification

Automatically verify torrents which need verification.
