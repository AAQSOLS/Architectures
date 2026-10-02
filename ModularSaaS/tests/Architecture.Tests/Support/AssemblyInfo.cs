using Xunit;

// Mono.Cecil reads lazily and is not thread-safe: run architecture tests sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
