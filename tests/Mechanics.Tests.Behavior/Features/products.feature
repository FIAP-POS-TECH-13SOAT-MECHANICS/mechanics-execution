Feature: Produtos
Como um atendente autenticado
Quero listar os produtos cadastrados
Para consultar o estoque de peças e serviços

Scenario: Listagem de produtos sem cadastros
    Given um atendente autenticado
    When ele solicita a listagem de produtos
    Then a resposta deve ser bem-sucedida
    And a lista retornada deve estar vazia
