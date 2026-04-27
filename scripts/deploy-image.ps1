param ([string]$environment)
. "$PSScriptRoot/get-environment.ps1"
$environment = Get-Environment $environment

$serviceName = (Get-Content "$PSScriptRoot/../src/Mechanics.Api/appsettings.json" | ConvertFrom-Json).AppInfo.Name

Write-Host -ForegroundColor Yellow "Retrieving information for project '$serviceName'..."
$accountId = aws sts get-caller-identity --query "Account" --output text
$repositoryUrl = "$accountId.dkr.ecr.us-east-1.amazonaws.com/fiap-mechanics-$environment/$serviceName-cr"
$tag = (new-guid).Guid

aws eks update-kubeconfig --name fiap-mechanics-$environment-cluster --region us-east-1

$password = aws ecr get-login-password --region us-east-1
docker login --username AWS --password $password $repositoryUrl

Write-Host -ForegroundColor Yellow "Building and pushing image with tag '$tag'..."
docker build -t "$serviceName" -t "$($repositoryUrl):$tag" -t "$($repositoryUrl):latest" .
docker push "$($repositoryUrl):$tag"
docker push "$($repositoryUrl):latest"

Write-Host -ForegroundColor Yellow "Deploying application..."
helm upgrade --install $serviceName ./k8s `
    --namespace $serviceName `
    --create-namespace `
    --set image.repository=$repositoryUrl `
    --set image.tag="$tag" `
    --set app.name=$serviceName `
    --set app.env=$environment
