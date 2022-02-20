namespace PeriStuff {
	public partial class Timeline<T> {
		private class Node {
			public readonly double Time;
			public readonly Entry Value;

			public Node? Next;
			public Node? Previous;
			public Node ( double time, Entry value ) {
				Time = time;
				Value = value;
			}

			public void Insert ( Node node ) {
				var current = this;
				while ( current.Next != null && node.Time >= current.Next.Time ) {
					current = current.Next;
				}
				while ( current.Previous != null && node.Time < current.Previous.Time ) {
					current = current.Previous;
				}

				if ( current.Next != null ) {
					current.Next.Previous = node;
					node.Next = current.Next;
				}
				current.Next = node;
				node.Previous = current;
			}

			public void Remove () {
				if ( Previous != null ) {
					Previous.Next = Next;
				}
				if ( Next != null ) {
					Next.Previous = Previous;
				}
			}
		}
	}
}
