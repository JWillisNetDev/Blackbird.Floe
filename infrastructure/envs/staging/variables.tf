variable "region" {
  type = string
  default = "us-west-2"
}

variable "image_tag" {
  type = string
}
variable "certificate_arn" {
  type = string
  default = null
}
variable "engine_version" {
  type = string
}
variable "admin_cidrs" {
  type = list(string)
  default = []
  sensitive = true
}