using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using System.Linq;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant.Entities;
public class EAssistantConversation : Conversation
{
    public static Func<Conversation, Question, Answer, Conversation> Map()
    {
        var lookup = new Dictionary<Guid, Conversation>();
        return (conversation, question, answer) =>
        {
            if (!lookup.TryGetValue(conversation.id, out var existingConversation))
            {
                existingConversation = conversation;
                existingConversation.questions = new List<Question>();
                lookup[conversation.id] = existingConversation;
            }

            var existingQuestion = existingConversation.questions.FirstOrDefault(q => q.id == question.id);
            if (existingQuestion == null)
            {
                existingQuestion = question;
                existingConversation.questions.Add(existingQuestion);
            }

            if (answer != null && !existingQuestion.answer.id.Equals(answer.id)) existingQuestion.answer = answer;

            return existingConversation;
        };
    }
}
