#!/bin/bash

# Load env variables from .env file
set -a
source .env
set +a

# Build the Docker image using Dockerfile.dev
docker build -f Dockerfile.dev -t tsts83/socialaggregatorapi:dev .

# Run the container with hot reload
docker run --env-file .env -it --rm -p 8080:8080 -v ${PWD}:/app tsts83/socialaggregatorapi:dev



    # --entrypoint /bin/bash \
