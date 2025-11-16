using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;

/// <summary>
/// Defines service operations for managing <see cref="Answer"/> entities.
/// </summary>
/// <remarks>
/// This service provides CRUD operations for answer entities through the base service interface.
/// Answers represent the responses generated for questions in a conversation.
/// </remarks>
public interface IServiceAnswer : IServiceBase<Answer>
{
}
