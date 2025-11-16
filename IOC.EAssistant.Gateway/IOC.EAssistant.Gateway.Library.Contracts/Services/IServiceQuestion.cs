using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Library.Contracts.Services;

/// <summary>
/// Defines service operations for managing <see cref="Question"/> entities.
/// </summary>
/// <remarks>
/// This service provides CRUD operations for question entities through the base service interface.
/// Questions represent user input within a conversation, and each question is associated with
/// a corresponding answer. Questions include metadata such as token count and indexing information
/// to maintain conversation order and context.
/// </remarks>
public interface IServiceQuestion : IServiceBase<Question>
{
}
