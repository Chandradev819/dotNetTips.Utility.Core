﻿// ***********************************************************************
// Assembly         : dotNetTips.Utility.Standard.Extensions
// Author           : David McCarter
// Created          : 07-22-2020
//
// Last Modified By : David McCarter
// Last Modified On : 08-04-2020
// ***********************************************************************
// <copyright file="SocketExtensions.cs" company="dotNetTips.com - David McCarter">
//     McCarter Consulting (David McCarter)
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using dotNetTips.Utility.Standard.Common;

namespace dotNetTips.Utility.Standard.Extensions
{
    /// <summary>
    /// Extensions for Socket.
    /// </summary>
    [Information("From .NET Core source.", author: "David McCarter", createdOn: "7/15/2020", modifiedOn: "7/29/2020", UnitTestCoverage = 0, Status = Status.New)]
    public static class SocketExtensions
    {

        /// <summary>
        /// Binds to an IP address and OS-assigned port. Returns the chosen port.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="address">The address.</param>
        /// <returns>System.Int32.</returns>
        [Information("From .NET Core source.", author: "David McCarter", createdOn: "7/15/2020", modifiedOn: "7/29/2020", UnitTestCoverage = 0, Status = Status.New)]
        public static int BindToAnonymousPort(this Socket socket, IPAddress address)
        {
            socket.Bind(new IPEndPoint(address, 0));

            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }


        /// <summary>
        /// On non-Windows platforms, once non-blocking is turned on (either explicitly
        /// or by performing an async operation), always stay in non-blocking mode.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        [Information("From .NET Core source.", author: "David McCarter", createdOn: "7/15/2020", modifiedOn: "7/29/2020", UnitTestCoverage = 0, Status = Status.New)]
        public static void ForceNonBlocking(this Socket socket, bool force)
        {
            if (force)
            {
                socket.Blocking = false;
                socket.Blocking = true;
            }
        }

        /// <summary>
        /// Tries to connect within the provided timeout interval
        /// Useful to speed up "can not connect" assertions on Windows
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        [Information("From .NET Core source.", author: "David McCarter", createdOn: "7/15/2020", modifiedOn: "7/29/2020", UnitTestCoverage = 0, Status = Status.New)]
        public static bool TryConnect(this Socket socket, EndPoint remoteEndpoint, int millisecondsTimeout)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var mre = new ManualResetEventSlim(false))
                {
                    using (var sea = new SocketAsyncEventArgs() { RemoteEndPoint = remoteEndpoint, UserToken = mre })
                    {
                        sea.Completed += (s, e) => ((ManualResetEventSlim)e.UserToken).Set();

                        var pending = socket.ConnectAsync(sea);

                        if (!pending || mre.Wait(millisecondsTimeout))
                        {
                            return sea.SocketError == SocketError.Success;
                        }

                        Socket.CancelConnectAsync(sea); // this will close the socket!

                        // In case of time-out, ManualResetEventSlim is left undisposed to avoid race conditions,
                        // letting SafeHandle's finalizer to do the cleanup.
                        return false;
                    }
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

    }
}
