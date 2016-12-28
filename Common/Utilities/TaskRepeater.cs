﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Utilities
{
	/// <summary>
	/// Репитер
	/// </summary>
    public static class TaskRepeater
    {
        public static Task Interval(TimeSpan pollInterval, Action action, CancellationToken token, bool runImmediately = false)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    if (runImmediately)
                    {
                        for (;;)
                        {
                            action();

                            if (token.WaitCancellationRequested(pollInterval))
                                break;
                        }
                    }
                    else
                    {
                        for (;;)
                        {
                            if (token.WaitCancellationRequested(pollInterval))
                                break;

                            action();
                        }
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    public static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}
