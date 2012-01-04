using System;
using System.Diagnostics;
using Graphite.Policy;
using NUnit.Framework;

namespace Graphite.Tests
{
	public class ExceptionHandlerTests
	{
		[Test]
		public void handle_exception()
		{
			TryPolicy(true);
		}

		[Test]
		public void not_handling_exception()
		{
			Assert.Throws<ApplicationException>(() => TryPolicy(false));
		}

		static void TryPolicy(bool swallowIt)
		{
			var exceptionThrown = 0;
			var finallyCalled = 0;

			var handler = ExceptionPolicy.InCaseOf<ApplicationException>()
				.Retry(4)
				.Finally(ex =>
					{
						finallyCalled++;
						Debug.WriteLine(ex.ToString());
						return swallowIt;
						// I could retrow here...
					});

			handler.Do(() =>
				{
					exceptionThrown++;
					throw new ApplicationException("things");
				});

			Assert.That(exceptionThrown, Is.EqualTo(5)); // meaning 4 retries
			Assert.That(finallyCalled, Is.EqualTo(1));
		}
	}
}