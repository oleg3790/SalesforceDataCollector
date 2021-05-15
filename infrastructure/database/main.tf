terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 2.70"
    }
  }
}

provider "aws" {
  profile = "default"
  region = local.region
}

locals {
  region = "us-east-2"
  tags = {
    Project = "salesforce-data-collector"
  }
}

resource "aws_db_instance" "data_collector" {
  identifier = "salesforce-data-collector"
  allocated_storage = 20
  storage_type = "gp2"
  engine = "postgres"
  engine_version = "12.4"
  instance_class = "db.t3.micro"
  publicly_accessible = true
  username = var.dbInstanceUsername
  password = var.dbInstancePassword
  skip_final_snapshot = true

  tags = local.tags
}