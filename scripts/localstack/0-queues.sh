#!/bin/bash

QUEUES=(
  'fiap-mechanics-dev-budget-created'
  'fiap-mechanics-dev-budget-revised'
  'fiap-mechanics-dev-work-order-created'
  'fiap-mechanics-dev-work-order-status-changed'
)

for QUEUE in "${QUEUES[@]}"; do
  awslocal sqs create-queue --queue-name "$QUEUE" > /dev/null
  echo "Queue '$QUEUE' created."
done
