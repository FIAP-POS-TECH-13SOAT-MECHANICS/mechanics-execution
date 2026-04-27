using Mechanics.Tests.Behavior.Drivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll;
using System.Net;
using System.Text.Json;

namespace Mechanics.Tests.Behavior.Steps;

[Binding]
public class VehicleSteps(ScenarioContext ctx)
{
    private readonly VehicleDriver _driver = new();

    [Given("um atendente autenticado")]
    public static void GivenUmAtendenteAutenticado()
    {
        // autenticação gerenciada pela factory — step serve de documentação
    }

    [When("ele solicita a listagem de veículos")]
    public async Task WhenEleSolicitaAListagemDeVeiculos()
    {
        ctx["response"] = await _driver.ListAsync();
    }

    [Then("a resposta deve ser bem-sucedida")]
    public void ThenARespostaDeveSerBemSucedida()
    {
        var response = (HttpResponseMessage)ctx["response"];
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Then("a lista retornada deve estar vazia")]
    public async Task ThenAListaRetornadaDeveEstarVazia()
    {
        var response = (HttpResponseMessage)ctx["response"];
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var json = document.RootElement;

        var items = json.GetProperty("items");
        Assert.AreEqual(0, items.GetArrayLength());
        var totalCount = json.GetProperty("totalCount");
        Assert.AreEqual(0, totalCount.GetInt32());
    }
}
