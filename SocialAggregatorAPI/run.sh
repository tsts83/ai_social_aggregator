#!/bin/bash

# Load env variables from .env file
set -a
source .env
set +a

# Pull the latest image from DockerHub
docker pull tsts83/socialaggregatorapi:latest

# Run the container from DockerHub with necessary environment variables
docker run --env-file .env -it --rm -p 8080:8080 -v ${PWD}:/app tsts83/socialaggregatorapi:latest

    # sh -c "echo 'Starting application...'; dotnet SocialAggregatorAPI.dll"
