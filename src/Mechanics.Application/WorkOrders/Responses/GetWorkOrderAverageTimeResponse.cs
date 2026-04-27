namespace Mechanics.Application.WorkOrders.Responses;

/// <summary>
///     Representa o tempo médio total estimado para execução dos serviços de uma ordem de serviço.
/// </summary>
public class GetWorkOrderAverageTimeResponse
{
    /// <summary>
    ///     Identificador da ordem de serviço.
    /// </summary>
    public required Guid WorkOrderId { get; init; }

    /// <summary>
    ///     Soma dos averageTime
    /// </summary>
    public required int TotalAverageTime { get; init; }
}
