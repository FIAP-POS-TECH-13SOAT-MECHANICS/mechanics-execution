Feature: Health Check
Como operador da plataforma
Quero verificar a saúde da aplicação
Para garantir que o serviço está operacional

Scenario: Aplicação saudável
    Given a aplicação está em execução
    When o health check é solicitado
    Then a resposta deve indicar que o serviço está saudável
