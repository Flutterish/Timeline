using System;
using System.Collections.Generic;

namespace PeriStuff {
	/// <summary>
	/// A timeline with a current time where entries have duration that can be seeked to a given time
	/// </summary>
	/// <typeparam name="T">The value of stored entries</typeparam>
	public partial class Timeline<T> {
		Dictionary<Entry, (Node start, Node end)> nodesByEntries = new();
		Node? firstStartNode;
		Node? firstEndNode;
		// current means that we have already activated them
		Node? currentStartNode;
		Node? currentEndNode;

		/// <inheritdoc cref="PeriStuff.ModifiedBehaviour"/>
		public ModifiedBehaviour ModifiedBehaviour = ModifiedBehaviour.Ignore;

		private double currentTime;
		/// <summary>
		/// The current time. Setting it will seek until the given time is reached
		/// </summary>
		/// <remarks>
		///	This uses <see cref="SeekToAfter(double)"/> to set time. 
		///	Take note that even if you set this to the same value as previously, 
		///	it might rise events if some werent due to seeking with other methods
		///	where the entries all start at the same time instant
		/// </remarks>
		public double CurrentTime {
			get => currentTime;
			set => SeekToAfter( value );
		}

		/// <summary>
		/// The next entry to start
		/// </summary>
		public Entry? NextStart => (currentStartNode is null ? firstStartNode : currentStartNode.Next)?.Value;
		/// <summary>
		/// The next entry to end
		/// </summary>
		public Entry? NextEnd => (currentEndNode is null ? firstEndNode : currentEndNode.Next)?.Value;
		/// <summary>
		/// The last entry that started
		/// </summary>
		public Entry? PreviousStart => currentStartNode?.Value;
		/// <summary>
		/// The last entry that ended
		/// </summary>
		public Entry? PreviousEnd => currentEndNode?.Value;

		/// <summary>
		/// Insert an entry into the timeline
		/// </summary>
		/// <param name="value">Value of the entry</param>
		/// <param name="time">Time at which the entry starts</param>
		/// <param name="duration">The duration of the entry. If negative, the entry starts this much earlier</param>
		/// <returns>The inserted entry</returns>
		public Entry Add ( T value, double time, double duration = 0 ) {
			if ( duration < 0 ) {
				time += duration;
				duration = -duration;
			}

			var currentTime = CurrentTime;
			var currentEntry = currentStartNode?.Value;
			var endTime = time + duration;

			var entry = new Entry( value, time, duration );
			var startNode = new Node( time, entry );
			var endNode = new Node( endTime, entry );
			nodesByEntries.Add( entry, (startNode, endNode) );

			if ( ModifiedBehaviour is ModifiedBehaviour.Rewind or ModifiedBehaviour.Reapply && currentTime > time ) {
				SeekToAfter( time );
			}

			if ( firstStartNode is null || time < firstStartNode.Time ) {
				if ( firstStartNode != null ) {
					startNode.Next = firstStartNode;
					firstStartNode.Previous = startNode;
				}
				firstStartNode = startNode;
			}
			else {
				(currentStartNode ?? firstStartNode).Insert( startNode );
			}

			if ( firstEndNode is null || endTime < firstEndNode.Time ) {
				if ( firstEndNode != null ) {
					endNode.Next = firstEndNode;
					firstEndNode.Previous = endNode;
				}
				firstEndNode = endNode;
			}
			else {
				(currentEndNode ?? firstEndNode).Insert( endNode );
			}

			if ( ModifiedBehaviour is ModifiedBehaviour.Ignore ) {
				// making sure the current- nodes are correct, that is, if we seeked past the inserted ones already they should be the current ones
				if ( currentStartNode != null && currentTime >= time && time > currentStartNode.Time ) {
					currentStartNode = startNode;
				}
				if ( currentEndNode != null && currentTime >= endTime && endTime > currentEndNode.Time ) {
					currentEndNode = endNode;
				}
			}
			else if ( ModifiedBehaviour is ModifiedBehaviour.Reapply && currentTime > time ) {
				if ( currentEntry?.StartTime == currentTime ) {
					SeekToAfterStart( currentEntry );
				}
				else {
					SeekToAfter( currentTime );
				}
			}

			return entry;
		}

		/// <summary>
		/// Removes an entry from the timeline
		/// </summary>
		/// <param name="entry">The entry</param>
		/// <returns>Whether the entry was removed successfully. It can only fail if it was not a part of the timeline</returns>
		public bool Remove ( Entry entry ) {
			if ( !nodesByEntries.ContainsKey( entry ) ) {
				return false;
			}

			var currentTime = CurrentTime;
			var currentEntry = currentStartNode?.Value;
			if ( currentTime >= entry.StartTime ) {
				if ( ModifiedBehaviour is ModifiedBehaviour.Rewind or ModifiedBehaviour.Reapply ) {
					SeekToBeforeStart( entry );
				}
			}

			nodesByEntries.Remove( entry, out var nodes );
			var (start, end) = nodes;

			if ( start == firstStartNode ) {
				firstStartNode = start.Next;
			}
			if ( start == currentStartNode ) {
				currentStartNode = currentStartNode.Previous;
			}
			if ( end == firstEndNode ) {
				firstEndNode = end.Next;
			}
			if ( end == currentEndNode ) {
				currentEndNode = currentEndNode.Previous;
			}
			start.Remove();
			end.Remove();

			if ( currentTime >= entry.StartTime && ModifiedBehaviour is ModifiedBehaviour.Reapply ) {
				if ( currentEntry?.StartTime == currentTime ) {
					SeekToAfterStart( currentEntry );
				}
				else {
					SeekToAfter( currentTime );
				}
			}

			return true;
		}

		/// <summary>
		/// Triggered when the start of an entry is reached while seeking forwards in time
		/// </summary>
		public event Action<Entry>? EventStarted;
		/// <summary>
		/// Triggered when the end of an entry is reached while seeking forwards in time
		/// </summary>
		public event Action<Entry>? EventEnded;
		/// <summary>
		/// Triggered when the end of an entry is reached while seeking backwards in time
		/// </summary>
		public event Action<Entry>? EventReverted;
		/// <summary>
		/// Triggered when the start of an entry is reached while seeking backwards in time
		/// </summary>
		public event Action <Entry>? EventRewound;
	}
}
