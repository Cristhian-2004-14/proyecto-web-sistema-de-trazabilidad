using POS.Shared.DTOs;
namespace POS.Server.Services;
public interface IDispatchService { Task<List<DispatchResponse>> GetAllAsync(); Task<DispatchResponse?> GetByIdAsync(int id); Task<DispatchResponse> CreateAsync(CreateDispatchRequest request,int userId); Task<DispatchResponse?> ConfirmAsync(int id,int userId); }
