output "cluster_name" {
  value = module.service.cluster_name
}
output "service_name" {
  value = module.service.service_name
}
output "migrate_task_family" {
    value = module.service.migrate_task_family
}
output "task_security_group_id" {
  value = module.service.task_security_group_id
}
output "subnet_ids" {
  value = data.aws_subnets.default.ids
}
output "alb_dns_name" {
  value = module.service.alb_dns_name
}