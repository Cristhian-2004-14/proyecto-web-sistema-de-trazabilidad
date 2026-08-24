using POS.Shared.DTOs;

namespace POS.Server.Services;

public interface IReceptionService
{
    Task<List<ReceptionResponse>> GetAllAsync();
    Task<ReceptionResponse?> GetByIdAsync(int id);
    Task<ReceptionResponse> CreateAsync(CreateReceptionRequest request, int authenticatedUserId);
    Task<ReceptionResponse?> ConfirmAsync(int id, int authenticatedUserId);
}

public sealed class BusinessValidationException(string message) : Exception(message);
