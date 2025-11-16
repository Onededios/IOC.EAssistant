using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;


/// <summary>
/// Defines service operations for managing <see cref="Session"/> entities.
/// </summary>
/// <remarks>
/// This service provides CRUD operations for session entities through the base service interface.
/// A session represents a user's interaction period with the AI assistant and contains one or more
/// conversations. Sessions are used to maintain context and history across multiple conversation threads.
/// </remarks>
public interface IServiceSession : IServiceBase<Session>
{
}
