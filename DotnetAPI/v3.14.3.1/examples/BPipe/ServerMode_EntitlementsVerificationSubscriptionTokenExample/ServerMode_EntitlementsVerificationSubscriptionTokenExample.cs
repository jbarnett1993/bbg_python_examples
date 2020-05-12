/* Copyright 2012. Bloomberg Finance L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:  The above copyright notice and this
 * permission notice shall be included in all copies or substantial portions of
 * the Software.  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES
 * OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
 
// This program demonstrates a server mode application that authorizes its
// users with tokens returned by a generateToken request. For the purposes
// of this demonstration, the "GetAuthorizationToken" program can be used
// to generate a token and display it on the console. For ease of demonstration
// this application takes one or more 'tokens' on the command line. But in a real
// server mode application the 'token' would be received from the client
// applications using some IPC mechanism.
//
// Workflow:
// * connect to server
// * open services
// * generate application token
// * send authorization request for application
// * send authorization request for each 'token' which represents a user.
// * subscribe to all specified 'securities' using application Identity
// * for each subscription data message, check which users are entitled to 
//   receive that data before distributing that message to the user.
//
// Command line arguments:
// -ip <serverHostNameOrIp>
// -p  <serverPort>
// -t  <user's token>
// -s  <security>
// -f  <field>
// -a  <application name authentication>
// Multiple securities and tokens can be specified but the application
// is limited to one field.
// You can use the ServerMode_GetAuthorizationToken sample to generate the tokens.
using System;
using ArrayList = System.Collections.ArrayList;
using Session = Bloomberglp.Blpapi.Session;
using Name = Bloomberglp.Blpapi.Name;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Service = Bloomberglp.Blpapi.Service;
using Identity = Bloomberglp.Blpapi.Identity;
using Request = Bloomberglp.Blpapi.Request;
using Element = Bloomberglp.Blpapi.Element;
using EventHandler = Bloomberglp.Blpapi.EventHandler;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using EventQueue = Bloomberglp.Blpapi.EventQueue;
using Subscription = Bloomberglp.Blpapi.Subscription;
using System.Collections.Generic;

namespace ServerMode_EntitlementsVerificationSubscriptionTokenExample
{
    class ServerMode_EntitlementsVerificationSubscriptionTokenExample
    {
       private String d_host;
        private int d_port;
        private String d_field;
        private Name d_fieldAsName;
        private List<string> d_securities;
        private List<string> d_tokens;
        private List<Identity> d_identities;
        private String d_appName;
        private Identity d_appIdentity;

        private Session d_session;
        private Service d_apiAuthSvc;
        private Service d_blpMktDataSvc;
        private System.Collections.Generic.List<Subscription> d_subscriptions;
        private static readonly Name AUTHORIZATION_SUCCESS = Name.GetName("AuthorizationSuccess");
        private static readonly Name AUTHORIZATION_FAILURE = Name.GetName("AuthorizationFailure");
        private static readonly Name TOKEN_SUCCESS = Name.GetName("TokenGenerationSuccess");
        private static readonly Name TOKEN_FAILURE = Name.GetName("TokenGenerationFailure");
        private Name EID = Name.GetName("EID");

        private const String AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
        private const String API_AUTH_SVC_NAME = "//blp/apiauth";
        private const String MKT_DATA_SVC_NAME = "//blp/mktdata";

        public static void Main(String[] args)
        {
            System.Console.WriteLine("Entitlements Verification Subscription Token Example");
            ServerMode_EntitlementsVerificationSubscriptionTokenExample example =
                new ServerMode_EntitlementsVerificationSubscriptionTokenExample();
            example.run(args);
            System.Console.WriteLine("Press ENTER to quit");
            System.Console.ReadLine();
        }

        public ServerMode_EntitlementsVerificationSubscriptionTokenExample()
        {
            
            d_host = "localhost";
            d_port = 8194;
            d_field = "BEST_BID1";
            d_fieldAsName = Name.GetName(d_field);
            d_appName = string.Empty;

            d_securities = new List<string>();
            d_identities = new List<Identity>();
            d_tokens = new List<string>();
            d_subscriptions = new System.Collections.Generic.List<Subscription>();
        }

        private void run(String[] args)
        {
            string token = string.Empty;

            if (!parseCommandLine(args))
                return;

            createSession();
            OpenServices();


		    // Generate server side Application Name token
            if (GenerateApplicationToken(out token))
            {
                // Authorize server side Application Name Identity for use with request/subscription
                if (authorizeApplication(token, out d_appIdentity))
                {
                    // Authorize all the users that are interested in receiving data
                    if (authorizeUsers())
                        // Make the various requests that we need to make with application's Identity
                        d_session.Subscribe(d_subscriptions, d_appIdentity);
                }
            }
        }

        private void createSession()
        {
            SessionOptions options = new SessionOptions();
            options.ServerHost = d_host;
            options.ServerPort = d_port;
            options.AuthenticationOptions = AUTH_APP_PREFIX + d_appName.Trim();

            System.Console.WriteLine("Connecting to " + d_host + ":" + d_port);

            d_session = new Session(options, new EventHandler(this.processEvent));
            bool sessionStarted = d_session.Start();
            if (!sessionStarted)
            {
                System.Console.WriteLine("Failed to start session. Exiting...");
                System.Environment.Exit(-1);
            }
        }

        private void OpenServices()
        {
            if (!d_session.OpenService(API_AUTH_SVC_NAME))
            {
                System.Console.WriteLine("Failed to open service: " + API_AUTH_SVC_NAME);
                System.Environment.Exit(-1);
            }

            if (!d_session.OpenService(MKT_DATA_SVC_NAME))
            {
                System.Console.WriteLine("Failed to open service: " + MKT_DATA_SVC_NAME);
                System.Environment.Exit(-2);
            }

            d_apiAuthSvc = d_session.GetService(API_AUTH_SVC_NAME);
            d_blpMktDataSvc = d_session.GetService(MKT_DATA_SVC_NAME);
        }

        public void processSubscriptionDataEvent(Event eventObj, Session session)
        {
            foreach (Message msg in eventObj)
            {
                if (!msg.HasElement(d_fieldAsName)) continue;
                string topic = (string)msg.CorrelationID.Object;
                System.Console.WriteLine(System.DateTime.Now.ToString("s")
                    + ": " + topic + " - " + msg.MessageType);
                Element field = msg.GetElement(d_fieldAsName);
                if (field.IsNull)
                {
                    System.Console.WriteLine(d_field + "is null, ignoring");
                }
                Service service = msg.Service;
                bool needsEntitlement = msg.HasElement(EID);
                for (int j = 0; j < d_identities.Count; ++j)
                {
                    Identity identity = (Identity)d_identities[j];
                    if (!needsEntitlement ||
                        identity.HasEntitlements(msg.GetElement(EID), service))
                    {
                        System.Console.WriteLine("User: " + (j+1) +
                            " is entitled to " + d_field);
                        // Now Distribute message to the user. 
                    }
                    else
                    {
                        System.Console.WriteLine("User: " + (j+1) +
                            " is NOT entitled for " + d_field);
                    }
                }
            }
        }

        public void processEvent(Event eventObj, Session session)
        {
            try
            {
                switch (eventObj.Type)
                {
                    case Event.EventType.SESSION_STATUS:
                    case Event.EventType.SERVICE_STATUS:
                    case Event.EventType.REQUEST_STATUS:
                    case Event.EventType.AUTHORIZATION_STATUS:
                    case Event.EventType.SUBSCRIPTION_STATUS:
                        printEvent(eventObj);
                        break;

                    case Event.EventType.SUBSCRIPTION_DATA:
                        processSubscriptionDataEvent(eventObj, session);
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }

        private bool GenerateApplicationToken(out string token)
        {
            bool isTokenSuccess = false;
            bool isRunning = false;

            token = string.Empty;
            CorrelationID tokenReqId = new CorrelationID(99);
            EventQueue tokenEventQueue = new EventQueue();
            
            System.Console.WriteLine("Application token generation");
            d_session.GenerateToken(tokenReqId, tokenEventQueue);

            while (!isRunning)
            {
                Event eventObj = tokenEventQueue.NextEvent();
                if (eventObj.Type == Event.EventType.TOKEN_STATUS)
                {
                    System.Console.WriteLine("processTokenEvents");
                    foreach (Message msg in eventObj)
                    {
                        System.Console.WriteLine(msg.ToString());
                        if (msg.MessageType == TOKEN_SUCCESS)
                        {
                            token = msg.GetElementAsString("token");
                            System.Console.WriteLine("Application token sucess");
                            isTokenSuccess = true;
                            isRunning = true;
                            break;
                        }
                        else if (msg.MessageType == TOKEN_FAILURE)
                        {
                            System.Console.WriteLine("Application token failure");
                            isRunning = true;
                            break;
                        }
                        else
                        {
                            System.Console.WriteLine("Error while application token generation");
                            isRunning = true;
                            break;
                        }
                    }
                }
            }

            return isTokenSuccess;
        }

        private bool authorizeApplication(string token, out Identity identity)
        {
            bool isAuthorized = false;
            bool isRunning = true;
            identity = null;

            if (!d_session.OpenService(API_AUTH_SVC_NAME))
            {
                System.Console.Error.WriteLine("Failed to open " + API_AUTH_SVC_NAME);
                return (isAuthorized = false);

            }
            Service authService = d_session.GetService(API_AUTH_SVC_NAME);


            Request authRequest = authService.CreateAuthorizationRequest();

            authRequest.Set("token", token);
            identity = d_session.CreateIdentity();
            EventQueue authEventQueue = new EventQueue();

            d_session.SendAuthorizationRequest(authRequest, identity, authEventQueue, new CorrelationID(1));

            while (isRunning)
            {
                Event eventObj = authEventQueue.NextEvent();
                System.Console.WriteLine("processEvent");
                if (eventObj.Type == Event.EventType.RESPONSE || eventObj.Type == Event.EventType.REQUEST_STATUS)
                {
                    foreach (Message msg in eventObj)
                    {
                        if (msg.MessageType == AUTHORIZATION_SUCCESS)
                        {
                            System.Console.WriteLine("Application authorization SUCCESS");

                            isAuthorized = true;
                            isRunning = false;
                            break;
                        }
                        else if (msg.MessageType == AUTHORIZATION_FAILURE)
                        {
                            System.Console.WriteLine("Application authorization FAILED");
                            System.Console.WriteLine(msg.ToString());
                            isRunning = false;
                        }
                        else
                        {
                            System.Console.WriteLine(msg.ToString());
                        }
                    }
                }
            }
            return isAuthorized;
        }

        private bool authorizeUsers()
        {
            bool is_any_user_authorized = false;
            // Authorize each of the users
            for (int i = 0; i < d_tokens.Count; ++i)
            {
                bool isRunning = true;
                Identity identity = d_session.CreateIdentity();
                d_identities.Add(identity);

                Request authRequest = d_apiAuthSvc.CreateAuthorizationRequest();

                authRequest.Set("token", d_tokens[i]);

                CorrelationID correlator = new CorrelationID(i);
                EventQueue eventQueue = new EventQueue();
                d_session.SendAuthorizationRequest(authRequest, identity,
                    eventQueue, correlator);

                Event eventObj = eventQueue.NextEvent();
                while (isRunning)
                {
                    if (eventObj.Type == Event.EventType.RESPONSE ||
                        eventObj.Type == Event.EventType.REQUEST_STATUS)
                    {
                        foreach (Message msg in eventObj)
                        {
                            if (msg.MessageType == AUTHORIZATION_SUCCESS)
                            {
                                System.Console.WriteLine("Authorization SUCCESS for user:" + (i + 1));
                                is_any_user_authorized = true;
                                isRunning = false;
                                break;
                            }
                            else if (msg.MessageType == AUTHORIZATION_FAILURE)
                            {
                                System.Console.WriteLine("Authorization FAILED for user:" + (i + 1));
                                System.Console.WriteLine(msg);
                                isRunning = false;
                                break;
                            }
                            else
                            {
                                System.Console.WriteLine(msg);
                                isRunning = false;
                            }
                        }
                    }
                }
            }
            return is_any_user_authorized;
        }
        private bool parseCommandLine(String[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (string.Compare(args[i], "-s", true) == 0
					&& i + 1 < args.Length)
                {
                    d_securities.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-f", true) == 0
					&& i + 1 < args.Length)
                {
                    d_field = args[++i];
                    d_fieldAsName = Name.GetName(d_field);
                }
                else if (string.Compare(args[i], "-a", true) == 0
					&& i + 1 < args.Length)
                {
                    d_appName = args[++i];
                }
                else if (string.Compare(args[i], "-t", true) == 0
					&& i + 1 < args.Length)
                {
                    d_tokens.Add(args[++i]);
                }
                else if (string.Compare(args[i], "-ip", true) == 0
					&& i + 1 < args.Length)
                {
                    d_host = args[++i];
                }
                else if (string.Compare(args[i], "-p", true) == 0
					&& i + 1 < args.Length)
                {
                    int outPort = 0;
                    if (int.TryParse(args[++i], out outPort))
                    {
                        d_port = outPort;
                    }
                }
                else if (string.Compare(args[i], "-h", true) == 0)
                {
                    printUsage();
                    return false;
                }
            }

            if (d_appName.Length == 0)
            {
                System.Console.WriteLine("No server side Application Name were specified");
                printUsage();
                return false;
            }

            if (d_tokens.Count <= 0)
            {
                System.Console.WriteLine("No tokens were specified");
                printUsage();
                return false;
            }
           
            if (d_securities.Count <= 0)
            {
                d_securities.Add("MSFT US Equity");
            }

            foreach (String security in d_securities)
            {
                d_subscriptions.Add(new Subscription(security, d_field, "",
                                                     new CorrelationID(security)));
            }
            return true;
        }

        private void printUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("	Entitlements Verification Subscription example");
            System.Console.WriteLine("		[-s		<security   = MSFT US Equity>]");
            System.Console.WriteLine("		[-f		<field      = BEST_BID1>]");
            System.Console.WriteLine("		[-a		<application name authentication>]");
            System.Console.WriteLine("		[-t		<user's token string>]");
            System.Console.WriteLine("		[-ip 	<ipAddress  = localhost>]");
            System.Console.WriteLine("		[-p 	<tcpPort    = 8194>]");
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("Multiple securities and tokens can be" +
                " specified. Only one field can be specified.");

        }

        private void printEvent(Event eventObj)
        {
            foreach (Message msg in eventObj)
            {
                CorrelationID correlationId = msg.CorrelationID;
                if (correlationId != null)
                {
                    System.Console.WriteLine("Correlator: " + correlationId);
                }
              System.Console.WriteLine(msg);
            }
        }

    }
}
