using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace TimingServices.Input
{
    public class InputServiceSettings
    {
    }

    public class Input
    {
        private static long nextSequence = 1;
        public long Sequence { get; }
        public object Data { get; }

        public Input(object data, long sequence = 0)
        {
            Data = data;
            Sequence = sequence > 0 ? sequence : nextSequence++;
        }
    }

    public class InputService
    {
        public Task EnableRfid()
        {
            return Task.CompletedTask;
        }
        
        public Task DisableRfid()
        {
            return Task.CompletedTask;
        }

        public Task<InputServiceSettings> GetSettings()
        {
            return Task.FromResult(new InputServiceSettings());
        }
        
        public Task SetSettings(InputServiceSettings settings)
        {
            return Task.CompletedTask;
        }

        public Task ProvideManualInput(string input)
        {
            return Task.CompletedTask;
        }

        public Task<IObservable<Input>> Subscribe(long fromSequence)
        {
            return Task.FromResult((IObservable<Input>)new Subject<Input>());
        }
    }
}