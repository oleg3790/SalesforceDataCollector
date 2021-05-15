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