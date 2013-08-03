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
using System.Runtime.InteropServices;

namespace MonoLibSpotify.Models
{
	public class SPLink : IDisposable
	{
		#region Declarations
		internal IntPtr linkPtr = IntPtr.Zero;
		#endregion
		
		#region Ctor
		internal SPLink(IntPtr linkPtr)
		{
			if(linkPtr == IntPtr.Zero)
				throw new ArgumentException("linkPtr can not be zero");	
			
			this.linkPtr = linkPtr;
		}
		#endregion
		
		#region Static methods
		public static SPLink Create(string linkString)
		{
			lock(LibspotifyWrapper.Mutex)
			{
			    var linkPtr = LibspotifyWrapper.Link.CreateFromString(linkString);
			    return linkPtr != IntPtr.Zero ? new SPLink(linkPtr) : null;
			}
		}
		#endregion
		
		#region Properties
		public sp_linktype LinkType
		{
			get
			{
				CheckDisposed(true);
				lock(LibspotifyWrapper.Mutex)
					return LibspotifyWrapper.Link.Type(linkPtr);
			}
		}
		#endregion
		
		#region Public methods
		public override string ToString()
		{
			CheckDisposed(true);
			
			var result = string.Empty;
			var bufSize = 256;
			
			while(true)
			{
				var strlen = bufSize;
				var bufferPtr = IntPtr.Zero;
				
				try
				{
					bufferPtr = Marshal.AllocHGlobal(bufSize);
					
					lock(LibspotifyWrapper.Mutex)
					{
						strlen = LibspotifyWrapper.Link.AsString(linkPtr, bufferPtr, bufSize);	
					}
					
					if(strlen < 0)
					{
						result = "ERROR";
						break;
					}
					else if(strlen < bufSize)
					{
						result = LibspotifyWrapper.GetString(bufferPtr, string.Empty);
						break;					
					}
					else
					{
						bufSize *=2;
					}
				}
				finally
				{
					if(bufferPtr != IntPtr.Zero)
						Marshal.FreeHGlobal(bufferPtr);
				}		                    
			}
			
			return result;
		}
		#endregion
		
		#region Cleanup
		~SPLink()
		{
			Dispose(false);
		}
		
		protected void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					
				}

			    if (linkPtr == IntPtr.Zero) return;
			    LibspotifyWrapper.Link.Release(linkPtr);
			    linkPtr = IntPtr.Zero;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}		
		
		public void Dispose()
		{
			if(linkPtr == IntPtr.Zero)
				return;
			
			Dispose(true);
			GC.SuppressFinalize(this);			
		}
		
		private bool CheckDisposed(bool throwOnDisposed)
		{
			lock(LibspotifyWrapper.Mutex)
			{
				var result = linkPtr == IntPtr.Zero;
				if(result && throwOnDisposed)
					throw new ObjectDisposedException("Link");
				
				return result;
			}
		}
		#endregion
	}
}

