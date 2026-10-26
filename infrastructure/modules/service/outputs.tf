output "cluster_name" {
  value = aws_ecs_cluster.this.name
}
output "service_name" {
  value = aws_ecs_service.api.name
}
output "migrate_task_family" {
  value = aws_ecs_task_definition.migrate.family
}
output "task_security_group_id" {
  value = aws_security_group.tasks.id
}
output "alb_dns_name" {
  value = aws_lb.this.dns_name
}
output "migrate_task_definition_arn" {
  value = aws_ecs_task_definition.migrate.arn
}