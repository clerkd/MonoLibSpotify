/*
Copyright (c) 2012-2013 Tim Ermilov, Clerkd, yamalight@gmail.com

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
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MonoTouch.CoreFoundation;
using MonoTouch.CoreMedia;
using MonoTouch.AVFoundation;
using MonoTouch.AudioToolbox;
using MonoTouch.AudioUnit;
using MonoTouch.AudioUnitWrapper;

using MonoLibSpotify.Models;

namespace MonoLibSpotify.Player
{
	// audio buffer
	internal class AudioBuffer
	{
		public IntPtr Buffer { get; set; }
		public int CurrentOffset { get; set; }
		public bool IsInUse { get; set; }
	}
	
	public class SPPlayeriOS : IDisposable
	{
		// stream description
		AudioStreamBasicDescription desc;
		public AudioStreamBasicDescription AudioDescription {
			get { return desc; }
			set {
				desc = value;
				// set buffer size
				bufferSize = (int)((desc.BytesPerFrame * desc.SampleRate) * TargetBufferLength / maxBufferCount);
				// init queue & buffers 
				initOutput();
			}
		}

		// the AudioToolbox decoder
		int bufferSize = 1024;
		List<AudioBuffer> outputBuffers;
		AudioBuffer currentBuffer;
		OutputAudioQueue OutputQueue;
		bool buffersInited = false;
		// Maximum buffers
		int maxBufferCount = 5;
		// Keep track of all queued up buffers, so that we know that the playback finished
		int queuedBufferCount = 0;

		// buffer time in seconds
		public float TargetBufferLength = 10f;

		// actions
		public delegate void OnPlayback();
		public delegate void OnOutputTime(double time);

		// on end raise
		bool endRaised;

		public event OnPlayback onPlaybackStart;
		public event OnPlayback onPlaybackEnd;
		public event OnOutputTime didOutputTime;

		// 
		public bool Started { get; private set; }
		public bool Paused { get; private set; }
		public double CurrentTime { get; private set; }
		public float Volume {
			get {
				return OutputQueue.Volume;
			}
			
			set {
				OutputQueue.Volume = value;
			}
		}

		/// <summary>
		/// Defines the size forearch buffer, when using a slow source use more buffers with lower buffersizes
		/// </summary>
		public int BufferSize {
			get {
				return bufferSize;
			}
			
			set {
				bufferSize = value;
			}
		}
		
		/// <summary>
		/// Defines the maximum Number of Buffers to use, the count can only change after Reset is called or the 
		/// StreamingPlayback is freshly instantiated
		/// </summary>
		public int MaxBufferCount
		{
			get {
				return maxBufferCount;
			}
			
			set {
				maxBufferCount = value;
			}
		}

		public SPPlayeriOS ()
		{
			endRaised = false;
		}

		void initOutput()
		{
			// init queue
			if( OutputQueue != null ){
				OutputQueue.Stop(true);
				OutputQueue.OutputCompleted -= HandleOutputQueueOutputCompleted;
				OutputQueue.Dispose();
				OutputQueue = null;
			}
			OutputQueue = new OutputAudioQueue (desc);
			OutputQueue.OutputCompleted += HandleOutputQueueOutputCompleted;

			initBuffers();
		}
		void initBuffers()
		{
			// create buffers
			outputBuffers = new List<AudioBuffer>();
			for (int i = 0; i < MaxBufferCount; i++) {
				IntPtr outBuffer;
				OutputQueue.AllocateBuffer (BufferSize, out outBuffer);
				outputBuffers.Add (new AudioBuffer () { Buffer = outBuffer });
			}
			currentBuffer = outputBuffers[0];

			buffersInited = true;
		}

		public int EnqueFrames(int channels, int rate, byte[] samples, int frames)
		{
			if( Paused ) return 0;
			if( !buffersInited ) return 0;

			int consumedFrames = 0;
			
			if (samples != null && samples.Length > 0)
			{
				// parse
				ParseBytes(samples, samples.Length, false, false);

				// report consumed frames
				double time = (double)frames / desc.SampleRate * 1000;
				CurrentTime += time;
				if( didOutputTime != null ) didOutputTime.DynamicInvoke(time);

				// report that we consumed all frames
				consumedFrames = frames;
			}
			
			return consumedFrames;
		}

		public void Reset ()
		{
			CurrentTime = 0;
			Started = false;
			Paused = false;
			endRaised = false;
			ResetOutputQueue();
		}
		
		public void ResetOutputQueue ()
		{
			if (OutputQueue != null) {
				OutputQueue.Stop (true);
				OutputQueue.Flush ();
				OutputQueue.Reset ();

				// clear buffers
				buffersInited = false;
				foreach (AudioBuffer buf in outputBuffers) {
					OutputQueue.FreeBuffer (buf.Buffer);
				}
				outputBuffers = null;

				// reinit buffers
				initBuffers();
			}
		}

		/// <summary>
		/// Stops the OutputQueue completely
		/// </summary>
		public void Stop ()
		{
			if( OutputQueue == null ) return;

			OutputQueue.Stop (true);
			Started = false;
		}
		
		/// <summary>
		/// Stops the OutputQueue
		/// </summary>
		public void Pause ()
		{
			Paused = true;
			Stop();
			//OutputQueue.Pause ();
			//Started = false;
		}
		
		/// <summary>
		/// Starts the OutputQueue
		/// </summary>
		public void Play ()
		{
			OutputQueue.Start ();
			Started = true;
			Paused = false;
			if( onPlaybackStart != null ) onPlaybackStart.DynamicInvoke();
		}
		
		/// <summary>
		/// Main methode to kick off the streaming, just send the bytes to this method
		/// </summary>
		public void ParseBytes (byte[] buffer, int count, bool discontinuity, bool lastPacket)
		{
			if( !buffersInited ) return;

			int left = bufferSize - currentBuffer.CurrentOffset;
			if (left < buffer.Length) {
				EnqueueBuffer ();
				WaitForBuffer ();
			}

			unsafe {
				fixed(byte* ptr = buffer) {
					OutputAudioQueue.FillAudioData (currentBuffer.Buffer, currentBuffer.CurrentOffset, new IntPtr ((void *)ptr), 0, buffer.Length);
				}
			}

			// Add the Size so that we know how much is in the buffer
			currentBuffer.CurrentOffset += buffer.Length;
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		/// <summary>
		/// Cleaning up all the native Resource
		/// </summary>
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (OutputQueue != null)
					OutputQueue.Stop (false);
				
				if (outputBuffers != null)
					foreach (var b in outputBuffers)
						OutputQueue.FreeBuffer (b.Buffer);
				
				if (OutputQueue != null) {
					OutputQueue.Dispose ();
					OutputQueue = null;
				}
			}
		}
		
		/// <summary>
		/// Flush the current buffer and close the whole thing up
		/// </summary>
		public void FlushAndClose ()
		{
			EnqueueBuffer ();
			OutputQueue.Flush ();
		}

		public void ForceRaiseEnd()
		{
			if (endRaised)
				return;

			if (onPlaybackEnd != null && !endRaised) {
				endRaised = true;
				onPlaybackEnd();
			}
		}
		
		/// <summary>
		/// Enqueue the active buffer to the OutputQueue
		/// </summary>
		void EnqueueBuffer ()
		{			
			currentBuffer.IsInUse = true;
			OutputQueue.EnqueueBuffer (currentBuffer.Buffer, currentBuffer.CurrentOffset, null);
			queuedBufferCount++;
			StartQueueIfNeeded ();
		}
		
		/// <summary>
		/// Wait until a buffer is freed up
		/// </summary>
		void WaitForBuffer ()
		{
			if( outputBuffers == null ) return;

			int curIndex = outputBuffers.IndexOf (currentBuffer);
			currentBuffer = outputBuffers[curIndex < outputBuffers.Count - 1 ? curIndex + 1 : 0];
			
			lock (currentBuffer) {
				while (currentBuffer.IsInUse) 
					Monitor.Wait (currentBuffer);
			}
		}
		
		void StartQueueIfNeeded ()
		{
			if (Started || Paused)
				return;
			
			Play ();
		}
		
		/// <summary>
		/// Is called when a buffer is completly read and can be freed up
		/// </summary>
		void HandleOutputQueueOutputCompleted (object sender, OutputCompletedEventArgs e)
		{
			queuedBufferCount--;
			IntPtr buf = e.IntPtrBuffer;
			
			foreach (var buffer in outputBuffers) {
				if (buffer.Buffer != buf)
					continue;
				
				// free Buffer
				buffer.CurrentOffset = 0;
				lock (buffer) {
					buffer.IsInUse = false;
					Monitor.Pulse (buffer);
				}
			}
			
			if (queuedBufferCount == 0 && onPlaybackEnd != null && !endRaised) {
				endRaised = true;
				onPlaybackEnd();
			}
		}
	}
}

