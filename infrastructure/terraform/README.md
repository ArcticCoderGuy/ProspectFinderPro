# Terraform Infrastructure

This directory will contain Terraform configurations for cloud infrastructure.

## Planned Structure
- `main.tf` - Main Terraform configuration
- `variables.tf` - Input variables
- `outputs.tf` - Output values
- `providers.tf` - Provider configurations
- `modules/` - Reusable Terraform modules
  - `database/` - Database infrastructure
  - `networking/` - VPC and networking
  - `compute/` - Application hosting

## Supported Providers
- Azure (primary target)
- AWS (secondary)
- Google Cloud Platform (future)

## Usage
```bash
# Initialize
terraform init

# Plan
terraform plan -var-file="environments/dev.tfvars"

# Apply  
terraform apply -var-file="environments/dev.tfvars"
```