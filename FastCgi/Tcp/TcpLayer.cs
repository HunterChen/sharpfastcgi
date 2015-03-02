﻿#region License
//*****************************************************************************/
// Copyright (c) 2012 Luigi Grilli
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//*****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Grillisoft.FastCgi.Protocol;
using ByteArray = Grillisoft.ImmutableArray.ImmutableArray<byte>;
using Grillisoft.ImmutableArray;

namespace Grillisoft.FastCgi.Tcp
{
	public class TcpLayer : ILowerLayer, IDisposable
	{
		public event EventHandler<UnhandledExceptionEventArgs> RunError;

        private readonly TcpClient _client;
        private readonly byte[] _recvBuffer = new byte[4096];
        private readonly byte[] _sendBuffer = new byte[4096];

		public TcpLayer(TcpClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Upper layer to send data received from tcp channel
		/// </summary>
		public IUpperLayer UpperLayer { get; set; }

		/// <summary>
        /// Continuously calls the <see cref="TcpClient.GetStream().Read"/> method of the <see cref="TcpClient"/> while the socket is connected
		/// </summary>
		/// <remarks>This is a blocking call. When exiting this call the socket will be already closed</remarks>
		public void Run()
		{
            try
			{
			    while (_client.Connected)
			    {
				    //blocking call
                    int read = _client.GetStream().Read(_recvBuffer, 0, _recvBuffer.Length);
				    if (read > 0)
                    {
                        //Console.WriteLine("received {0} bytes", read);
                        this.UpperLayer.Receive(new ByteArray(_recvBuffer, read));
                    }
			    }
            }
            catch (Exception ex)
            {
                this.OnRunError(new UnhandledExceptionEventArgs(ex, false));
            }
            finally
            {
                _client.Close();
            }

            Console.WriteLine("Tcp layer loop completed");
		}

		/// <summary>
		/// Sends data to the network
		/// </summary>
		/// <param name="data">Data to send</param>
		public void Send(ByteArray data)
		{
			lock (_sendBuffer)
			{
                while (data.Count > 0)
                {
                    int length = Math.Min(data.Count, _sendBuffer.Length);

                    data.CopyTo(_sendBuffer, 0, length);
                    _client.GetStream().Write(_sendBuffer, 0, length);
                    _client.GetStream().Flush();
                    //Console.WriteLine("sent {0} bytes", length);
                    data = data.SubArray(length, true);
                }
			}
		}

		public void Close()
		{
            if(_client.Connected)
            {
                _client.Client.Disconnect(true);
                _client.Close();
            }
		}

		protected virtual void OnRunError(UnhandledExceptionEventArgs args)
		{
			if (this.RunError != null)
				this.RunError(this, args);
		}

        public void Dispose()
        {
            this.Close();
        }
    }
}
