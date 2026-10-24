terraform {
  required_version = ">= 1.11"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~>6.0"
    }
  }
}

provider "aws" {
  region = var.region
  allowed_account_ids = ["155419943746"]
}

variable "region" {
  default = "us-west-2"
}

variable "github_repo" {
  type    = string
  default = "JWillisNetDev/Blackbird.Floe"
}

data "aws_caller_identity" "current" {}

resource "aws_s3_bucket" "tfstate" {
  bucket = "floe-tfstate-${data.aws_caller_identity.current.account_id}"
}

resource "aws_s3_bucket_versioning" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_public_access_block" "tfstate" {
  bucket                  = aws_s3_bucket.tfstate.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_ecr_repository" "this" {
  for_each             = toset(["floe-api", "floe-migrate"])
  name                 = each.key
  image_tag_mutability = "IMMUTABLE"
  image_scanning_configuration {
    scan_on_push = true
  }
}

/* Expires all images in the ECR except for the 20 most-recent */
resource "aws_ecr_lifecycle_policy" "this" {
  for_each   = aws_ecr_repository.this
  repository = each.value.name
  policy = jsonencode({
    rules = [{
      rulePriority = 1
      selection    = { tagStatus = "any", countType = "imageCountMoreThan", countNumber = 20 }
      action       = { type = "expire" }
    }]
  })
}

/* Set up for automatic deployments with GitHub CI/CD pipelines */
resource "aws_iam_openid_connect_provider" "github" {
  url            = "https://token.actions.githubusercontent.com"
  client_id_list = ["sts.amazonaws.com"]
}

data "aws_iam_policy_document" "staging_trust" {
  statement {
    actions = ["sts:AssumeRoleWithWebIdentity"]
    principals {
      type        = "Federated"
      identifiers = [aws_iam_openid_connect_provider.github.arn]
    }
    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:aud"
      values   = ["sts.amazonaws.com"]
    }
    /* Limits use to the staging GitHub environment */
    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:sub"
      values   = ["repo:${var.github_repo}:environment:staging"]
    }
  }
}

resource "aws_iam_role" "ci_staging" {
  name               = "floe-ci-staging"
  assume_role_policy = data.aws_iam_policy_document.staging_trust.json
}

/* Give staging role Power User access (should be able to create most things) */
resource "aws_iam_role_policy_attachment" "ci_staging_power" {
  role       = aws_iam_role.ci_staging.name
  policy_arn = "arn:aws:iam::aws:policy/PowerUserAccess"
}

/* Also give the staging role iam access because Power User does not get it by default */
data "aws_iam_policy_document" "ci_staging_iam" {
  statement {
    actions = ["iam:*Role*", "iam:PassRole", "iam:GetPolicy*", "iam:ListPolicies"]
    resources = [
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/floe-staging-*",
      "arn:aws:iam::aws:policy/*",
    ]
  }
}

resource "aws_iam_role_policy" "ci_staging_iam" {
  role = aws_iam_role.ci_staging.id
  policy = data.aws_iam_policy_document.ci_staging_iam.json
}

output "state_bucket" { value = aws_s3_bucket.tfstate.bucket }
output "ci_staging_role" { value = aws_iam_role.ci_staging.arn }
output "ecr_registry" { value = split("/", aws_ecr_repository.this["floe-api"].repository_url)[0] }
