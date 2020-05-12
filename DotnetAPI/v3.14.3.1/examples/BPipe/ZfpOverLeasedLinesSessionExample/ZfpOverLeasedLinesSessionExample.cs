/* Copyright 2019. Bloomberg Finance L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions: The above
 * copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */
using System;
using System.Security;

namespace Bloomberglp.Blpapi.Examples
{
	// The goal of this example is to demonstrate how to establish a ZFP
	// session that leverages private leased line connectivity. To see how to
	// use the resulting session (authorizing a session, establishing
	// subscriptions or making requests etc.), please refer to the other
	// examples.
	public class ZfpOverLeasedLinesSessionExample
	{
		private const string AuthUser = "AuthenticationType=OS_LOGON";

		private const string AuthAppPrefix =
			"AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_ANKEY;ApplicationName=";

		private const string AuthUserAppManualPrefix =
			"AuthenticationMode=USER_ANAPPLICATION;AuthenticationType=MANUAL;ApplicationAuthenticationType=APPNAME_ANKEY;ApplicationName=";

		private const string AuthDirPrefix = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";
		private const string AuthOptionNone = "none";
		private const string AuthOptionUser = "user";
		private const string AuthOptionApp = "app=";
		private const string AuthOptionDir = "dir=";
		private const string AuthOptionManual = "manual=";
		private const string Port8194 = "8194";
		private const string Port8196 = "8196";

		private const string TLSCredentialsArgument = "-tls-client-credentials";
		private const string TLSPasswordArgument = "-tls-client-credentials-password";
		private const string TLSTrustMaterialArgument = "-tls-trust-material";
		private const string ZfpArgument = "-zfp-over-leased-line";
		private const string AuthArgument = "-auth";

		private string manualUserName = null;
		private string manualIPAddress = null;

		private string authOptions = AuthUser;
		private TlsOptions tlsOptions;
		private ZfpUtil.Remote remote;

		public void Run(string[] args)
		{
			if (!ParseCommandLine(args))
			{
				PrintUsage();
				return;
			}

			var sessionOptions = ZfpUtil.GetZfpOptionsForLeasedLines(
				remote,
				tlsOptions);

			sessionOptions.AutoRestartOnDisconnection = true;

			// Note: ZFP solution requires authorization. The appropriate
			// authentication option must be set here on the 'SessionOptions'
			// before the session is created.
			sessionOptions.AuthenticationOptions = authOptions;

			var session = new Session(sessionOptions);

			if (!session.Start())
			{
				Console.Error.WriteLine("Failed to start session.");
				while (true)
				{
					var nextEvent = session.TryNextEvent();
					if (nextEvent == null)
					{
						break;
					}

					foreach (var message in nextEvent.GetMessages())
					{
						Console.WriteLine(message);
					}
				}

				return;
			}

			Console.WriteLine("Session started succesfully.");

			// Note: ZFP solution requires authorization, which should be done
			// here before any subscriptions or requests can be made. For
			// examples of how to authorize or get data, please refer to the
			// specific examples.
		}

		private void PrintUsage()
		{
			Console.WriteLine("ZFP over leased lines session startup:");
			Console.WriteLine("    [-zfp-over-leased-line <port>] enable ZFP connections over leased lines on the specified port (8194 or 8196)");
			Console.WriteLine("    [-auth <option>]            authorization option  (default = user)");
			Console.WriteLine("           none");
			Console.WriteLine("           user                 as a user using OS logon information");
			Console.WriteLine("           dir=<property>       as a user using directory services");
			Console.WriteLine("           app=<app>            as the specified application");
			Console.WriteLine("           userapp=<app>        as user and application using logon information for the user");
			Console.WriteLine("           manual=<app,ip,user> as user and application, with manually provided IP address and EMRS user");
			Console.WriteLine();
			Console.WriteLine("TLS OPTIONS (specify all or none):");
			Console.WriteLine("    [-tls-client-credentials <file>]              name a PKCS#12 file to use as a source of client credentials");
			Console.WriteLine("    [-tls-client-credentials-password <file>]     specify password for accessing client credentials");
			Console.WriteLine("    [-tls-trust-material <file>]                  name a PKCS#7 file to use as a source of trusted certificates");
			Console.WriteLine("Press ENTER to quit");
		}


