using POS.Shared.DTOs;

namespace POS.Server.Services;

public interface IProductionLotService
{
    Task<List<ProductionLotResponse>> GetAllAsync();
    Task<ProductionLotResponse?> GetByIdAsync(int id);
    Task<ProductionLotResponse> CreateAsync(CreateProductionLotRequest request, int authenticatedUserId);
    Task<ProductionLotResponse?> ConsumeAsync(int id, MaterialConsumptionRequest request, int authenticatedUserId);
    Task<ProductionLotResponse?> StartAsync(int id, int authenticatedUserId);
    Task<ProductionLotResponse?> FinishAsync(int id, FinishProductionLotRequest request, int authenticatedUserId);
}
