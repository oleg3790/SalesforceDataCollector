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

resource "aws_ecr_repository" "app_repo" {
  name    = "salesforce-data-collector"
  
  tags    = {
    Project = "salesforce-data-collector"
  }
}