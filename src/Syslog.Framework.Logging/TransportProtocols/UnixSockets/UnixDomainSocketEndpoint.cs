// This file was copied from https://github.com/dotnet/corefx/blob/d0dc5fc099946adc1035b34a8b1f6042eddb0c75/src/System.Net.Sockets/tests/FunctionalTests/UnixDomainSocketTest.cs#L248
// as System.Net.Socket.UnixDomainSocketEndPoint is not available in NETStandard 2.0
 
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See https://github.com/dotnet/corefx/blob/master/LICENSE.TXT

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Syslog.Framework.Logging.TransportProtocols.UnixSockets
{
	/// <summary>
	/// Version of System.Net.Sockets.UnixDomainSocketEndPoint copied from https://github.com/dotnet/corefx/blob/d0dc5fc099946adc1035b34a8b1f6042eddb0c75/src/System.Net.Sockets/tests/FunctionalTests/UnixDomainSocketTest.cs#L248
	/// as System.Net.Sockets.UnixDomainSocketEndPoint is not available in NETStandard 2.0
	/// </summary>
	internal sealed class UnixDomainSocketEndPoint : EndPoint
	{
		private const AddressFamily EndPointAddressFamily = AddressFamily.Unix;

		private static readonly Encoding s_pathEncoding = Encoding.UTF8;

		private static readonly int s_nativePathOffset = 2; // = offsetof(struct sockaddr_un, sun_path). It's the same on Linux and OSX
		private static readonly int s_nativePathLength = 91; // sockaddr_un.sun_path at http://pubs.opengroup.org/onlinepubs/9699919799/basedefs/sys_un.h.html, -1 for terminator
		private static readonly int s_nativeAddressSize = s_nativePathOffset + s_nativePathLength;

		private readonly string _path;
		private readonly byte[] _encodedPath;

		public UnixDomainSocketEndPoint(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			_path = path;
			_encodedPath = s_pathEncoding.GetBytes(_path);

			if (path.Length == 0 || _encodedPath.Length > s_nativePathLength)
			{
				throw new ArgumentOutOfRangeException(nameof(path));
			}
		}

		internal UnixDomainSocketEndPoint(SocketAddress socketAddress)
		{
			if (socketAddress == null)
			{
				throw new ArgumentNullException(nameof(socketAddress));
			}

			if (socketAddress.Family != EndPointAddressFamily ||
			    socketAddress.Size > s_nativeAddressSize)
			{
				throw new ArgumentOutOfRangeException(nameof(socketAddress));
			}

			if (socketAddress.Size > s_nativePathOffset)
			{
				_encodedPath = new byte[socketAddress.Size - s_nativePathOffset];
				for (int i = 0; i < _encodedPath.Length; i++)
				{
					_encodedPath[i] = socketAddress[s_nativePathOffset + i];
				}

				_path = s_pathEncoding.GetString(_encodedPath, 0, _encodedPath.Length);
			}
			else
			{
				_encodedPath = Array.Empty<byte>();
				_path = string.Empty;
			}
		}

		public override SocketAddress Serialize()
		{
			var result = new SocketAddress(AddressFamily.Unix, s_nativeAddressSize);
			Debug.Assert(_encodedPath.Length + s_nativePathOffset <= result.Size, "Expected path to fit in address");

			for (int index = 0; index < _encodedPath.Length; index++)
			{
				result[s_nativePathOffset + index] = _encodedPath[index];
			}

			result[s_nativePathOffset + _encodedPath.Length] = 0; // path must be null-terminated

			return result;
		}

		public override EndPoint Create(SocketAddress socketAddress) => new UnixDomainSocketEndPoint(socketAddress);

		public override AddressFamily AddressFamily => EndPointAddressFamily;

		public override string ToString() => _path;
	}
}