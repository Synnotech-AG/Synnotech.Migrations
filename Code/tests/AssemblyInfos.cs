/*
 * This file can be used by test projects that perform integration tests with third-party systems
 * to disable running tests in parallel.
 */

using Xunit;

[assembly:CollectionBehavior(DisableTestParallelization = true)]