using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using maxbl4.RfidCheckpointService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests
{
    public class ObservableTests
    {
        [Fact]
        public void Observable_error_without_handlers()
        {
            var subject = new Subject<int>();
            subject.Subscribe(x => throw new ArgumentException());
            Assert.Throws<ArgumentException>(() => subject.OnNext(1));
            Assert.Throws<ArgumentException>(() => subject.OnNext(1));
            
            subject = new Subject<int>();

            string s = "";
            var errors = 0;

            subject.Catch((Exception err) =>
            {
                errors++; return subject; }).Subscribe(x =>
            {
                s += x;
                throw new ArgumentException();
            });

            Assert.Throws<ArgumentException>(() => subject.OnNext(1));
            subject.OnNext(2);
            s.ShouldBe("1");
        }
        
        [Fact]
        public void Should_continue_observable_after_many_exceptions()
        {
            var logger = new BufferLogger();
            var subject = new Subject<int>();

            var s = "";
            subject.Subscribe(x =>
                Safe.Execute(() =>{
                    s += x;
                    throw new ArgumentException();
                }, logger));

            subject.OnNext(1);
            subject.OnNext(2);
            s.ShouldBe("12");
            logger.Messages.Count.ShouldBe(2);
            
        }
    }
}