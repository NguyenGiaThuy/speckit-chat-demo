#!/usr/bin/env bash
# Generate a dev RS256 key pair and output them as environment variables
# Usage: source ./scripts/gen-dev-keys.sh

set -euo pipefail

PRIVATE_KEY=$(openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 2>/dev/null)
PUBLIC_KEY=$(echo "$PRIVATE_KEY" | openssl rsa -pubout 2>/dev/null)

export JWT_PRIVATE_KEY="$PRIVATE_KEY"
export JWT_PUBLIC_KEY="$PUBLIC_KEY"

echo "JWT_PRIVATE_KEY and JWT_PUBLIC_KEY have been exported to the environment."
echo ""
echo "To use with docker-compose, add to a .env file:"
echo 'JWT_PRIVATE_KEY="'"$PRIVATE_KEY"'"'
echo 'JWT_PUBLIC_KEY="'"$PUBLIC_KEY"'"'
