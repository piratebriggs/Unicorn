using NSubstitute;
using Rainbow.Model;
using Sitecore.SecurityModel;
using System;
using Unicorn.Data;
using Unicorn.Loader;
using Xunit;

namespace Unicorn.Tests.Loader
{
	public class DeserializeFailureRetryerTests
	{
		[Fact]
		public void ShouldRetrieveItemRetry()
		{
			// Arrange
			var retryer = new DeserializeFailureRetryer();
			var item = CreateTestItem();
			var exception = new Exception();
			var callback = Substitute.For<Action<IItemData>>();
			SecurityState securityStatePre = SecurityStateSwitcher.CurrentValue;
			SecurityState? securityStatePost = null;

			retryer.AddItemRetry(item, exception);

			// Act
			retryer.RetryAll(Substitute.For<ISourceDataStore>(), x => { callback(x); securityStatePost = SecurityStateSwitcher.CurrentValue; }, callback);

			callback.Received()(item);                                      // Check that item is retried
			Assert.Equal(SecurityState.Default, securityStatePre);          // Check that Switcher value is Default before test
			Assert.True(securityStatePost.HasValue
				&& securityStatePost.Value == SecurityState.Disabled);      // Check that switcher is set to disabled while item is being retried
		}

		[Fact]
		public void ShouldRetrieveTreeRetry()
		{
			// Arrange
			var retryer = new DeserializeFailureRetryer();
			var item = CreateTestItem();
			var exception = new Exception();
			var callback = Substitute.For<Action<IItemData>>();
			SecurityState securityStatePre = SecurityStateSwitcher.CurrentValue;
			SecurityState? securityStatePost = null;

			retryer.AddTreeRetry(item, exception);

			// Act
			retryer.RetryAll(Substitute.For<ISourceDataStore>(), callback, x => { callback(x); securityStatePost = SecurityStateSwitcher.CurrentValue; });

			callback.Received()(item);                                      // Check that item is retried
			Assert.Equal(SecurityState.Default, securityStatePre);          // Check that Switcher value is Default before test
			Assert.True(securityStatePost.HasValue 
				&& securityStatePost.Value == SecurityState.Disabled);		// Check that switcher is set to disabled while item is being retried
		}

		[Fact]
		public void ShouldThrowIfItemRetryFails()
		{
			var retryer = new DeserializeFailureRetryer();
			var item = CreateTestItem();
			var exception = new Exception();

			retryer.AddItemRetry(item, exception);

			Action<IItemData> callback = delegate (IItemData x) { throw new Exception(); };

			Assert.Throws<DeserializationAggregateException>(() => retryer.RetryAll(Substitute.For<ISourceDataStore>(), callback, callback));
		}

		[Fact]
		public void ShouldThrowIfTreeRetryFails()
		{
			var retryer = new DeserializeFailureRetryer();
			var item = CreateTestItem();
			var exception = new Exception();

			retryer.AddTreeRetry(item, exception);

			Action<IItemData> callback = delegate (IItemData x) { throw new Exception(); };

			Assert.Throws<DeserializationAggregateException>(() => retryer.RetryAll(Substitute.For<ISourceDataStore>(), callback, callback));
		}

		private IItemData CreateTestItem()
		{
			return new ProxyItem { Path = "/test" };
		}
	}
}
