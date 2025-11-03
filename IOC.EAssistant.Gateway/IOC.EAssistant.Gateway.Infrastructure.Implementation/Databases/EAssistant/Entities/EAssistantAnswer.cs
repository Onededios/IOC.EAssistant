using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant.Entities;
public class EAssistantAnswer : Answer
{
    public Func<Answer, Answer> Map()
    {
        return (answer) =>
        {
            return answer;
        };
    }
}