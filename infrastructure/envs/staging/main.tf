provider "aws" {
  region = "us-west-2"
  default_tags {
    tags = { Project = "floe", Environment = "staging" }
  }
}

locals {
  name = "floe-staging"
}

data "aws_vpc" "default" {
  default = true
}

data "aws_subnets" "default" {
  filter {
    name = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
  filter {
    name = "default-for-az"
    values = ["true"]
  }
}

data "aws_ecr_repository" "api" {
  name = "floe-api"
}

data "aws_ecr_repository" "migrate" {
  name = "floe-migrate"
}

module "database" {
  source = "../../modules/database"
  name = local.name
  vpc_id = data.aws_vpc.default.id
  subnet_ids = data.aws_subnets.default.ids
  engine_version = var.engine_version
  min_capacity = 0
  max_capacity = 2
}

module "service" {
  source = "../../modules/service"
  name = local.name
  vpc_id = data.aws_vpc.default.id
  subnet_ids = data.aws_subnets.default.ids
  image = "${data.aws_ecr_repository.api.repository_url}:${var.image_tag}"
  migrate_image = "${data.aws_ecr_repository.migrate.repository_url}:${var.image_tag}"
  certificate_arn = var.certificate_arn
  db_endpoint = module.database.endpoint
  db_secret_arn = module.database.secret_arn
}

resource "aws_vpc_security_group_ingress_rule" "db_from_tasks" {
  security_group_id = module.database.security_group_id
  referenced_security_group_id = module.service.task_security_group_id
  ip_protocol = "tcp"
  from_port = 5432
  to_port = 5432
}