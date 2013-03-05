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

namespace MonoLibSpotify.Events
{
	public class MusicDeliveryEventArgs : EventArgs
	{
		private int channels;
		private int rate;
		private int type;
		private byte[] samples;
		private int frames;
		
		internal MusicDeliveryEventArgs(int channels, int type, int rate, byte[] samples, int frames)
		{
			this.channels = channels;
			this.rate = rate;			
			this.samples = samples;
			this.frames = frames;
			this.type = type;
			
			this.ConsumedFrames = 0;
		}
		
		public int ConsumedFrames 
		{
			get;
			set;			
		}
		
		public int Frames 
		{
			get { return frames; }			
		}
		
		public int Channels
		{
			get { return channels; }
		}
		
		public int Rate
		{
			get { return rate; }
		}	

		public int Type
		{
			get { return type; }
		}
		
		public byte[] Samples
		{
			get { return samples; }
		}
	}
}

