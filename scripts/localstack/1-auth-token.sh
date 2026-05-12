#!/bin/bash

AUTH_TOKEN='fiap-mechanics-dev-auth-token'

zip -j /tmp/auth-token.zip /etc/localstack/init/ready.d/1-auth-token.mjs
awslocal lambda create-function --function-name "$AUTH_TOKEN" \
  --runtime nodejs20.x --handler auth-token.handler \
  --zip-file fileb:///tmp/auth-token.zip \
  --role arn:aws:iam::000000000000:role/irrelevant > /dev/null
echo "Function '$AUTH_TOKEN' created."
