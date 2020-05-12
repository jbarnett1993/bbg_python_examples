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
 * 
 * All materials including all software, equipment and documentation made 
 * available by Bloomberg are for informational purposes only. Bloomberg and its 
 * affiliates make no guarantee as to the adequacy, correctness or completeness 
 * of, and do not make any representation or warranty (whether express or 
 * implied) or accept any liability with respect to, these materials. No right, 
 * title or interest is granted in or to these materials and you agree at all 
 * times to treat these materials in a confidential manner. All materials and 
 * services provided to you by Bloomberg are governed by the terms of any 
 * applicable Bloomberg Agreement(s).
 */

package com.bloomberglp.blpapi.examples;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.ArrayList;
import java.util.logging.Level;
import java.io.IOException;

import javax.swing.text.DateFormatter;

import com.bloomberglp.blpapi.Datetime;
import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.EventQueue;
import com.bloomberglp.blpapi.CorrelationID;
import com.bloomberglp.blpapi.Element;
import com.bloomberglp.blpapi.InvalidRequestException;
import com.bloomberglp.blpapi.Logging;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.MessageIterator;
import com.bloomberglp.blpapi.Name;
import com.bloomberglp.blpapi.Request;
import com.bloomberglp.blpapi.Service;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.Identity;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

public class Msg1RecoveryExample {
	private String DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss";
	private String MSG1_SERVICE = "//blp/msgscrape";
	private String AUTH_SVC = "//blp/apiauth";
	private String REPLAY = "replay";
	private String STATUS_INFO = "statusInfo";

	private static final Name ERROR_RESPONSE = new Name("errorResponse");
	private static final Name REPLAY_RESPONSE = new Name("replayResponse");
	private static final Name ERROR_MESSAGE = new Name("errorMsg");
	private static final Name MARKET_DATAS = new Name("marketDatas");
	private static final Name START = new Name("start");
	private static final Name END = new Name("end");
	private static final Name FILTER = new Name("filter");
	private static final Name EID = new Name("eid");
	private static final Name SERIAL = new Name("serial");
	private static final Name TIME = new Name("time");
	private static final Name AUTHORIZATION_SUCCESS = Name
			.getName("AuthorizationSuccess");
	private static final Name AUTHORIZATION_FAILURE = Name
			.getName("AuthorizationFailure");
	private static final Name TOKEN_SUCCESS = Name
			.getName("TokenGenerationSuccess");
	private static final Name TOKEN_FAILURE = Name
			.getName("TokenGenerationFailure");

	private enum FilterChoiceType {
		ALL, LAST_UPDATE_ONLY,
	};

	private ArrayList<String> d_hosts;
	private int d_port;
	private String d_authOption;
	private String d_dsName;
	private String d_name;
	private Identity d_identity;
	private String d_requestType;
	private Name d_startType;
	private Name d_endType;
	private int d_startSerial;
	private int d_endSerial;
	private Datetime d_startTime;
	private Datetime d_endTime;
	private FilterChoiceType d_filter;
	private int d_eid;
	private boolean d_eidProvided;

	private Session d_session;

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Exception {
		System.out.println("Msg1 Recovery Example");
		Msg1RecoveryExample example = new Msg1RecoveryExample();
		example.run(args);

		System.out.println("Press ENTER to quit");
		System.in.read();
	}

	public Msg1RecoveryExample() {
		d_port = 8194;
		d_hosts = new ArrayList<String>();
		d_startType = SERIAL;
		d_startSerial = 0;
		d_endType = TIME;
		Date dt = new Date();
		d_endTime = new Datetime(dt.getYear() + 1900, dt.getMonth() + 1,
				dt.getDate(), dt.getHours(), dt.getMinutes(), dt.getSeconds(),
				0);
		;
		d_filter = FilterChoiceType.LAST_UPDATE_ONLY;
		d_eid = 0;
		d_eidProvided = false;
		d_requestType = STATUS_INFO;
		d_authOption = "LOGON";
		d_dsName = "";
		d_name = "";
	}

