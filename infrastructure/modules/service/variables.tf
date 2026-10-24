variable "name" {
    type = string
}
variable "vpc_id" {
  type = string
}
variable "subnet_ids" {
    type = list(string)
}
variable "image" {
    type = string
}
variable "migrate_image" {
  type = string
}
variable "certificate_arn" {
  type = string
  default = null
}
variable "db_endpoint" {
  type = string
}
variable "db_secret_arn" {
  type = string
}

variable "environment" {
    type = string
    default = "Staging"
}

variable "desired_count" {
    type = number
    default = 1
}

variable "cpu" {
    type = number
    default = 512
}

variable "memory" {
  type = number
  default = 1024
}