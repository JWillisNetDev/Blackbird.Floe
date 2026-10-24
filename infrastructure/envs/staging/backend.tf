terraform {
  required_version = ">= 1.10"
  required_providers {
    aws = { source = "hashicorp/aws", version = "~> 6.0" }
  }
  backend "s3" {
    bucket       = "floe-tfstate"
    key          = "prod/terraform.tfstate"
    region       = "us-west-2"
    use_lockfile = true
  }
}