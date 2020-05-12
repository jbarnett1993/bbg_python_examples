/*
 * Copyright 2012. Bloomberg Finance L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:  The above
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
package com.bloomberglp.blpapi.examples;

import com.bloomberglp.blpapi.*;
import com.bloomberglp.blpapi.Event.EventType;
import com.bloomberglp.blpapi.SessionOptions.ServerAddress;

import java.io.IOException;
import java.util.ArrayList;

public class ConnectionAndAuthExample {
    private class HostAndPort {
        final String d_host;
        final int    d_port;

        HostAndPort(String host, int port) {
            d_host = host;
            d_port = port;
        }

        String host() {
            return d_host;
        }
        int port() {
            return d_port;
        }
    }
    private static final Name AUTHORIZATION_SUCCESS = Name.getName("AuthorizationSuccess");
    private static final Name TOKEN_SUCCESS         = Name.getName("TokenGenerationSuccess");

    private static final String AUTH_USER        = "AuthenticationType=OS_LOGON";
    private static final String AUTH_APP_PREFIX  = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private static final String AUTH_USER_APP_PREFIX = "AuthenticationMode=USER_AND_APPLICATION;AuthenticationType=OS_LOGON;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private static final String AUTH_USER_APP_MANUAL_PREFIX = "AuthenticationMode=USER_AND_APPLICATION;AuthenticationType=MANUAL;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private static final String AUTH_DIR_PREFIX  = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";
    private static final String AUTH_OPTION_NONE = "none";
    private static final String AUTH_OPTION_USER = "user";
    private static final String AUTH_OPTION_APP  = "app=";
    private static final String AUTH_OPTION_USER_APP = "userapp=";
    private static final String AUTH_OPTION_DIR  = "dir=";
    private static final String AUTH_OPTION_MANUAL = "manual=";

    private static final String d_defaultHost      = "localhost";
    private static final int    d_defaultPort      = 8194;

    private ArrayList<HostAndPort> d_hosts = new ArrayList<HostAndPort>();

    private String              d_authOptions = AUTH_USER;

    private String              d_clientCredentials = null;
    private String              d_clientCredentialsPassword = null;
    private String              d_trustMaterial = null;
    private String              d_manualUserName = null;
    private String              d_manualIPAddress = null;

    private void printUsage() {
        System.out.println(
"Usage:\n" +
"\t[-host <ipAddress:port>] server name or IP (default: localhost)\n" +
"\t[-p    <tcpPort>]        server port (default: 8194)\n" +
"\t[-auth <option>]         authentication option (default: user):\n" +
"\t\tnone\n" +
"\t\tuser                 as a user using OS logon information\n" +
"\t\tdir=<property>       as a user using directory services\n" +
"\t\tapp=<app>            as the specified application\n" +
"\t\tuserapp=<app>        as user and application using logon information\n" +
"\t\t                     for the user\n" +
"\t\tmanual=<app,ip,user> as user and application, with manually provided\n" +
"\t\t                     IP address and EMRS user\n" +
"\n" +
"TLS OPTIONS (specify all or none):\n" +
"\t[-tls-client-credentials <file>] \tname a PKCS#12 file to use as a source of client credentials\n" +
"\t[-tls-client-credentials-password <file>] \tspecify password for accessing client credentials\n" +
"\t[-tls-trust-material <file>]     \tname a PKCS#7 file to use as a source of trusted certificates\n");
    }

    private HostAndPort parseHostAndPort(String arg) {
        String params[] = arg.split(":");
        int port = 8194;
        if (params.length > 2) {
            System.err.println("Invalid argument to -host: " + arg);
            printUsage();
            return null;
        }
        if (params.length == 2) {
            try {
                port = Integer.parseInt(params[1]);
            } catch (NumberFormatException e) {
                System.err.println("Invalid argument to -host: " + arg);
                printUsage();
                return null;
            }
        }
        return new HostAndPort(params[0], port);
    }

    private boolean parseCommandLine(String[] args) {
        for (int i = 0; i < args.length; ++i) {
            if (args[i].equalsIgnoreCase("-host") && i + 1 < args.length) {
                ++i;
                HostAndPort host = parseHostAndPort(args[i]);
                if (host == null) {
                    return false;
                }
                d_hosts.add(host);
            } else if (args[i].equalsIgnoreCase("-auth") && i + 1 < args.length) {
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
                } else if (args[i].regionMatches(true, 0, AUTH_OPTION_USER_APP,
                                            0, AUTH_OPTION_USER_APP.length())) {
                    d_authOptions = AUTH_USER_APP_PREFIX
                            + args[i].substring(AUTH_OPTION_USER_APP.length());

                } else if (args[i].regionMatches(true, 0, AUTH_OPTION_MANUAL,
                        0, AUTH_OPTION_MANUAL.length())) {
                    String[] params = args[i].substring(AUTH_OPTION_MANUAL.length()).split(",");
                    for (String s : params) {
                        System.out.println("\t\ttoken:" + s + "\n");
                    }
                    if (params.length != 3) {
                        printUsage();
                        return false;
                    }
                    d_authOptions = AUTH_USER_APP_MANUAL_PREFIX + params[0];
                    d_manualIPAddress = params[1];
                    d_manualUserName = params[2];

                } else {
                    printUsage();
                    return false;
                }
            } else if (args[i].equalsIgnoreCase("-tls-client-credentials") && i + 1 < args.length) {
                d_clientCredentials = args[++i];
            } else if (args[i].equalsIgnoreCase("-tls-client-credentials-password") && i + 1 < args.length) {
                d_clientCredentialsPassword = args[++i];
            } else if (args[i].equalsIgnoreCase("-tls-trust-material") && i + 1 < args.length) {
                d_trustMaterial = args[++i];
            } else {
                printUsage();
                return false;
            }
        }

        if (d_hosts.isEmpty()) {
            d_hosts.add(new HostAndPort(d_defaultHost, d_defaultPort));
        }

        return true;
    }

    void printMessage(Event event) {
        for (Message msg: event) {
            System.out.println(msg);
        }
    }

    private void run(String[] args) throws IOException, InterruptedException {
        if (!parseCommandLine(args))
            return;

        ServerAddress[] servers = new ServerAddress[d_hosts.size()];
        for (int i = 0; i < d_hosts.size(); ++i) {
            servers[i] = new ServerAddress(d_hosts.get(i).host(),
                                           d_hosts.get(i).port());
        }

        SessionOptions sessionOptions = new SessionOptions();
        sessionOptions.setServerAddresses(servers);
        sessionOptions.setAuthenticationOptions(d_authOptions);
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(servers.length);

        if (d_clientCredentials != null && d_trustMaterial != null) {
            try {
                TlsOptions tlsOptions
                    = TlsOptions.createFromFiles(d_clientCredentials,
                                                 d_clientCredentialsPassword
                                                     .toCharArray(),
                                                 d_trustMaterial);
                sessionOptions.setTlsOptions(tlsOptions);
            } catch (TlsOptions.TlsInitializationException ex) {
                System.err.println(
                    "Failed to create TlsOptions object: " + ex.getMessage());
                return;
            }
        }

        System.out.print("Connecting to");
        for (ServerAddress server: sessionOptions.getServerAddresses()) {
            System.out.print(" " + server);
        }
        System.out.println();

        Session session = new Session(sessionOptions);
        if (!session.start()) {
            for (;;) {
                Event e = session.tryNextEvent();
                if (e == null)
                    break;
                printMessage(e);
            }
            System.err.println("Failed to start session.");
            return;
        }

        Identity identity = null;
        if (d_authOptions != null) {
            boolean isAuthorized = false;
            identity = session.createIdentity();
            if (session.openService("//blp/apiauth")) {
                Service authService = session.getService("//blp/apiauth");
                if (authorize(authService, identity, session, new CorrelationID())) {
                    isAuthorized = true;
                }
            }
            else {
                System.err.println("Failed to open //blp/apiauth.");
            }
            if (!isAuthorized) {
                System.err.println("No authorization");
                return;
            }
        }
    }

    private boolean authorize(
            Service authService,
            Identity identity,
            Session session,
            CorrelationID cid)
    throws IOException, InterruptedException {

        EventQueue tokenEventQueue = new EventQueue();
        try {
            if (d_manualIPAddress != null && d_manualUserName != null) {
                session.generateToken(d_manualUserName,
                                      d_manualIPAddress,
                                      new CorrelationID(),
                                      tokenEventQueue);
            } else {
                session.generateToken(new CorrelationID(), tokenEventQueue);
            }
        } catch (Exception e) {
            System.out.println("Generate token failed with exception: \n"  + e);
            return false;
        }
        String token = null;
        int timeoutMilliSeconds = 10000;
        Event event = tokenEventQueue.nextEvent(timeoutMilliSeconds);
        if (event.eventType() == EventType.TOKEN_STATUS ||
                event.eventType() == EventType.REQUEST_STATUS) {
            for (Message msg: event) {
                System.out.println(msg.toString());
                if (msg.messageType() == TOKEN_SUCCESS) {
                    token = msg.getElementAsString("token");
                }
            }
        }
        if (token == null){
            System.err.println("Failed to get token");
            return false;
        }

        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set("token", token);

        session.sendAuthorizationRequest(authRequest, identity, cid);

        long startTime = System.currentTimeMillis();
        final int WAIT_TIME = 10000; // 10 seconds
        while (true) {
            event = session.nextEvent(WAIT_TIME);
            if (event.eventType() == EventType.RESPONSE
                    || event.eventType() == EventType.PARTIAL_RESPONSE
                    || event.eventType() == EventType.REQUEST_STATUS) {
                for (Message msg: event) {
                    System.out.println(msg.toString());
                    if (msg.messageType() == AUTHORIZATION_SUCCESS) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            if (System.currentTimeMillis() - startTime > WAIT_TIME) {
                return false;
            }
        }
    }

    public static void main(String[] args) {
        System.out.println("ConnectionAndAuthExample");
        ConnectionAndAuthExample example = new ConnectionAndAuthExample();
        try {
            example.run(args);
        } catch (IOException e) {
            e.printStackTrace();
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }
}
