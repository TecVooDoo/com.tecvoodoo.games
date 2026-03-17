// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on Generic Processing Chains by Adam Myhre (adammyhre)

using System;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Generic processor interface. Transforms TIn to TOut.
    /// </summary>
    public interface IProcessor<in TIn, out TOut>
    {
        TOut Process(TIn input);
    }

    /// <summary>
    /// Delegate form of a processor for lightweight use cases.
    /// </summary>
    public delegate TOut ProcessorDelegate<in TIn, out TOut>(TIn input);

    /// <summary>
    /// Combines two processors into a single sequential processor: A -> B -> C.
    /// </summary>
    public class CombinedProcessor<A, B, C> : IProcessor<A, C>
    {
        readonly IProcessor<A, B> first;
        readonly IProcessor<B, C> second;

        public CombinedProcessor(IProcessor<A, B> first, IProcessor<B, C> second)
        {
            this.first = first;
            this.second = second;
        }

        public C Process(A input)
        {
            return second.Process(first.Process(input));
        }
    }

    /// <summary>
    /// Fluent processor chain. Build with ProcessorChain.Start(), chain with .Then(), execute with .Run().
    /// </summary>
    public class ProcessorChain<TIn, TOut>
    {
        readonly IProcessor<TIn, TOut> processor;

        ProcessorChain(IProcessor<TIn, TOut> processor)
        {
            this.processor = processor;
        }

        public static ProcessorChain<TIn, TOut> Start(IProcessor<TIn, TOut> processor)
        {
            return new ProcessorChain<TIn, TOut>(processor);
        }

        public ProcessorChain<TIn, TNext> Then<TNext>(IProcessor<TOut, TNext> next)
        {
            CombinedProcessor<TIn, TOut, TNext> combined = new CombinedProcessor<TIn, TOut, TNext>(processor, next);
            return new ProcessorChain<TIn, TNext>(combined);
        }

        public TOut Run(TIn input)
        {
            return processor.Process(input);
        }

        public ProcessorDelegate<TIn, TOut> Compile()
        {
            return input => processor.Process(input);
        }
    }
}