		private bool ParseCommandLine(string[] args)
		{
			string clientCredentials = null;
			string clientCredentialsPassword = null;
			string trustMaterial = null;

			for (int i = 0; i < args.Length; ++i)
			{
				if (AuthArgument.Equals(args[i], StringComparison.InvariantCulture) && i + 1 < args.Length)
				{
					++i;
					if (AuthOptionNone.Equals(args[i], StringComparison.InvariantCulture))
					{
						authOptions = null;
					}
					else if (AuthOptionUser.Equals(args[i], StringComparison.InvariantCulture))
					{
						authOptions = AuthUser;
					}
					else if (string.Compare(AuthOptionApp, 0, args[i], 0,
						         AuthOptionApp.Length, true) == 0)
					{
						authOptions = AuthAppPrefix
						              + args[i].Substring(AuthOptionApp.Length);
					}
					else if (string.Compare(AuthOptionDir, 0, args[i], 0,
						         AuthOptionDir.Length, true) == 0)
					{
						authOptions = AuthDirPrefix
						              + args[i].Substring(AuthOptionDir.Length);
					}
					else if (string.Compare(AuthOptionManual, 0, args[i],
						         0, AuthOptionManual.Length, true) == 0)
					{
						string[] parms = args[i].Substring(AuthOptionManual.Length).Split(',');
						if (parms.Length != 3)
						{
							return false;
						}

						authOptions = AuthUserAppManualPrefix + parms[0];
						manualIPAddress = parms[1];
						manualUserName = parms[2];
					}
					else
					{
						return false;
					}
				}
				else if (TLSCredentialsArgument.Equals(args[i], StringComparison.InvariantCulture) &&
				         i + 1 < args.Length)
				{
					clientCredentials = args[++i];
				}
				else if (TLSPasswordArgument.Equals(args[i], StringComparison.InvariantCulture) &&
				         i + 1 < args.Length)
				{
					clientCredentialsPassword = args[++i];
				}
				else if (TLSTrustMaterialArgument.Equals(args[i], StringComparison.InvariantCulture) &&
				         i + 1 < args.Length)
				{
					trustMaterial = args[++i];
				}
				else if (ZfpArgument.Equals(args[i], StringComparison.InvariantCulture) && i + 1 < args.Length) {
					if (!TryGetRemote(args[++i], out remote)) {
						PrintUsage();
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			if (clientCredentials == null ||
			    trustMaterial == null ||
			    clientCredentialsPassword == null)
			{
				Console.WriteLine("TLS parameters are required for ZFP connections over a leased line.");
				return false;
			}

			using (var password = new SecureString())
			{
				foreach (var c in clientCredentialsPassword)
				{
					password.AppendChar(c);
				}

				tlsOptions = TlsOptions.CreateFromFiles(clientCredentials, password, trustMaterial);
			}

			return true;
		}

		public static void Main(string[] args)
		{
			var example = new ZfpOverLeasedLinesSessionExample();
			try
			{
				example.Run(args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			Console.ReadLine();
		}

		private static bool TryGetRemote(
			string input,
			out ZfpUtil.Remote remote)
		{
			switch (input) {
				case Port8194:
					remote = ZfpUtil.Remote.Remote_8194;
					return true;
				case Port8196:
					remote = ZfpUtil.Remote.Remote_8196;
					return true;
				default:
					Console.WriteLine($"Invalid ZFP port '{input}'");
					remote = default(ZfpUtil.Remote);
					return false;
			}
		}
	}

}
