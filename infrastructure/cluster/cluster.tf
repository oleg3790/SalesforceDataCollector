data "aws_ecr_repository" "app_repo" {
  name    = "salesforce-data-collector"
}

resource "aws_secretsmanager_secret" "app_token_secret" {
  name                    = "salesforce-data-collector-connection-string"
  description             = "DB Connection String"
  recovery_window_in_days = 0

  tags = local.tags
}

resource "aws_secretsmanager_secret_version" "app_token_secret_version" {
  secret_id     = aws_secretsmanager_secret.app_token_secret.arn
  secret_string = var.connectionString
}

resource "aws_cloudwatch_log_group" "app_log_group" {
  name  = "salesforce-data-collector-log-group"

  tags = local.tags
}

resource "aws_ecs_cluster" "app_cluster" {
  name                = "salesforce-data-collector-cluster"
  capacity_providers  = ["FARGATE"]

  tags = local.tags
}

resource "aws_ecs_task_definition" "app_task_def" {
  family                      = "salesforce-data-collector-task"
  task_role_arn               = aws_iam_role.app_task_role.arn
  execution_role_arn          = aws_iam_role.app_task_execution_role.arn
  requires_compatibilities    = ["FARGATE"]
  network_mode                = "awsvpc"
  cpu                         = 512
  memory                      = 1024
  container_definitions       = <<EOF
[
  {
    "name": "salesforce-data-collector",
    "image": "${data.aws_ecr_repository.app_repo.repository_url}:latest",
    "essential": true,
    "secrets": [
      { "name": "ConnectionStrings:default", "valueFrom": "${aws_secretsmanager_secret.app_token_secret.arn}" }
    ],
    "logConfiguration": {
      "logDriver": "awslogs",
      "options": {
        "awslogs-group": "${aws_cloudwatch_log_group.app_log_group.name}",
        "awslogs-region": "${local.region}",
        "awslogs-stream-prefix": "salesforce-data-collector"
      }
    }
  }
]
EOF
  tags = local.tags
}