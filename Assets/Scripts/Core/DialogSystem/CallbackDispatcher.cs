using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dialogues.Runtime
{
    public class CallbackDispatcher : ICallbackDispatcher
    {
        public UniTask DispatchAsync(string callback)
        {
            return UniTask.CompletedTask;
        }
    }
}