	private boolean createSession() throws IOException, InterruptedException {
		String authOptions = null;
		if (d_authOption == "APPLICATION") {
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions += "ApplicationName=" + d_name;
		} else if (d_authOption == "USER_APP") {
			// Set User and Application Authentication Option
			authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
			authOptions += "AuthenticationType=OS_LOGON;";
			authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions += "ApplicationName=" + d_name;
		} else if (d_authOption == "USER_DS_APP") {
			// Set User and Application Authentication Option
			authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
			authOptions += "AuthenticationType=DIRECTORY_SERVICE;";
			authOptions += "DirSvcPropertyName=" + d_dsName + ";";
			authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions += "ApplicationName=" + d_name;
		} else if (d_authOption == "DIRSVC") {
			// Authenticate user using active directory service property
			authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
			authOptions += "DirSvcPropertyName=" + d_dsName;
		} else {
			// Authenticate user using windows/unix login name
			authOptions = "AuthenticationType=OS_LOGON";
		}

		System.out.println("authOptions = " + authOptions);
		SessionOptions sessionOptions = new SessionOptions();
		if (d_authOption != null) {
			sessionOptions.setAuthenticationOptions(authOptions);
		}

		ServerAddress[] servers = new ServerAddress[d_hosts.size()];
		for (int i = 0; i < d_hosts.size(); ++i) {
			servers[i] = new ServerAddress(d_hosts.get(i), d_port);
		}

		sessionOptions.setServerAddresses(servers);
		sessionOptions.setAutoRestartOnDisconnection(true);
		sessionOptions.setNumStartAttempts(d_hosts.size());

		System.out.print("Connecting to port " + d_port + " on server:");
		for (ServerAddress server : sessionOptions.getServerAddresses()) {
			System.out.print(" " + server);
		}
		System.out.println();
		d_session = new Session(sessionOptions);

		return d_session.start();
	}

	private boolean authorize() throws IOException, InterruptedException {
		Event event;
		MessageIterator msgIter;

		EventQueue tokenEventQueue = new EventQueue();
		CorrelationID corrlationId = new CorrelationID(99);
		d_session.generateToken(corrlationId, tokenEventQueue);
		String token = null;
		int timeoutMilliSeonds = 10000;
		event = tokenEventQueue.nextEvent(timeoutMilliSeonds);
		if (event.eventType() == EventType.TOKEN_STATUS) {
			MessageIterator iter = event.messageIterator();
			while (iter.hasNext()) {
				Message msg = iter.next();
				System.out.println(msg.toString());
				if (msg.messageType() == TOKEN_SUCCESS) {
					token = msg.getElementAsString("token");
				}
			}
		}
		if (token == null) {
			System.err.println("Failed to get token");
			return false;
		}

		if (d_session.openService(AUTH_SVC)) {
			Service authService = d_session.getService(AUTH_SVC);
			Request authRequest = authService.createAuthorizationRequest();
			authRequest.set("token", token);

			EventQueue authEventQueue = new EventQueue();

			d_session.sendAuthorizationRequest(authRequest, d_identity,
					authEventQueue, new CorrelationID(d_identity));

			while (true) {
				event = authEventQueue.nextEvent();
				if (event.eventType() == EventType.RESPONSE
						|| event.eventType() == EventType.PARTIAL_RESPONSE
						|| event.eventType() == EventType.REQUEST_STATUS) {
					msgIter = event.messageIterator();
					while (msgIter.hasNext()) {
						Message msg = msgIter.next();
						System.out.println(msg);
						if (msg.messageType() == AUTHORIZATION_SUCCESS) {
							return true;
						} else {
							System.err.println("Not authorized");
							return false;
						}
					}
				}
			}
		}
		return false;
	}

	private void run(String[] args) throws Exception {
		if (!parseCommandLine(args))
			return;

		if (!createSession()) {
			System.err.println("Failed to start session.");
			return;
		}

		if (d_authOption != null) {
			d_identity = d_session.createIdentity();
			if (!authorize()) {
				return;
			}
		}

		System.out.println("Connected successfully.");
		if (!d_session.openService(MSG1_SERVICE)) {
			System.err.println("Failed to open " + MSG1_SERVICE);
			d_session.stop();
			return;
		}

		try {
			if (d_requestType == STATUS_INFO) {
				sendMSG1StatusRequest(d_session);
			} else if (d_requestType == REPLAY) {
				sendMSG1RecoverRequest(d_session);
			}
		} catch (InvalidRequestException e) {
			e.printStackTrace();
		}

		// wait for events from session.
		eventLoop(d_session);

		d_session.stop();
	}

	private void eventLoop(Session session) throws Exception {
		boolean done = false;
		while (!done) {
			Event event = session.nextEvent();
			if (event.eventType() == Event.EventType.PARTIAL_RESPONSE) {
				System.out.println("Processing Partial Response");
				processResponseEvent(event);
			} else if (event.eventType() == Event.EventType.RESPONSE) {
				System.out.println("Processing Response");
				processResponseEvent(event);
				done = true;
			} else {
				MessageIterator msgIter = event.messageIterator();
				while (msgIter.hasNext()) {
					Message msg = msgIter.next();
					System.out.println(msg.asElement());
					if (event.eventType() == Event.EventType.SESSION_STATUS) {
						if (msg.messageType().equals("SessionTerminated")
								|| msg.messageType().equals(
										"SessionStartupFailure")) {
							done = true;
						}
					}
				}
			}
		}
	}

