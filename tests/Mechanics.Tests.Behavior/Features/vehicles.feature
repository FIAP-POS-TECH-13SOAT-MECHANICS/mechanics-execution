Feature: Veículos
Como um atendente autenticado
Quero listar os veículos cadastrados
Para consultar o histórico de ordens de serviço

Scenario: Listagem de veículos sem cadastros
    Given um atendente autenticado
    When ele solicita a listagem de veículos
    Then a resposta deve ser bem-sucedida
    And a lista retornada deve estar vazia
