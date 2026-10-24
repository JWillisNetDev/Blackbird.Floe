data "aws_region" "current" {}

locals {
  container_port = 8080
  https = var.certificate_arn != null

  db_env = [
    { name = "ConnectionStrings__floe", value = "Host=${var.db_endpoint};Database=floe;Username=floe;SSL Mode=Require" },
  ]

  db_secrets = [
    { name = "Database__Password", valueFrom = "${var.db_secret_arn}:password::" },
  ]

  log_config = {
    logDriver = "awslogs"
    options = {
      awslog-group          = aws_cloudwatch_log_group.this.name
      awslogs-region        = data.aws_region.current
      awslogs-stream-prefix = "ecs"
    }
  }
}

resource "aws_ecs_cluster" "this" {
  name = var.name
}

resource "aws_cloudwatch_log_group" "this" {
  name              = "/ecs/${var.name}"
  retention_in_days = 28
}

ephemeral "random_password" "jwt" {
  length  = 64
  special = false
}

resource "aws_secretsmanager_secret" "jwt" {
  name = "${var.name}/jwt-signing-key"
}

resource "aws_secretsmanager_secret_version" "jwt" {
  secret_id                = aws_secretsmanager_secret.jwt.id
  secret_string_wo         = ephemeral.random_password.jwt.result
  secret_string_wo_version = 1 # Bump to rotate password
}

/* Security Groups */

resource "aws_security_group" "alb" {
  name   = "${var.name}-alb"
  vpc_id = var.vpc_id
}

resource "aws_vpc_security_group_ingress_rule" "alb_https" {
  count = local.https ? 1 : 0
  security_group_id = aws_security_group.alb.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 443
  to_port           = 443
}

resource "aws_vpc_security_group_ingress_rule" "alb_http" {
  security_group_id = aws_security_group.alb.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 80
  to_port           = 80
}

resource "aws_vpc_security_group_egress_rule" "alb_to_tasks" {
  security_group_id            = aws_security_group.alb.id
  referenced_security_group_id = aws_security_group.tasks.id
  ip_protocol                  = "tcp"
  from_port                    = local.container_port
  to_port                      = local.container_port
}

resource "aws_security_group" "tasks" {
  name   = "${var.name}-tasks"
  vpc_id = var.vpc_id
}

resource "aws_vpc_security_group_ingress_rule" "tasks_from_alb" {
  security_group_id            = aws_security_group.tasks.id
  referenced_security_group_id = aws_security_group.alb.id
  ip_protocol                  = "tcp"
  from_port                    = local.container_port
  to_port                      = local.container_port
}

/* Public IP rule - Can communicate to ECR, Secrets Manager, CloudWatch directly over AWS network  */
resource "aws_vpc_security_group_egress_rule" "tasks_all" {
  security_group_id = aws_security_group.tasks.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
}

/* ALB - Automated Load Balancer */

resource "aws_lb" "this" {
  name               = var.name
  load_balancer_type = "application"
  subnets            = var.subnet_ids
  security_groups    = [aws_security_group.alb.id]
}

resource "aws_lb_target_group" "api" {
  name                 = "${var.name}-api"
  vpc_id               = var.vpc_id
  port                 = local.container_port
  protocol             = "HTTP"
  target_type          = "ip"
  deregistration_delay = 30

  health_check {
    path    = "/health"
    matcher = "200"
  }
}

resource "aws_lb_listener" "https" {
  count = local.https ? 1 : 0
  load_balancer_arn = aws_lb.this.arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = var.certificate_arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.this.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = local.https ? "redirect" : "forward"
    target_group_arn = local.https ? null : aws_lb_target_group.api.arn

    dynamic "redirect" {
      for_each = local.https ? [1] : []
      content {
        port = "443"
        protocol = "HTTPS"
        status_code = "HTTP_301"
      }
    }
  }
}

/* Task Definitions */

resource "aws_ecs_task_definition" "api" {
  family                   = "${var.name}-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = var.cpu
  memory                   = var.memory
  execution_role_arn       = aws_iam_role.execution.arn
  task_role_arn            = aws_iam_role.task.arn

  runtime_platform {
    operating_system_family = "LINUX"
    cpu_architecture        = "ARM64"
  }

  container_definitions = jsonencode([{
    name         = "api"
    image        = var.image
    essential    = true
    portMappings = [{ containerPort = local.container_port }]
    environment = concat(local.db_env, [
      { name = "ASPNETCORE_ENVIRONMENT", value = vars.environment },
      { name = "ASPNETCORE_FORWARDEDHEADERS_ENABLED", value = "true" },
    ])
    secrets = concat(local.db_secrets, [
      { name = "Jwt__SigningKey", valueFrom = aws_secretsmanager_secret.jwt.arn }
    ])
    logConfiguration = local.log_config
  }])
}

resource "aws_ecs_task_definition" "migrate" {
  family = "${var.name}-migrate"
  requires_compatibilities = ["FARGATE"]
  network_mode = "awsvpc"
  cpu = 256
  memory = 512
  execution_role_arn = aws_iam_role.execution.arn

  runtime_platform {
    operating_system_family = "LINUX"
    cpu_architecture = "ARM64"
  }

  container_definitions = jsonencode([{
    name = "migrate"
    image = var.migrate_image
    essential = true
    environment = local.db_env
    secrets = local.db_secrets
    logConfiguration = local.log_config
  }])
}

/* Service */

resource "aws_ecs_service" "api" {
  name = "${var.name}-api"
  cluster = aws_ecs_cluster.this.id
  task_definition = aws_ecs_task_definition.api.arn
  desired_count = var.desired_count
  launch_type = "FARGATE"
  health_check_grace_period_seconds = 30

  network_configuration {
    subnets = var.subnet_ids
    security_groups = [aws_security_group.tasks.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name = "api"
    container_port = local.container_port
  }

  deployment_circuit_breaker {
    enable = true
    rollback = true
  }

  depends_on = [ aws_lb_listener.https ]
}
