#!/bin/bash

QUEUES=(
  'fiap-mechanics-dev-customer-created'
)
AUTH_TOKEN='fiap-mechanics-dev-auth-token'

for QUEUE in "${QUEUES[@]}"; do
  awslocal sqs create-queue --queue-name "$QUEUE" > /dev/null
  echo "Queue '$QUEUE' created."
done

zip -j /tmp/auth-token.zip /etc/localstack/init/ready.d/auth-token.mjs
awslocal lambda create-function --function-name "$AUTH_TOKEN" \
  --runtime nodejs20.x --handler auth-token.handler \
  --zip-file fileb:///tmp/auth-token.zip \
  --role arn:aws:iam::000000000000:role/irrelevant > /dev/null
echo "Function '$AUTH_TOKEN' created."
