using Mechanics.Tests.Behavior.Drivers;
using Reqnroll;
using System.Net;

namespace Mechanics.Tests.Behavior.Steps;

[Binding]
public class HealthCheckSteps(ScenarioContext ctx)
{
    private readonly HealthCheckDriver _driver = new();

    [Given("a aplicação está em execução")]
    public static void GivenAAplicacaoEstaEmExecucao()
    {
        // a factory já garante isso — step serve de documentação viva
    }

    [When("o health check é solicitado")]
    public async Task WhenOHealthCheckESolicitado()
    {
        var response = await _driver.GetHealthAsync();
        ctx["response"] = response;
    }

    [Then("a resposta deve indicar que o serviço está saudável")]
    public async Task ThenARespostaDeveIndicarQueOServicoEstaSaudavel()
    {
        var response = (HttpResponseMessage)ctx["response"];
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
    }
}
