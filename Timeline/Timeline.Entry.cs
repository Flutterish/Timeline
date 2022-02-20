namespace PeriStuff {
	public partial class Timeline<T> {
		public record Entry ( T Value, double StartTime, double Duration ) {
			public double EndTime => StartTime + Duration;
		}
	}
}
