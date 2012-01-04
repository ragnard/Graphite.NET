using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Graphite.Policy
{
	// https://github.com/phatboyg/Magnum/blob/master/src/Magnum/Policies

	// Copyright 2007-2008 The Apache Software Foundation.
	//
	// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
	// this file except in compliance with the License. You may obtain a copy of the
	// License at
	//
	// http://www.apache.org/licenses/LICENSE-2.0
	//
	// Unless required by applicable law or agreed to in writing, software distributed
	// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
	// CONDITIONS OF ANY KIND, either express or implied. See the License for the
	// specific language governing permissions and limitations under the License.

	public class ExceptionPolicy
	{
		private readonly Action<Action> _policy;

		public ExceptionPolicy(Action<Action> policy)
		{
			if (policy == null) throw new ArgumentNullException("policy");
			_policy = policy;
		}

		[DebuggerNonUserCode]
		public void Do(Action action)
		{
			_policy(action);
		}

		[DebuggerNonUserCode]
		public TResult Do<TResult>(Func<TResult> action)
		{
			TResult result = default(TResult);

			_policy(() => { result = action(); });

			return result;
		}

		public static PolicyBuilder<ExceptionHandler> InCaseOf<TException>()
		where TException : Exception
		{
			return PolicyBuilder.For<EventHandler>(ex => ex is TException);
		}

		public static PolicyBuilder<ExceptionHandler> InCaseOf<TException1, TException2>()
			where TException1 : Exception
			where TException2 : Exception
		{
			return PolicyBuilder.For<EventHandler>(ex => (ex is TException1) || (ex is TException2));
		}

		public static PolicyBuilder<ExceptionHandler> InCaseOf<TException1, TException2, TException3>()
			where TException1 : Exception
			where TException2 : Exception
			where TException3 : Exception
		{
			return PolicyBuilder.For<EventHandler>(ex => (ex is TException1) || (ex is TException2) || (ex is TException3));
		}
	}

	public static class PolicyBuilder
	{
		public static PolicyBuilder<ExceptionHandler> For<T>(ExceptionHandler condition)
		{
			return new PolicyBuilder<ExceptionHandler>(condition);
		}
	}

	public class PolicyBuilder<T>
	{
		public PolicyBuilder(T condition)
		{
			Condition = condition;
		}

		public T Condition { get; private set; }
	}

	public interface IExceptionPolicy
	{
		void Execute(Action action);
	}

	public delegate bool ExceptionHandler(Exception ex);

	public static class FinallyPolicy
	{
		/// <summary>
		/// A finally-wrapper around some other policy.
		/// </summary>
		/// <param name="previousPolicy">Some other policy of your choice.</param>
		/// <param name="canHandle">Whether the exception was handled and should not be furthered.</param>
		/// <returns>An executable exception policy. Usage <code>policy.Do(() => socket.Send( ... ))</code></returns>
		public static ExceptionPolicy Finally(this ExceptionPolicy previousPolicy, Func<Exception, bool> canHandle)
		{
			return new ExceptionPolicy(action =>
			{
				try
				{
					previousPolicy.Do(action);
				}
				catch (Exception e)
				{
					if (!canHandle(e))
						throw;
				}
			});
		}

	}

	public static class RetryPolicy
	{
		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			return Retry(builder, 0);
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, int retryCount)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			return new ExceptionPolicy(action => ImplementPolicy(action, RetryImmediately(builder.Condition, retryCount)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, int retryCount, Action<Exception, int> retryAction)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (retryAction == null) throw new ArgumentNullException("retryAction");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryImmediately(builder.Condition, retryCount, retryAction)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, Action<Exception> retryAction)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (retryAction == null) throw new ArgumentNullException("retryAction");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryImmediately(builder.Condition, retryAction)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, IEnumerable<TimeSpan> intervals, Action<Exception, TimeSpan> retryAction)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (intervals == null) throw new ArgumentNullException("intervals");
			return new ExceptionPolicy(action => ImplementPolicy(action, RetryInterval(builder.Condition, intervals, retryAction)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, IEnumerable<TimeSpan> intervals)
		{
			if (builder == null) throw new ArgumentNullException("builder");
			if (intervals == null) throw new ArgumentNullException("intervals");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryInterval(builder.Condition, intervals)));
		}

		private static void ImplementPolicy(Action action, Func<Exception, bool> isHandled)
		{
			while (true)
			{
				try
				{
					action();
					return;
				}
				catch (Exception ex)
				{
					if (!isHandled(ex))
						throw;
				}
			}
		}

		private static Func<Exception, bool> RetryImmediately(ExceptionHandler isHandled, int retryCount)
		{
			return RetryImmediately(isHandled, retryCount, (ex, c) => { });
		}

		private static Func<Exception, bool> RetryImmediately(ExceptionHandler isHandled, int retryCount, Action<Exception, int> retryAction)
		{
			int failureCount = 0;

			return x =>
			{
				failureCount++;

				if (!isHandled(x))
					return false;

				if (failureCount > retryCount)
					return false;

				retryAction(x, failureCount);

				return true;
			};
		}

		private static Func<Exception, bool> RetryImmediately(ExceptionHandler isHandled, Action<Exception> retryAction)
		{
			return x =>
			{
				if (!isHandled(x))
					return false;

				retryAction(x);

				return true;
			};
		}

		private static Func<Exception, bool> RetryInterval(ExceptionHandler isHandled, IEnumerable<TimeSpan> intervals)
		{
			return RetryInterval(isHandled, intervals, (ex, c) => { });
		}

		private static Func<Exception, bool> RetryInterval(ExceptionHandler isHandled, IEnumerable<TimeSpan> intervals, Action<Exception, TimeSpan> retryAction)
		{
			IEnumerator<TimeSpan> enumerator = intervals.GetEnumerator();

			return x =>
			{
				if (!isHandled(x))
					return false;

				if (!enumerator.MoveNext())
					return false;

				TimeSpan interval = enumerator.Current;

				retryAction(x, interval);

				Thread.Sleep(interval);

				return true;
			};
		}
	}
}