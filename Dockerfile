# syntax=docker/dockerfile:1.20

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet tool install --global dotnet-ef --version 8.0.25
ENV PATH="$PATH:/root/.dotnet/tools"

RUN curl -o /tmp/aws-rds-global.crt https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem

COPY --parents src/**/*.csproj .
RUN dotnet restore src/Mechanics.Api/Mechanics.Api.csproj --locked-mode

COPY src src
RUN dotnet build src/Mechanics.Api/Mechanics.Api.csproj --no-restore

RUN dotnet ef migrations bundle --project src/Mechanics.Infra.Data --startup-project src/Mechanics.Api --no-build --output /dist/efbundle
RUN dotnet publish src/Mechanics.Api/Mechanics.Api.csproj --no-restore -c Release -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra AS final

COPY --from=build /tmp/aws-rds-global.crt /usr/local/share/ca-certificates/aws-rds-global.crt
ENV SSL_CERT_FILE=/usr/local/share/ca-certificates/aws-rds-global.crt

ENV TZ=America/Sao_Paulo
ENV LANG=pt_BR.UTF-8 LANGUAGE=pt_BR:pt LC_ALL=pt_BR.UTF-8
EXPOSE 8080

WORKDIR /app
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "Mechanics.Api.dll"]
