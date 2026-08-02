// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using NUnit.Framework;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class ProcessorChainTests
    {
        sealed class DoubleProcessor : IProcessor<int, int>
        {
            public int Process(int input) => input * 2;
        }

        sealed class IntToStringProcessor : IProcessor<int, string>
        {
            public string Process(int input) => input.ToString();
        }

        sealed class ExclaimProcessor : IProcessor<string, string>
        {
            public string Process(string input) => input + "!";
        }

        sealed class CountingProcessor : IProcessor<int, int>
        {
            public int Calls { get; private set; }

            public int Process(int input)
            {
                Calls++;
                return input;
            }
        }

        [Test]
        public void CombinedProcessor_RunsFirstThenSecond()
        {
            CombinedProcessor<int, int, string> combined =
                new CombinedProcessor<int, int, string>(new DoubleProcessor(), new IntToStringProcessor());

            Assert.That(combined.Process(4), Is.EqualTo("8"));
        }

        [Test]
        public void Start_ThenRun_AppliesSingleProcessor()
        {
            ProcessorChain<int, int> chain = ProcessorChain<int, int>.Start(new DoubleProcessor());
            Assert.That(chain.Run(3), Is.EqualTo(6));
        }

        [Test]
        public void Then_ChainsAcrossTypeChanges()
        {
            ProcessorChain<int, string> chain = ProcessorChain<int, int>
                .Start(new DoubleProcessor())
                .Then(new IntToStringProcessor());

            Assert.That(chain.Run(5), Is.EqualTo("10"));
        }

        [Test]
        public void Then_ChainsThreeStages()
        {
            ProcessorChain<int, string> chain = ProcessorChain<int, int>
                .Start(new DoubleProcessor())
                .Then(new IntToStringProcessor())
                .Then(new ExclaimProcessor());

            Assert.That(chain.Run(6), Is.EqualTo("12!"));
        }

        [Test]
        public void Compile_ProducesEquivalentDelegate()
        {
            ProcessorChain<int, string> chain = ProcessorChain<int, int>
                .Start(new DoubleProcessor())
                .Then(new IntToStringProcessor());

            ProcessorDelegate<int, string> compiled = chain.Compile();

            Assert.That(compiled(7), Is.EqualTo("14"));
            Assert.That(compiled(7), Is.EqualTo(chain.Run(7)));
        }

        [Test]
        public void Run_IsRepeatable()
        {
            ProcessorChain<int, int> chain = ProcessorChain<int, int>.Start(new DoubleProcessor());
            Assert.That(chain.Run(1), Is.EqualTo(2));
            Assert.That(chain.Run(2), Is.EqualTo(4));
            Assert.That(chain.Run(3), Is.EqualTo(6));
        }

        // Building the chain must not execute it -- work happens only on Run.
        [Test]
        public void BuildingChain_DoesNotInvokeProcessors()
        {
            CountingProcessor counter = new CountingProcessor();
            ProcessorChain<int, int> chain = ProcessorChain<int, int>
                .Start(counter)
                .Then(new DoubleProcessor());

            Assert.That(counter.Calls, Is.EqualTo(0));

            chain.Run(1);
            Assert.That(counter.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Compile_DoesNotInvokeProcessors()
        {
            CountingProcessor counter = new CountingProcessor();
            ProcessorChain<int, int> chain = ProcessorChain<int, int>.Start(counter);

            ProcessorDelegate<int, int> compiled = chain.Compile();
            Assert.That(counter.Calls, Is.EqualTo(0));

            compiled(1);
            Assert.That(counter.Calls, Is.EqualTo(1));
        }
    }
}
