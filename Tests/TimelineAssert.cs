using NUnit.Framework;
using PeriStuff;
using System;
using System.Collections.Generic;

namespace Tests {
	public class TimelineAssert<T> : IDisposable where T : IEquatable<T> {
		public readonly Timeline<T> Timeline;
		Queue<(EventType type, T value, string? message)> expectations = new();
		
		public TimelineAssert ( Timeline<T> timeline ) {
			Timeline = timeline;

			Timeline.EventStarted += onEventStarted;
			Timeline.EventEnded += onEventEnded;
			Timeline.EventReverted += onEventReverted;
			Timeline.EventRewound += onEventRewound;
		}

		public void Expect ( EventType type, T value, string? message = null ) {
			expectations.Enqueue(( type, value, message ));
		}

		public void AssertFinished () {
			if ( expectations.Count != 0 ) {
				Assert.Fail( $"Expected all timeline events to fire, but they did not. Next expected event: {expectations.Peek().type} | {expectations.Peek().value}" );
			}
		}

		public void Dispose () {
			AssertFinished();

			Timeline.EventStarted -= onEventStarted;
			Timeline.EventEnded -= onEventEnded;
			Timeline.EventReverted -= onEventReverted;
			Timeline.EventRewound -= onEventRewound;
		}

		private void onEventRewound ( Timeline<T>.Entry e ) {
			next( EventType.Rewound, e.Value );
		}

		private void onEventReverted ( Timeline<T>.Entry e ) {
			next( EventType.Reverted, e.Value );
		}

		private void onEventEnded ( Timeline<T>.Entry e ) {
			next( EventType.Ended, e.Value );
		}

		private void onEventStarted ( Timeline<T>.Entry e ) {
			next( EventType.Started, e.Value );
		}

		void next ( EventType type, T value ) {
			if ( !expectations.TryDequeue( out var expectation ) ) {
				Assert.Fail( $"An unexpected event of type {type} and value {value} was rised" );
			}

			Assert.That( expectation.type == type && expectation.value.Equals( value ), expectation.message );
		}
	}

	public enum EventType {
		/// <inheritdoc cref="Timeline{T}.EventStarted"/>
		Started,
		/// <inheritdoc cref="Timeline{T}.EventEnded"/>
		Ended,
		/// <inheritdoc cref="Timeline{T}.EventReverted"/>
		Reverted,
		/// <inheritdoc cref="Timeline{T}.EventRewound"/>
		Rewound
	}
}
