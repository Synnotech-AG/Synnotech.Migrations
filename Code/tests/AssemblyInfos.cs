/*
 * This file can be used by test projects that perform integration tests with third-party systems
 * to disable test parallelization.
 */

using Xunit;

[assembly:CollectionBehavior(DisableTestParallelization = true)]