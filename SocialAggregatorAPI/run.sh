#!/bin/bash

# Load env variables from .env file
set -a
source .env
set +a

docker run -it \
  -p 8080:8080 \
  -e DEFAULT_CONNECTION="$DEFAULT_CONNECTION" \
  -e JWT_KEY="$JWT_KEY" \
  -e NEWSDATA_API_KEY="$NEWSDATA_API_KEY" \
  -e HUGGINGFACE_API_KEY="$HUGGINGFACE_API_KEY" \
  -e ADMIN_API_KEY="$ADMIN_API_KEY" \
  -e MINIAPP_USER_PASSWORD="$MINIAPP_USER_PASSWORD" \
  tsts83/socialaggregatorapi:latest

    