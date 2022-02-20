namespace PeriStuff {
	public partial class Timeline<T> {
		public record Entry ( T Value, double StartTime, double Duration ) { }
	}
}
