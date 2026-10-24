data "aws_iam_policy_document" "ecs_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "execution" {
  name = "${var.name}-execution"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume.json
}

resource "aws_iam_role_policy_attachment" "execution" {
  role = aws_iam_role.execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

data "aws_iam_policy_document" "secrets" {
  statement {
    actions = ["secretsmanager:GetSecretValue"]
    resources = [var.db_secret_arn, aws_secretsmanager_secret.jwt.arn]
  }
}

resource "aws_iam_role_policy" "execution_secrets" {
  role = aws_iam_role.execution.id
  policy = data.aws_iam_policy_document.secrets.json
}

resource "aws_iam_role" "task" {
  name = "${var.name}-task"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume.json
}