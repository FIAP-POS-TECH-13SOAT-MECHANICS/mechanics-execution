# Scripts para deploy

1. Certifique-se de ter instalado as ferramentas necessárias:
    - AWS CLI
    - Helm
2. Configure suas credenciais na AWS:
    - Usando AWS CLI: `aws configure`; ou,
    - Colando no arquivo de configuração (se já existir): `notepad $ENV:USERPROFILE\.aws\credentials`;
3. Utilize o script `deploy-image.ps1` para subir a aplicação.
    - Repita o comando para gerar uma nova release.
4. Utilize o script `new-token.ps1` para gerar um token de acesso ou `invoke-getToken.ps1` para obter um token através
   do endpoint de login.

O comando abaixo faz deploy no ambiente DEV com base no nome do serviço especificado
em [appsettings.json](../src/Mechanics.Api/appsettings.json).
Execute na raiz do projeto.

```powershell
.\scripts\deploy-image.ps1 dev
```

Para alternar entre ambientes, use o script `set-environment.ps1`.
Os scripts são idempotentes, isto é, podem ser executados múltiplas vezes.

## Permissão de execução de scripts

No Windows, a execução de scripts do Powershell vem desabilitada por padrão.

Para habilitar, abra um terminal como administrador e utilize o
comando [Set-ExecutionPolicy](https://learn.microsoft.com/pt-br/powershell/module/microsoft.powershell.security/set-executionpolicy):

```powershell
Set-ExecutionPolicy Unrestricted
```
