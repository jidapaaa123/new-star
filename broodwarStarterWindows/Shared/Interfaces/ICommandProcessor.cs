using Shared.Models;

namespace Shared.Interfaces
{
    public interface ICommandProcessor
    {
        void EnqueueCommand(BotCommand command);
        void ProcessCommands(/* dependencies */);
    }
}
