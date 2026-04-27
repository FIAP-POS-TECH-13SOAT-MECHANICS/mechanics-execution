param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

# terraform
terraform -chdir="./infra" init -backend-config="key=$environment.tfstate" -reconfigure

# EKS cluster (se existir)
$clusterName = "fiap-mechanics-$environment-cluster"
if ((aws eks list-clusters --region us-east-1 --query "clusters" | ConvertFrom-Json).Contains($clusterName)) {
    aws eks update-kubeconfig --name fiap-mechanics-$environment-cluster --region us-east-1
}

Write-Host -ForegroundColor Green "Environment updated to '$environment'."