	// return true if processing is completed, false otherwise
	private void processResponseEvent(Event event) throws Exception {
		MessageIterator msgIter = event.messageIterator();
		while (msgIter.hasNext()) {
			Message msg = msgIter.next();
			if (msg.hasElement(ERROR_RESPONSE)) {
				printErrorInfo("REQUEST FAILED: ",
						msg.getElement(ERROR_RESPONSE));
				continue;
			} else if (msg.asElement().name().equals(REPLAY_RESPONSE)) {
				System.out.println("# of Recovered data: "
						+ msg.getElement(MARKET_DATAS).numValues());
				continue;
			} else {
				System.out.println("Received Response: " + msg.toString());
				continue;
			}
		}
	}

	private void sendMSG1StatusRequest(Session session) throws Exception {
		Service service = session.getService(MSG1_SERVICE);
		Request request = service.createRequest(STATUS_INFO);
		if (d_eidProvided) {
			request.getElement(EID).setValue(d_eid);
		}
		System.out.println("Sending request: " + request.toString());

		// request data with identity object
		session.sendRequest(request, d_identity, null);
	}

	private void sendMSG1RecoverRequest(Session session) throws Exception {
		Service service = session.getService(MSG1_SERVICE);
		Request request = service.createRequest(REPLAY);
		request.getElement(START).setChoice(d_startType);
		if (d_startType == TIME) {
			request.getElement(START).setChoice(TIME).setValue(d_startTime);
		} else if (d_startType == SERIAL) {
			request.getElement(START).setChoice(SERIAL).setValue(d_startSerial);
		}
		if (d_endType == TIME) {
			request.getElement(END).setChoice(TIME).setValue(d_endTime);
		} else if (d_endType == SERIAL) {
			request.getElement(END).setChoice(SERIAL).setValue(d_endSerial);
		}
		if (d_eidProvided) {
			request.getElement(EID).setValue(d_eid);
		}
		request.getElement(FILTER).setValue(d_filter.name());
		System.out.println("Sending request: " + request.toString());

		// request data with identity object
		session.sendRequest(request, d_identity, null);
	}

