namespace PeriStuff {
	public partial class Timeline<T> {
		Node? nextStart => currentStartNode is null ? firstStartNode : currentStartNode.Next;
		Node? nextEnd => currentEndNode is null ? firstEndNode : currentEndNode.Next;
		Node? previousStart => currentStartNode;
		Node? previousEnd => currentEndNode;

		/// <summary>
		/// Seeks forwards or backwards in time until the given time is reached.
		/// Events that happen at the given time will all be rised
		/// </summary>
		/// <param name="time">The time to seek to</param>
		/// <returns>
		/// The signed amount of entries seeked by. This is starts + ends combined.
		/// It will be negatiove when seeking backward and positive when seeking forward
		/// </returns>
		public int SeekToAfter ( double time ) {
			int count = 0;
			while ( time < currentTime && (previousStart?.Time > time || previousEnd?.Time > time) ) {
				SeekOneEntryBack();
				count--;
			}
			while ( time >= currentTime && (nextStart?.Time <= time || nextEnd?.Time <= time) ) {
				SeekOneEntryForward();
				count++;
			}

			currentTime = time;
			return count;
		}

		/// <summary>
		/// Seeks forwards or backwards in time until the given time is reached.
		/// Events that happen at the given time will all *NOT* be rised
		/// </summary>
		/// <param name="time">The time to seek to</param>
		/// <returns>
		/// The signed amount of entries seeked by. This is starts + ends combined.
		/// It will be negatiove when seeking backward and positive when seeking forward
		/// </returns>
		public int SeekToBefore ( double time ) {
			int count = 0;
			while ( time <= currentTime && (previousStart?.Time >= time || previousEnd?.Time >= time) ) {
				SeekOneEntryBack();
				count--;
			}
			while ( time > currentTime && (nextStart?.Time < time || nextEnd?.Time < time) ) {
				SeekOneEntryForward();
				count++;
			}

			currentTime = time;
			return count;
		}

		/// <summary>
		/// Seeks forwards or backwards in time until the given entrys start is reached.
		/// Events that happen before this entry and the entry itself will be rised
		/// </summary>
		/// <param name="entry">The entry to seek to</param>
		/// <returns>
		/// The signed amount of entries seeked by. This is starts + ends combined.
		/// It will be negatiove when seeking backward and positive when seeking forward
		/// </returns>
		public int SeekToAfterStart ( Entry entry ) {
			if ( nodesByEntries.TryGetValue( entry, out var nodes ) ) {
				var node = nodes.start;
				var time = node.Time;
				int count = 0;
				while ( previousStart != node && currentTime > time ) {
					SeekOneEntryBack();
					count--;
				}
				while ( previousStart != node && currentTime <= time ) {
					SeekOneEntryForward();
					count++;
				}

				return count;
			}
			else {
				return SeekToAfter( entry.StartTime );
			}
		}

		/// <summary>
		/// Seeks forwards or backwards in time until the given entrys start is reached.
		/// Events that happen before this entry will be rised, but the entry itself will not
		/// </summary>
		/// <param name="entry">The entry to seek to</param>
		/// <returns>
		/// The signed amount of entries seeked by. This is starts + ends combined.
		/// It will be negatiove when seeking backward and positive when seeking forward
		/// </returns>
		public int SeekToBeforeStart ( Entry entry ) {
			if ( nodesByEntries.TryGetValue( entry, out var nodes ) ) {
				var node = nodes.start;
				var time = node.Time;
				int count = 0;
				while ( nextStart != node && currentTime >= time ) {
					SeekOneEntryBack();
					count--;
				}
				while ( nextStart != node && currentTime < time ) {
					SeekOneEntryForward();
					count++;
				}

				return count;
			}
			else {
				return SeekToBefore( entry.StartTime );
			}
		}

		/// <summary>
		/// Seeks forwards or backwards in time until the given entrys end is reached.
		/// Events that happen before this entry and the entry itself will be rised
		/// </summary>
		/// <param name="entry">The entry to seek to</param>
		/// <returns>
		/// The signed amount of entries seeked by. This is starts + ends combined.
		/// It will be negatiove when seeking backward and positive when seeking forward
		/// </returns>
		public int SeekToAfterEnd ( Entry entry ) {
			if ( nodesByEntries.TryGetValue( entry, out var nodes ) ) {
				var node = nodes.end;
				var time = node.Time;
				int count = 0;
				while ( previousEnd != node && currentTime > time ) {
					SeekOneEntryBack();
					count--;
				}
				while ( previousEnd != node && currentTime <= time ) {
					SeekOneEntryForward();
					count++;
				}

				return count;
			}
			else {
				return SeekToAfter( entry.EndTime );
			}
		}

		/// <summary>
		/// Seeks forwards or backwards in time until the given entrys end is reached.
		/// Events that happen before this entry will be rised, but the entry itself will not
		/// </summary>
		/// <param name="entry">The entry to seek to</param>
		/// <returns>
		/// The signed amount of entries seeked by. This is starts + ends combined.
		/// It will be negatiove when seeking backward and positive when seeking forward
		/// </returns>
		public int SeekToBeforeEnd ( Entry entry ) {
			if ( nodesByEntries.TryGetValue( entry, out var nodes ) ) {
				var node = nodes.end;
				var time = node.Time;
				int count = 0;
				while ( nextEnd != node && currentTime >= time ) {
					SeekOneEntryBack();
					count--;
				}
				while ( nextEnd != node && currentTime < time ) {
					SeekOneEntryForward();
					count++;
				}

				return count;
			}
			else {
				return SeekToBefore( entry.EndTime );
			}
		}

		/// <summary>
		/// Seeks one start or end of an entry forward
		/// </summary>
		/// <returns>Whether the operation was successfull. It can only fail when there are no more entries to seek to</returns>
		public bool SeekOneEntryForward () {
			var nextStart = this.nextStart;
			var nextEnd = this.nextEnd;

			if ( nextEnd != null && nextStart != null ) {
				if ( nextStart.Time <= nextEnd.Time ) {
					nextEnd = null;
				}
				else {
					nextStart = null;
				}
			}

			if ( nextStart != null ) {
				currentTime = nextStart.Time;
				currentStartNode = nextStart;
				EventStarted?.Invoke( nextStart.Value );
				return true;
			}
			else if ( nextEnd != null ) {
				currentTime = nextEnd.Time;
				currentEndNode = nextEnd;
				EventEnded?.Invoke( nextEnd.Value );
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Seeks one start or end of an entry back
		/// </summary>
		/// <returns>Whether the operation was successfull. It can only fail when there are no more entries to seek to</returns>
		public bool SeekOneEntryBack () {
			var previousStart = this.previousStart;
			var previousEnd = this.previousEnd;

			if ( previousStart != null && previousEnd != null ) {
				if ( previousStart.Time <= previousEnd.Time ) {
					previousStart = null;
				}
				else {
					previousEnd = null;
				}
			}

			if ( previousEnd != null ) {
				currentTime = previousEnd.Time;
				currentEndNode = previousEnd.Previous;
				EventReverted?.Invoke( previousEnd.Value );
				return true;
			}
			else if ( previousStart != null ) {
				currentTime = previousStart.Time;
				currentStartNode = previousStart.Previous;
				EventRewound?.Invoke( previousStart.Value );
				return true;
			}
			else {
				return false;
			}
		}
	}
}
