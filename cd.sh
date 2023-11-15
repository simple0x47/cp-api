#!/bin/bash

CP_ORGANIZATION_SSH_PRIVATE_KEY=$(bws secret get "7ad8e4fa-477a-425b-8377-b09e00885ab3" --access-token "$SECRETS_MANAGER_ACCESS_TOKEN" | jq -r '.value')

mkdir ~/.ssh
echo "$CP_ORGANIZATION_SSH_PRIVATE_KEY" > ~/.ssh/cp_organization_ssh_private_key
chmod 600 ~/.ssh/cp_organization_ssh_private_key

eval `ssh-agent`
ssh-add ~/.ssh/*

ssh -o StrictHostKeyChecking=accept-new $SSH_CONNECTION  'cd /home/ubuntu/cp && ./run.sh'