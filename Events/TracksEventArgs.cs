/*
Copyright (c) 2012-2013 Tim Ermilov, Clerkd, yamalight@gmail.com

Based on source code from:
https://github.com/jonasl/libspotify-sharp
Copyright (c) 2009 Jonas Larsson, jonas@hallerud.se

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

using MonoLibSpotify.Models;

namespace MonoLibSpotify.Events
{
	public class TracksEventArgs
	{
		private SPTrack[] tracks;
		private int[] trackIndices;
		private int position;
		private int newPosition;
		private SPTrack[] currentTracks;
		
		internal TracksEventArgs(SPTrack[] tracks, int[] trackIndices, int position, int newPosition, SPTrack[] currentTracks)
		{
			this.tracks = tracks;
			this.trackIndices = trackIndices;
			this.position = position;
			this.newPosition = newPosition;
			this.currentTracks = currentTracks;
		}
		
		public SPTrack[] Tracks
		{
			get
			{
				return tracks;
			}
		}
		
		public int[] TrackIndices
		{
			get
			{
				return trackIndices;
			}
		}
		
		public int Position
		{
			get
			{
				return position;
			}
		}
		
		public int NewPosition
		{
			get
			{
				return newPosition;
			}
		}
		
		public SPTrack[] CurrentTracks
		{
			get
			{
				return currentTracks;
			}
		}
	}
}

