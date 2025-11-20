using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;

/// <summary>
/// Defines service operations for managing <see cref="Conversation"/> entities.
/// </summary>
/// <remarks>
/// This service provides CRUD operations for conversation entities through the base service interface.
/// A conversation represents a thread of questions and answers within a user session, containing
/// an ordered collection of questions with their corresponding answers.
/// </remarks>
public interface IServiceConversation : IServiceBase<Conversation>
{
}
