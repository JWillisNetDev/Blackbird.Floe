#!/usr/bin/env bash
set -euo pipefail
out() { terraform output -raw "$1"; }

cluster=$(out cluster_name)
subnets=$(terraform output -json subnet_ids | jq -r 'join(",")')

task=$(aws ecs run-task --cluster "$cluster" --launch-type FARGATE \
    --task-definition "$(out migrate_task_family)" \
    --network-configuration "awsvpcConfiguration={subnets=[$subnets],securityGroups=[$(out task_security_group_id)],assignPublicIp=ENABLED}" \
    --query 'tasks[0].taskArn' --output text)

aws ecs wait tasks-stopped --cluster "$cluster" --tasks "$task"

code=$(aws ecs desribe-tasks --cluster "$cluster" --tasks "$task" \
    --query 'tasks[0].containers[0].exitCode' --output text)

[[ "$code" == "0" ]] || { echo "Migration failed (exit code $code)"; exit 1; }
