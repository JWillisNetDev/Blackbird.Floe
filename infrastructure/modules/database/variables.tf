variable "name" {
  type = string
}
variable "vpc_id" {
  type = string
}
variable "subnet_ids" {
  type = list(string)
}
variable "engine_version" {
  type = string
}
variable "max_capacity" {
  type = number
}
variable "min_capacity" {
  type = number
}

/* For defining who (by address) can connect directly to the DB cluster */
variable "admin_cidrs" {
  type    = list(string)
  default = []
}