	private boolean parseCommandLine(String[] args) {
		for (int i = 0; i < args.length; ++i) {
			if (args[i].equalsIgnoreCase("-s") && i + 1 < args.length) {
				d_requestType = REPLAY;
				String startArg = args[++i];
				try {
					d_startSerial = Integer.parseInt(startArg);
					d_startType = SERIAL;
					continue;
				} catch (Exception e) {
				}

				try {
					SimpleDateFormat formatter = new SimpleDateFormat(
							DATE_FORMAT);
					Date dt = formatter.parse(startArg);
					dt.parse(startArg);
					d_startTime = new Datetime(dt.getYear() + 1900,
							dt.getMonth() + 1, dt.getDate(), dt.getHours(),
							dt.getMinutes(), dt.getSeconds(), 0);
					;
					d_startType = TIME;
				} catch (Exception e) {
					System.out.println("Error: '" + startArg
							+ "' is not in the proper Datetime format: "
							+ DATE_FORMAT);
					printUsage();
					return false;
				}
			} else if (args[i].equalsIgnoreCase("-e") && i + 1 < args.length) {
				d_requestType = REPLAY;
				String endArg = args[++i];
				try {
					d_endSerial = Integer.parseInt(endArg);
					d_endType = SERIAL;
					continue;
				} catch (Exception e) {
				}

				try {
					SimpleDateFormat formatter = new SimpleDateFormat(
							DATE_FORMAT);
					Date dt = formatter.parse(endArg);
					d_endTime = new Datetime(dt.getYear() + 1900,
							dt.getMonth() + 1, dt.getDate(), dt.getHours(),
							dt.getMinutes(), dt.getSeconds(), 0);
					;
					d_endType = TIME;
				} catch (Exception e) {
					System.out.println("Error: '" + endArg
							+ "' is not in the proper Datetime format: "
							+ DATE_FORMAT);
					printUsage();
					return false;
				}
			} else if (args[i].equalsIgnoreCase("-f") && i + 1 < args.length) {
				String filter = args[++i];
				try {
					d_filter = FilterChoiceType.valueOf(filter);
					d_requestType = REPLAY;
				} catch (Exception e) {
					System.out.println("Error: '" + filter
							+ "' is not a supported filter type.");
					printUsage();
					return false;
				}
			} else if (args[i].equalsIgnoreCase("-eid") && i + 1 < args.length) {
				String eidArg = args[++i];
				try {
					d_eid = Integer.parseInt(eidArg);
					d_eidProvided = true;
				} catch (Exception e) {
					System.out.println("Error: '" + eidArg
							+ "' is not an integer");
					printUsage();
					return false;
				}
			} else if (args[i].equalsIgnoreCase("-ip") && i + 1 < args.length) {
				d_hosts.add(args[++i]);
			} else if (args[i].equalsIgnoreCase("-p") && i + 1 < args.length) {
				d_port = Integer.parseInt(args[++i]);
			} else if (args[i].equalsIgnoreCase("-auth") && i + 1 < args.length) {
				d_authOption = args[++i];
			} else if (args[i].equalsIgnoreCase("-ds") && i + 1 < args.length) {
				d_dsName = args[++i];
			} else if (args[i].equalsIgnoreCase("-n") && i + 1 < args.length) {
				d_name = args[++i];
			} else if (args[i].equalsIgnoreCase("-h")) {
				printUsage();
				return false;
			} else {
				System.out.println("Error: unknown argument" + args[i]);
				printUsage();
				return false;
			}
		}

		// check for application name
		if ((d_authOption.equalsIgnoreCase("APPLICATION") || d_authOption
				.equalsIgnoreCase("USER_APP")) && (d_name.equalsIgnoreCase(""))) {
			System.out
					.println("Application name cannot be NULL for application authorization.");
			printUsage();
			return false;
		}
		if (d_authOption.equalsIgnoreCase("USER_DS_APP")
				&& (d_name.equalsIgnoreCase("") || d_dsName
						.equalsIgnoreCase(""))) {
			System.out
					.println("Application name cannot be NULL for application authorization.");
			printUsage();
			return false;
		}
		// check for Directory Service name
		if ((d_authOption.equalsIgnoreCase("DIRSVC"))
				&& (d_name.equalsIgnoreCase(""))) {
			System.out
					.println("Directory Service property name cannot be NULL for DIRSVC authorization.");
			printUsage();
			return false;
		}

		// handle default arguments
		if (d_hosts.size() == 0) {
			d_hosts.add("localhost");
		}

		return true;
	}

	private void printErrorInfo(String leadingStr, Element errorInfo)
			throws Exception {
		System.out.println(leadingStr + " ("
				+ errorInfo.getElementAsString(ERROR_MESSAGE) + ")");
	}

	private void printUsage() {
		System.out.println("Usage:");
		System.out.println("	Retrieve MSG1 data ");
		System.out.println("      [-s     <start	    = 0>");
		System.out.println("      [-e     <end  	    = " + d_endTime + ">(in "
				+ DATE_FORMAT + " format)");
		System.out.println("      [-f     <filter	    = LAST_UPDATE_ONLY>");
		System.out.println("      [-eid          	    = an EID]");
		System.out.println("      [-ip    <ipAddress	= localhost>");
		System.out.println("      [-p     <tcpPort	= 8194>");
		System.out
				.println("      [-auth  <authenticationOption = LOGON(default) or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]");
		System.out.println("      [-n     <name = applicationName>]");
		System.out.println("      [-ds    <dsName = directoryService>]");
		System.out.println("Notes:");
		System.out
				.println("1) This example client make a status infomation query by default.");
		System.out
				.println("2) Specify start and end to request MSG1 recovory request.");
		System.out.println("Notes on MSG1 recovery:");
		System.out
				.println("1) Specify start as either a number (as serial id) or time (as timestamp).");
		System.out
				.println("2) Specify end as either a number (as serial id) or time (as timestamp).");
		System.out.println("3) Specify filter as 'ALL' or 'LAST_UPDATE_ONLY'");
		System.out
				.println("4) Sepcify the EID whose data needed to be recovered. This field is optional. It is only necessary when one B-PIPE client has multiple EIDs enabled and the EID specified is different from the EID mapped to the default proxy UUID.\n");
		System.out.println("Notes on authorization:");
		System.out
				.println("1) Specify only LOGON to authorize 'user' using Windows login name.");
		System.out
				.println("2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service.");
		System.out
				.println("3) Specify APPLICATION and name(Application Name) to authorize application.\n");
	}
}
