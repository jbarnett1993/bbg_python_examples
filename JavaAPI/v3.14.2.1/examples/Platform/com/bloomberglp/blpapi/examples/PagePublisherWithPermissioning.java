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
package com.bloomberglp.blpapi.examples;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashSet;
import java.util.Set;

import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.EventFormatter;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.ProviderEventHandler;
import com.bloomberglp.blpapi.ProviderSession;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Topic;
import com.bloomberglp.blpapi.TopicList;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.Identity.SeatType;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class PagePublisherWithPermissioning {
	private static final Name TOPIC_SUBSCRIBED      = Name.getName("TopicSubscribed");
	private static final Name TOPIC_UNSUBSCRIBED    = Name.getName("TopicUnsubscribed");
	private static final Name TOPIC_CREATED         = Name.getName("TopicCreated");
	private static final Name TOPIC_RECAP           = Name.getName("TopicRecap");
	private static final Name PERMISSION_REQUEST    = Name.getName("PermissionRequest");
	private static final Name TOPIC                 = Name.getName("topic");
	private static final Name START_COL             = Name.getName("startCol");
	private static final Name TOKEN_SUCCESS         = Name.getName("TokenGenerationSuccess");
	private static final Name TOKEN_FAILURE         = Name.getName("TokenGenerationFailure");
	private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
	private static final Name AUTHORIZATION_FAILURE = Name.getName("AuthorizationFailure");
	private static final Name SESSION_TERMINATED    = Name.getName("SessionTerminated");

	private static final String AUTH_USER        = "AuthenticationType=OS_LOGON";
	private static final String AUTH_APP_PREFIX  = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
	private static final String AUTH_DIR_PREFIX  = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";
	private static final String AUTH_OPTION_NONE = "none";
	private static final String AUTH_OPTION_USER = "user";
	private static final String AUTH_OPTION_APP  = "app=";
	private static final String AUTH_OPTION_DIR  = "dir=";

	private static Set<Topic> d_topicSet = new HashSet<Topic>();

	private String              d_service = "//viper/page";
	private int                 d_verbose = 0;
	private ArrayList<String>   d_hosts = new ArrayList<String>();
	private int                 d_port = 8194;
	private ArrayList<Integer>  d_eids = new ArrayList<Integer>();

	private String              d_token = null;
	private String              d_authOptions = AUTH_USER;
	private Boolean             d_tokenGenerationResponse = null;
	private Boolean             d_authorizationResponse = null;
	private CorrelationID       d_authorizationResponseCorrelationId = null;

	private static volatile boolean g_running = true;

	class MyEventHandler implements ProviderEventHandler {

		public void processEvent(Event event, ProviderSession session) {
			try {
				doProcessEvent(event, session);
			}
			catch (Exception e) {
				// don't let exceptions thrown by the library go back
				// into the library unnoticed
				e.printStackTrace();
			}
		}

		private void doProcessEvent(Event event, ProviderSession session) {
			TopicList topicList = new TopicList();

			if (d_verbose > 0) {
				System.out.println("Received event " + event.eventType());
				for (Message msg: event) {
					System.out.println("cid = " + msg.correlationID());
					System.out.println("Message = " + msg);
				}
			}

			if (event.eventType() == EventType.SESSION_STATUS) {
				for (Message msg: event) {
					if (msg.messageType() == SESSION_TERMINATED) {
						g_running = false;
						break;
					}
				}
			}
			if (event.eventType() == EventType.TOKEN_STATUS) {
				for (Message msg: event) {
					Object tokenResponseMonitor = msg.correlationID().object();
					synchronized (tokenResponseMonitor) {
						if (msg.messageType() == TOKEN_SUCCESS) {
							d_tokenGenerationResponse = Boolean.TRUE;
							d_token = msg.getElementAsString("token");
						} else if (msg.messageType() == TOKEN_FAILURE) {
							d_tokenGenerationResponse = Boolean.FALSE;
							if (d_verbose == 0) {
								System.err.println(msg);
							}
						}
						tokenResponseMonitor.notifyAll();
					}
				}
			}
			else if (event.eventType() == EventType.TOKEN_STATUS) {
				for (Message msg: event) {
					if (d_verbose > 0)
						System.out.println(msg);
					if (msg.messageType() == TOKEN_SUCCESS) {
						Object tokenReady = msg.correlationID().object();
						synchronized (tokenReady) {
							d_token = msg.getElementAsString("token");
							tokenReady.notifyAll();
						}
					}
				}
			} else if (event.eventType() == EventType.TOPIC_STATUS) {
				for (Message msg: event) {
					if (d_verbose > 0)
						System.out.println(msg);
					if (!msg.asElement().getElementAsString(TOPIC).startsWith(d_service)) {
						continue;
					}
					if (msg.messageType() == TOPIC_SUBSCRIBED) {
						Topic topic = session.getTopic(msg);
						if (topic == null) {
							CorrelationID cid = new CorrelationID(msg.getElementAsString("topic"));
							topicList.add(msg, cid);
						}
						else {
							synchronized (d_topicSet) {
								if (d_topicSet.add(topic))
									d_topicSet.notifyAll();
							}
						}
					}
					else if (msg.messageType() == TOPIC_UNSUBSCRIBED) {
						Topic topic = session.getTopic(msg);
						synchronized (d_topicSet) {
							d_topicSet.remove(topic);
						}
					}
					else if (msg.messageType() == TOPIC_CREATED) {
						Topic topic = session.getTopic(msg);
						synchronized (d_topicSet) {
							if (d_topicSet.add(topic))
								d_topicSet.notifyAll();
						}
					}
					else if (msg.messageType() == TOPIC_RECAP) {
						Topic topic = session.getTopic(msg);
						if (topic != null) {
							// send initial paint, this should come from my own
							// cache
							Service service = session.getService(d_service);
							Event recapEvent = service.createPublishEvent();
							EventFormatter eventFormatter = new EventFormatter(recapEvent);
							eventFormatter.appendRecapMessage(topic, msg.correlationID());
							eventFormatter.setElement("numRows", 25);
							eventFormatter.setElement("numCols", 80);
							eventFormatter.pushElement("rowUpdate");
							for (int i = 0; i < 5; ++i) {
								eventFormatter.appendElement();
								eventFormatter.setElement("rowNum", i);
								eventFormatter.pushElement("spanUpdate");
								eventFormatter.appendElement();
								eventFormatter.setElement("startCol", 1);
								eventFormatter.setElement("length", 5);
								eventFormatter.setElement("text", "RECAP");
								eventFormatter.popElement();
								eventFormatter.popElement();
								eventFormatter.popElement();
							}
							eventFormatter.popElement();
							session.publish(recapEvent);
						}
					}
				}

				if (topicList.size() > 0) {
					session.createTopicsAsync(topicList);
				}
			}
			else if (event.eventType() == EventType.REQUEST) {
				Service service = session.getService(d_service);
				for (Message msg: event) {
					if (d_verbose > 0)
						System.out.println(msg);
					if (msg.messageType() == PERMISSION_REQUEST) {
						// Similar to createPublishEvent. We assume just one
						// service - d_service. A responseEvent can only be for
						// single request so we can specify the correlationId -
						// which establishes context - when we create the Event.
						Event response = service.createResponseEvent(msg.correlationID());
						EventFormatter ef = new EventFormatter(response);
						// In appendResponse the string is the name of the
						// operation, the correlationId indicates
						// which request we are responding to.
						int permission = 1; // ALLOWED: 0, DENIED: 1
						if (msg.hasElement("uuid")) {
							int uuid = msg.getElementAsInt32("uuid");
							if (d_verbose > 0)
								System.out.println("UUID = " + uuid);
							permission = 0;
						}
						if (msg.hasElement("applicationId")) {
							int applicationId = msg.getElementAsInt32("applicationId");
							if (d_verbose > 0)
								System.out.println("APPID = " + applicationId);
							permission = 0;
						}
						if (msg.hasElement("seatType")) {
							SeatType seatType = SeatType.fromInt(msg.getElementAsInt32("seatType"));
							if (seatType == Identity.SeatType.INVALID_SEAT) {
								permission = 1;
							}
							else {
								permission = 0;
							}
						}
						ef.appendResponse("PermissionResponse");
						ef.pushElement("topicPermissions");
						// For each of the topics in the request, add an entry
						// to the response
						Element topicsElement = msg.getElement(Name.getName("topics"));
						for (int i = 0; i < topicsElement.numValues(); ++i) {
							ef.appendElement();
							ef.setElement("topic", topicsElement.getValueAsString(i));
							ef.setElement("result", permission); // ALLOWED: 0, DENIED: 1

							if (permission == 1) {// DENIED
								ef.pushElement("reason");
								ef.setElement("source", "My Publisher Name");
								ef.setElement("category", "NOT_AUTHORIZED"); // or BAD_TOPIC, or custom
								ef.setElement("subcategory", "Publisher Controlled");
								ef.setElement("description", "Permission denied by My Publisher Name");
								ef.popElement();
							}
							else { // ALLOWED
								if (!d_eids.isEmpty()) {
									ef.pushElement("permissions");
									ef.appendElement();
									ef.setElement("permissionService", "//blp/blpperm");
									ef.pushElement("eids");
									for (int j = 0; j < d_eids.size(); ++j) {
										ef.appendValue(d_eids.get(j));
									}
									ef.popElement();
									ef.popElement();
									ef.popElement();
								}
							}
							ef.popElement();
						}
						ef.popElement();
						// Service is implicit in the Event. sendResponse has a
						// second parameter - partialResponse -
						// that defaults to false.
						session.sendResponse(response);
					}
				}
			}
			else if (event.eventType() == EventType.RESPONSE
					|| event.eventType() == EventType.PARTIAL_RESPONSE
					|| event.eventType() == EventType.REQUEST_STATUS) {
				for (Message msg: event) {
					if (msg.correlationID().equals(d_authorizationResponseCorrelationId)) {
						Object authorizationResponseMonitor = msg.correlationID().object();
						synchronized (authorizationResponseMonitor) {
							if (msg.messageType() == AUTHORIZATION_SUCCESS) {
								d_authorizationResponse = Boolean.TRUE;
								authorizationResponseMonitor.notifyAll();
							}
							else if (msg.messageType() == AUTHORIZATION_FAILURE) {
								d_authorizationResponse = Boolean.FALSE;
								System.err.println("Not authorized: " + msg.getElement("reason"));
								authorizationResponseMonitor.notifyAll();
							}
							else {
								assert d_authorizationResponse == Boolean.TRUE;
								System.out.println("Permissions updated");
							}
						}
					}
				}
			}
		}
	}

	private void printUsage() {
		System.out.println("Usage:");
		System.out.println("  Publish on a topic ");
		System.out.println("	-v					verbose, use multiple times to increase verbosity");
		System.out.println("	-ip   <ipAddress>	server name or IP (default = localhost)");
		System.out.println("	-p    <tcpPort>		server port (default = 8194)");
		System.out.println("	-s    <service>		service name (default = //viper/page>)");
		System.out.println("	-auth <option>		authentication option: user|none|app=<app>|dir=<property> (default = user)");
	}

	private boolean parseCommandLine(String[] args) {
		for (int i = 0; i < args.length; ++i) {

			if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
				d_service = args[++i];
			}
			else if (args[i].equalsIgnoreCase("-e") && i + 1 < args.length) {
				d_eids.add(Integer.parseInt(args[++i]));
			}
			else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
				d_hosts.add(args[++i]);
			}
			else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
				d_port = Integer.parseInt(args[++i]);
			}
			else if (args[i].equalsIgnoreCase("-v")) {
				++d_verbose;
			}
			else if (args[i].equalsIgnoreCase("-auth")  && i + 1 < args.length) {
				++i;
				if (args[i].equalsIgnoreCase(AUTH_OPTION_NONE)) {
					d_authOptions = null;
				} else if (args[i].equalsIgnoreCase(AUTH_OPTION_USER)) {
					d_authOptions = AUTH_USER;
				} else if (args[i].regionMatches(true, 0, AUTH_OPTION_APP,
												0, AUTH_OPTION_APP.length())) {
					d_authOptions = AUTH_APP_PREFIX
							+ args[i].substring(AUTH_OPTION_APP.length());
				} else if (args[i].regionMatches(true, 0, AUTH_OPTION_DIR,
												0, AUTH_OPTION_DIR.length())) {
					d_authOptions = AUTH_DIR_PREFIX
							+ args[i].substring(AUTH_OPTION_DIR.length());
				} else {
					printUsage();
					return false;
				}
			}
			else {
				printUsage();
				return false;
			}
		}

		if (d_hosts.isEmpty()) {
			d_hosts.add("localhost");
		}

		return true;
	}

	private void printMessage(Event event) {
		for (Message msg: event) {
			System.out.println(msg);
		}
	}

	public void run(String[] args) throws Exception {
		if (!parseCommandLine(args))
			return;

		ServerAddress[] servers = new ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) {
			servers[i] = new ServerAddress(d_hosts.get(i), d_port);
		}

		SessionOptions sessionOptions = new SessionOptions();
		sessionOptions.setServerAddresses(servers);
		sessionOptions.setAuthenticationOptions(d_authOptions);
		sessionOptions.setAutoRestartOnDisconnection(true);
		sessionOptions.setNumStartAttempts(servers.length);

		System.out.print("Connecting to");
		for (ServerAddress server: sessionOptions.getServerAddresses()) {
			System.out.print(" " + server);
		}
		System.out.println();

		ProviderSession session = new ProviderSession(
				sessionOptions,
				new MyEventHandler());

		if (!session.start()) {
			System.err.println("Failed to start session");
			return;
		}

		Identity identity = null;
		if (d_authOptions != null) {
			Object tokenResponseMonitor = new Object();
			synchronized (tokenResponseMonitor) {
				session.generateToken(new CorrelationID(tokenResponseMonitor));
				long waitTime = 10 * 1000;
				long tokenResponseTimeout = System.currentTimeMillis() + waitTime;
				while(d_tokenGenerationResponse == null && waitTime > 0) {
					tokenResponseMonitor.wait(waitTime);
					waitTime = tokenResponseTimeout - System.currentTimeMillis();
				}
				if (d_tokenGenerationResponse == null) {
					System.err.println("Timeout waiting for token");
					return;
				}
				else if (d_tokenGenerationResponse == Boolean.FALSE || d_token == null) {
					System.err.println("Token generation failed");
					return;
				}
			}

			Object authorizationResponseMonitor = new Object();
			if (session.openService("//blp/apiauth")) {
				Service authService = session.getService("//blp/apiauth");
				Request authRequest = authService.createAuthorizationRequest();
				authRequest.set("token", d_token);
				identity = session.createIdentity();
				d_authorizationResponseCorrelationId =
					new CorrelationID(authorizationResponseMonitor);
				synchronized (authorizationResponseMonitor) {
					session.sendAuthorizationRequest(
							authRequest,
							identity,
							d_authorizationResponseCorrelationId);
					long waitTime = 60 * 1000;
					long authorizationResponseTimeout = System.currentTimeMillis() + waitTime;
					while(d_authorizationResponse == null && waitTime > 0) {
						authorizationResponseMonitor.wait(1000);
						waitTime = authorizationResponseTimeout - System.currentTimeMillis();
					}
					if (d_authorizationResponse == null) {
						System.err.println("Timeout waiting for authorization");
						System.exit(1);
					}
					else if (d_authorizationResponse == Boolean.FALSE) {
						System.err.println("Authorization failed");
						return;
					}
				}
			}
		}

		if (!session.registerService(d_service, identity)) {
			System.err.println("Failed to register " + d_service);
			return;
		}

		System.out.println("Service registered " + d_service);

		Service service = session.getService(d_service);
		while (g_running) {
			Event event;
			synchronized (d_topicSet) {
				try {
					if (d_topicSet.isEmpty())
						d_topicSet.wait(100);
				}
				catch (InterruptedException e) {
				}
				if (d_topicSet.isEmpty())
					continue;

				System.out.println("Publishing");
				event = service.createPublishEvent();
				EventFormatter eventFormatter = new EventFormatter(event);

				for (Topic topic: d_topicSet) {
					if (!topic.isActive()) {
						System.out.println("[WARNING] Publishing on an inactive topic.");
					}
					String os = (new Date()).toString();

					eventFormatter.appendMessage("RowUpdate", topic);
					eventFormatter.setElement("rowNum", 1);
					eventFormatter.pushElement("spanUpdate");
					eventFormatter.appendElement();
					eventFormatter.setElement(START_COL, 1);
					eventFormatter.setElement("length", os.length());
					eventFormatter.setElement("text", os);
					eventFormatter.popElement();
					eventFormatter.popElement();
				}
			}

			if (d_verbose > 1) {
				printMessage(event);
			}

			session.publish(event);
			Thread.sleep(20 * 1000);
		}

		session.stop();
	}

	public static void main(String[] args) throws Exception {
		System.out.println("PagePublisherWithPermissioning");
		PagePublisherWithPermissioning example = new PagePublisherWithPermissioning();
		example.run(args);
	}
}


