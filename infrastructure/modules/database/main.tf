resource "aws_db_subnet_group" "this" {
  name       = var.name
  subnet_ids = var.subnet_ids
}

resource "aws_security_group" "this" {
  name   = "${var.name}-db"
  vpc_id = var.vpc_id
}

resource "aws_rds_cluster" "this" {
  cluster_identifier          = var.name
  engine                      = "aurora-postgresql"
  engine_mode                 = "provisioned"
  engine_version              = var.engine_version
  database_name               = "floe"
  master_username             = "floe"
  manage_master_user_password = true
  db_subnet_group_name        = aws_db_subnet_group.this.name
  vpc_security_group_ids      = [aws_security_group.this.id]
  storage_encrypted           = true
  skip_final_snapshot         = true # false
  # deletion_protected = true
  # final_snapshot_identifier = "${var.name}-final"

  serverlessv2_scaling_configuration {
    min_capacity             = var.min_capacity
    max_capacity             = var.max_capacity
    seconds_until_auto_pause = var.min_capacity == 0 ? 600 : null
  }
}

resource "aws_rds_cluster_instance" "writer" {
  identifier          = "${var.name}-writer"
  cluster_identifier  = aws_rds_cluster.this.id
  instance_class      = "db.serverless"
  engine              = aws_rds_cluster.this.engine
  engine_version      = aws_rds_cluster.this.engine_version
  publicly_accessible = length(var.admin_cidrs) > 0 # Only made "public" if someone can connect directly (not another AWS service)
}

resource "aws_vpc_security_group_ingress_rule" "admin" {
  for_each          = toset(var.admin_cidrs)
  security_group_id = aws_security_group.this.id
  cidr_ipv4         = each.value
  from_port         = 5432
  to_port           = 5432
  ip_protocol       = "tcp"
}
