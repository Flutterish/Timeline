namespace PeriStuff {
	/// <summary>
	/// How the timeline behaves when an entry is added or removed
	/// </summary>
	public enum ModifiedBehaviour {
		/// <summary>
		/// Do not do anything and just modify the timeline
		/// </summary>
		Ignore,
		/// <summary>
		/// Rewind to before the entry started, then modify
		/// </summary>
		Rewind,
		/// <summary>
		/// Rewind to before the entry started, modify, then seek back to current time
		/// </summary>
		Reapply
	}
}
