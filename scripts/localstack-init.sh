#!/bin/bash

QUEUES=(
  "fiap-mechanics-dev-customer-created"
)

for QUEUE in "${QUEUES[@]}"; do
  awslocal sqs create-queue --queue-name "$QUEUE"
done
