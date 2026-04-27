# Kubernetes e Helm chart

O chart do projeto é responsável por criar os recursos no kubernetes, rodar as migrações do banco de dados e iniciar 2 pods, com escalonamento para até 10 réplicas.

Ao instalar o chart, os seguintes hooks são executados:

1. `cluster-secret-store`: resposável por importar as credenciais do Secrets Manager
2. `db-secret`: secrets para o banco de dados
3. `migrations-job`: executa as migrações do banco de dados

Somente após os hooks terem rodado com sucesso é que os demais recursos são provisionados:

- `email-secret`
- `keys-secret`
- `config-map`
- `service`
- `deployment`
- `hpa`
- `ingress`

O comando abaixo faz deploy em DEV usando a tag `latest`:

```powershell
$repositoryUrl = aws ecr describe-repositories --repository-names fiap-mechanics-dev-cr --query "repositories[0].repositoryUri" --output text
helm upgrade --install fiap-mechanics ./k8s --set image.repository=$repositoryUrl
```

## Requisitos

O projeto utiliza [External Secrets Operator (ESO)](https://external-secrets.io/latest/introduction/overview/) para sincronizar as credenciais do banco de dados e servidor SMTP do Secrets Manager da AWS, portanto é importante já ter feito a seguinte configuração antes de instalar o chart do projeto:

1. Instalar ESO:
   - [Instale via Helm](https://external-secrets.io/latest/introduction/getting-started/#option-1-install-from-chart-repository)
   - Aguarde os pods estarem prontos
2. Criar uma secret no namespace `external-secrets` chamada `aws-credentials` com os dados da AWS Academy:
   - `access-key-id`
   - `secret-access-key`
   - `session-token`

```powershell
helm install external-secrets external-secrets `
  --repo https://charts.external-secrets.io `
  --namespace external-secrets `
  --create-namespace `
  --wait
kubectl create secret generic aws-credentials `
  --namespace external-secrets `
  --from-literal=access-key-id="$(aws configure get aws_access_key_id)" `
  --from-literal=secret-access-key="$(aws configure get aws_secret_access_key)" `
  --from-literal=session-token="$(aws configure get aws_session_token)" `
  --dry-run=client `
  --output yaml | kubectl apply -f -
```

Para o HPA, é necessário também instalar o [Metrics Server](https://artifacthub.io/packages/helm/metrics-server/metrics-server):

```powershell
helm install metrics-server metrics-server `
  --repo https://kubernetes-sigs.github.io/metrics-server/ `
  --namespace kube-system `
  --set args[0]="--kubelet-insecure-tls" `
  --set args[1]="--kubelet-preferred-address-types=InternalIP"
```

## Parâmetros

O chart possui os seguintes valores:

| Parâmetro                 | Descrição                                             | Padrão                   |
|---------------------------|-------------------------------------------------------|--------------------------|
| app.name                  | Nome do projeto                                       | fiap-mechanics           |
| app.version               | Versão do projeto                                     | 1.0.0                    |
| app.env                   | Ambiente (dev, stg ou prod)                           | dev                      |
| app.port                  | Porta de saída (service)                              | 5000                     |
| app.runMigrationsOnUpdate | Executa migrações ao atualizar ou somente ao instalar | true                     |
| app.baseUrl               | URL do projeto para envio de notificações (e-mails)   | http://localhost:5000    |
| image.repository          | Repositório da imagem Docker                          |                          |
| image.tag                 | Tag da imagem Docker                                  | latest                   |
| email.smtpServer          | Endereço do servidor SMTP                             | mailpit-smtp             |
| email.smtpPort            | Porta do servidor SMTP                                | 25                       |
| email.senderName          | Nome do remetente de e-mail                           | FIAP Mechanics           |
| email.senderAddress       | Endereço do remetente de e-mail                       | postmaster@mechanics.com |

O ESO busca as secrets seguindo o seguinte padrão:
- `$AppName-$AppEnv-email`:
  - `userName`: nome de usuário do servidor SMTP, por exemplo `fulano.tal@mechanics.com`
  - `password`: senha do servidor SMTP
- `$AppName-$AppEnv-database`:
  - `value`: connectionString do banco de dados
- `$AppName-$AppEnv-jwt/public-key`:
  - `value`: chave pública para assinatura de tokens JWT
