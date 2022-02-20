using NUnit.Framework;
using PeriStuff;

namespace Tests {
	public class Tests {
		[Test]
		public void SimplePass () {
			var timeline = new Timeline<int>();

			// 0     100     200   300     400   500
			// [1    ]       [2    ]       [3    ]
			timeline.Add( 1, 0, 100 );
			timeline.Add( 2, 200, 100 );
			timeline.Add( 3, 400, 100 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Ended, 1 );
				pass.Expect( EventType.Started, 2 );
				pass.Expect( EventType.Ended, 2 );
				pass.Expect( EventType.Started, 3 );

				timeline.SeekToBefore( 500 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Ended, 3 );

				timeline.SeekToAfter( 500 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Reverted, 3 );
				pass.Expect( EventType.Rewound, 3 );
				pass.Expect( EventType.Reverted, 2 );
				pass.Expect( EventType.Rewound, 2 );
				pass.Expect( EventType.Reverted, 1 );

				timeline.SeekToAfter( 0 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Rewound, 1 );

				timeline.SeekToBefore( 0 );
			}
		}

		[Test]
		public void FullCover () {
			var timeline = new Timeline<int>();

			// 0       50         100       150
			// [1                           ]
			//         [2         ]
			timeline.Add( 1, 0, 150 );
			timeline.Add( 2, 50, 50 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Started, 2 );
				pass.Expect( EventType.Ended, 2 );

				timeline.SeekToBefore( 150 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Ended, 1 );

				timeline.SeekToAfter( 150 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Reverted, 1 );
				pass.Expect( EventType.Reverted, 2 );

				timeline.CurrentTime = 75;
			}
		}

		[Test]
		public void PartialCover () {
			var timeline = new Timeline<int>();

			// 0       50         100       150          200
			// [1                           ]
			//         [2                                ]
			timeline.Add( 1, 0, 150 );
			timeline.Add( 2, 50, 150 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Started, 2 );
				pass.Expect( EventType.Ended, 1 );
				pass.Expect( EventType.Ended, 2 );

				timeline.CurrentTime = 200;
			}
		}

		[Test]
		public void Simultanious () {
			var timeline = new Timeline<int>();

			// 0             200
			// [1            ]
			// [2            ]
			timeline.Add( 1, 0, 200 );
			timeline.Add( 2, 0, 200 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Started, 2 );
				pass.Expect( EventType.Ended, 1 );
				pass.Expect( EventType.Ended, 2 );

				timeline.CurrentTime = 200;
			}
		}

		[Test]
		public void BackToBack () {
			var timeline = new Timeline<int>();

			// 0             100           200
			// [1            ]
			//               [2            ]
			timeline.Add( 1, 0, 100 );
			timeline.Add( 2, 100, 100 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Started, 2 );
				pass.Expect( EventType.Ended, 1 );
				pass.Expect( EventType.Ended, 2 );

				timeline.CurrentTime = 200;
			}
		}

		[Test]
		public void Instant () {
			var timeline = new Timeline<int>();

			// 0  0
			// [1 ]
			timeline.Add( 1, 0, 0 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Ended, 1 );

				timeline.SeekToAfter( 0 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Reverted, 1 );
				pass.Expect( EventType.Rewound, 1 );

				timeline.SeekToBefore( 0 );
			}
		}

		[Test]
		public void ModifyIgnore () {
			var timeline = new Timeline<int> { ModifiedBehaviour = ModifiedBehaviour.Ignore };

			// 0            100
			// [1           ]
			var entry = timeline.Add( 1, 0, 100 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Ended, 1 );

				timeline.SeekToAfter( 400 );
				Assert.That( timeline.PreviousEnd == entry );
				Assert.That( timeline.PreviousStart == entry );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				// 0            100            200          300              400
				// [1           ]              [2           ]                v
				entry = timeline.Add( 2, 200, 100 );

				Assert.That( timeline.PreviousEnd == entry );
				Assert.That( timeline.PreviousStart == entry );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Reverted, 2 );
				pass.Expect( EventType.Rewound, 2 );
				// 0            100     150    200          300
				// [1           ]       v      [2           ]
				timeline.CurrentTime = 150;

				Assert.That( timeline.PreviousEnd != entry );
				Assert.That( timeline.PreviousStart != entry );
			}
		}

		[Test]
		public void ModifyIgnore2 () {
			var timeline = new Timeline<int> { ModifiedBehaviour = ModifiedBehaviour.Ignore };

			// 0            100
			// [1           ]
			var entry = timeline.Add( 1, 0, 100 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Ended, 1 );

				timeline.SeekToAfter( 150 );
				Assert.That( timeline.PreviousEnd == entry );
				Assert.That( timeline.PreviousStart == entry );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				// 0            100     150    200          300 
				// [1           ]       v      [2           ]                
				entry = timeline.Add( 2, 200, 100 );

				Assert.That( timeline.PreviousEnd != entry );
				Assert.That( timeline.PreviousStart != entry );
				Assert.That( timeline.NextEnd == entry );
				Assert.That( timeline.NextStart == entry );
			}
		}

		[Test]
		public void ModifyReapply () {
			var timeline = new Timeline<int> { ModifiedBehaviour = ModifiedBehaviour.Reapply };

			//                             200          300              400    
			//                             [1           ]                v
			timeline.Add( 1, 200, 100 );

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Ended, 1 );

				timeline.SeekToAfter( 400 );
			}

			using ( var pass = new TimelineAssert<int>( timeline ) ) {
				pass.Expect( EventType.Reverted, 1 );
				pass.Expect( EventType.Rewound, 1 );

				pass.Expect( EventType.Started, 2 );
				pass.Expect( EventType.Ended, 2 );
				pass.Expect( EventType.Started, 1 );
				pass.Expect( EventType.Ended, 1 );
				// 0            100            200          300          400
				// [2           ]              [1           ]            v        
				timeline.Add( 2, 0, 100 );
			}
		}
	}
}