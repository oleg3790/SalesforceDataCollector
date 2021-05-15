  
data "aws_db_instance" "data_collector" {
  db_instance_identifier = "salesforce-data-collector"
}

resource "aws_iam_role" "app_task_execution_role" {
  name = "data-collector-task-execution-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = { "Service": [ "ecs-tasks.amazonaws.com" ] }
      },
    ]
  })
}

resource "aws_iam_role" "app_task_role" {
  name = "data-collector-task-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = { "Service": [ "ecs-tasks.amazonaws.com" ] }
      },
    ]
  })
}

resource "aws_iam_role_policy" "app_task_execution_role_policy" {
  name = "data-collector-task-execution-role-policy"
  role = aws_iam_role.app_task_execution_role.id

  policy = <<POLICY
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": "secretsmanager:GetSecretValue",
      "Resource": "${aws_secretsmanager_secret.app_token_secret.arn}"
    }
  ]
}
POLICY
}

resource "aws_iam_role_policy" "app_task_role_policy" {
  name = "data-collector-task-role-policy"
  role = aws_iam_role.app_task_role.id

  policy = <<POLICY
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": "rds-data:*",
      "Resource": "${data.aws_db_instance.data_collector_db_instance.db_instance_arn}"
    }
  ]
}
POLICY
}