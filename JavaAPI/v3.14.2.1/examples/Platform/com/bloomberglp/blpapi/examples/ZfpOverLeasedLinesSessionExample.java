/*
 * Copyright (C) Bloomberg L.P., 2019
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

import java.io.IOException;

import com.bloomberglp.blpapi.Event;
import com.bloomberglp.blpapi.Message;
import com.bloomberglp.blpapi.Session;
import com.bloomberglp.blpapi.SessionOptions;
import com.bloomberglp.blpapi.TlsOptions;
import com.bloomberglp.blpapi.TlsOptions.TlsInitializationException;
import com.bloomberglp.blpapi.ZfpUtil;
import com.bloomberglp.blpapi.ZfpUtil.Remote;

/**
 * The example demonstrates how to establish a ZFP session that leverages
 * private leased line connectivity. To see how to use the resulting session
 * (authorizing a session, establishing subscriptions or making requests etc.),
 * please refer to the other examples.
 */
public class ZfpOverLeasedLinesSessionExample {
    private static final String AUTH_USER = "AuthenticationType=OS_LOGON";
    private static final String AUTH_APP_PREFIX = "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private static final String AUTH_USER_APP_PREFIX = "AuthenticationMode=USER_AND_APPLICATION;AuthenticationType=OS_LOGON;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private static final String AUTH_USER_APP_MANUAL_PREFIX = "AuthenticationMode=USER_AND_APPLICATION;AuthenticationType=MANUAL;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=";
    private static final String AUTH_DIR_PREFIX = "AuthenticationType=DIRECTORY_SERVICE;DirSvcPropertyName=";

    private static final String AUTH_OPTION_NONE = "none";
    private static final String AUTH_OPTION_USER = "user";
    private static final String AUTH_OPTION_APP = "app=";
    private static final String AUTH_OPTION_USER_APP = "userapp=";
    private static final String AUTH_OPTION_DIR = "dir=";
    private static final String AUTH_OPTION_MANUAL = "manual=";

    private Remote remote = Remote.REMOTE_8194;

    private String authOptions = AUTH_USER;
    private String manualUserName = null;
    private String manualIPAddress = null;

    private TlsOptions tlsOptions = null;

    public ZfpOverLeasedLinesSessionExample() {
    }

    private void run(String[] args)
            throws TlsInitializationException, InterruptedException, IOException {
        if (!parseCommandLine(args)) {
            return;
        }
        SessionOptions sessionOptions = ZfpUtil.getZfpOptionsForLeasedLines(remote, tlsOptions);

        // Note: ZFP solution requires authorization. The appropriate
        // authentication option must be set here on the 'SessionOptions'
        // before the session is created.
        sessionOptions.setAuthenticationOptions(authOptions);

        Session session = new Session(sessionOptions);

        if (!session.start()) {
            System.err.println("Failed to start session.");

            while(true) {
                Event event = session.tryNextEvent();

                if (event == null) {
                    break;
                }

                for (Message msg : event) {
                    System.out.println(msg);
                }
            }

            return;
        }

        System.out.println("Session started successfully.");

        // Note: ZFP solution requires authorization, which should be done here
        // before any subscriptions or requests can be made. For examples of
        // how to authorize or get data, please refer to the specific examples.
    }

    private boolean parseAuthOptions(String value) {
        if (value.equalsIgnoreCase(AUTH_OPTION_NONE)) {
            authOptions = null;

        } else if (value.equalsIgnoreCase(AUTH_OPTION_USER)) {
            authOptions = AUTH_USER;

        } else if (value.startsWith(AUTH_OPTION_APP)) {
            authOptions = AUTH_APP_PREFIX + value.substring(AUTH_OPTION_APP.length());

        } else if (value.startsWith(AUTH_OPTION_DIR)) {
            authOptions = AUTH_DIR_PREFIX + value.substring(AUTH_OPTION_DIR.length());

        } else if (value.startsWith(AUTH_OPTION_USER_APP)) {
            authOptions = AUTH_USER_APP_PREFIX + value.substring(AUTH_OPTION_USER_APP.length());

        } else if (value.startsWith(AUTH_OPTION_MANUAL)) {
            String[] params = value.substring(AUTH_OPTION_MANUAL.length()).split(",");
            if (params == null || params.length != 3) {
                return false;
            }

            authOptions = AUTH_USER_APP_MANUAL_PREFIX + params[0];
            manualIPAddress = params[1];
            manualUserName = params[2];

        } else {
            return false;

        }

        return true;
    }

    private static Remote remoteOfPort(int port) {
        if (port == 8194) {
            return Remote.REMOTE_8194;
        }

        if (port == 8196) {
            return Remote.REMOTE_8196;
        }

        throw new IllegalArgumentException("Invalid port");
    }

    private boolean parseCommandLine(String[] args) {
        String clientCredentials = null;
        String clientCredentialsPassword = null;
        String trustMaterial = null;

        for (int i = 0; i < args.length; ++i) {
            String option = args[i];
            if (i + 1 < args.length) {
                ++i;
                String value = args[i];

                if (option.equalsIgnoreCase("-zfp-over-leased-line")) {
                    try {
                        remote = remoteOfPort(Integer.valueOf(value));
                    } catch (IllegalArgumentException ex) {
                        System.err.println("Invalid ZFP port: " + value);
                        printUsage();
                        return false;
                    }

                } else if (option.equalsIgnoreCase("-auth")) {
                    if (!parseAuthOptions(value)) {
                        printUsage();
                        return false;
                    }

                } else if (option.equalsIgnoreCase("-tls-client-credentials")) {
                    clientCredentials = value;

                } else if (option.equalsIgnoreCase("-tls-client-credentials-password")) {
                    clientCredentialsPassword = value;

                } else if (option.equalsIgnoreCase("-tls-trust-material")) {
                    trustMaterial = value;

                } else {
                    System.err.println("Invalid option: " + option);
                    System.err.println();
                    printUsage();
                    return false;
                }
            } else {
                printUsage();
                return false;
            }
        }

        if (clientCredentials == null
                || clientCredentialsPassword == null
                || trustMaterial == null) {
            System.err.println("Tls options parameters are required.");
            System.err.println();
            printUsage();
            return false;
        }

        try {
            tlsOptions = TlsOptions.createFromFiles(
                                    clientCredentials,
                                    clientCredentialsPassword.toCharArray(),
                                    trustMaterial);
        } catch (TlsInitializationException ex) {
            System.err.println("Failed to create TlsOptions");
            ex.printStackTrace();
            return false;
        }

        return true;
    }

    private static void printUsage() {
        System.out.println("ZFP over leased lines session startup");
        System.out.println("Usage:");
        System.out.println("\t[-zfp-over-leased-line <port>] enable ZFP connections over leased lines on the specified port (8194 or 8196) (default: 8194)");
        System.out.println("\t[-auth <option>]               authentication option (default: user):");
        System.out.println("\t\tnone");
        System.out.println("\t\tuser                     as a user using OS logon information");
        System.out.println("\t\tdir=<property>           as a user using directory services");
        System.out.println("\t\tapp=<app>                as the specified application");
        System.out.println("\t\tuserapp=<app>            as user and application using logon information for the user");
        System.out.println("\t\tmanual=<app>,<ip>,<user> as user and application, with manually provided IP address and EMRS user");
        System.out.println();
        System.out.println("TLS OPTIONS (specify all):");
        System.out.println("\t[-tls-client-credentials <file>]         name a PKCS#12 file to use as a source of client credentials");
        System.out.println("\t[-tls-client-credentials-password <pwd>] specify password for accessing client credentials");
        System.out.println("\t[-tls-trust-material <file>]             name a PKCS#7 file to use as a source of trusted certificates");
    }


    public static void main(String[] args) {
        System.out.println(ZfpOverLeasedLinesSessionExample.class.getSimpleName());

        ZfpOverLeasedLinesSessionExample example = new ZfpOverLeasedLinesSessionExample();
        try {
            example.run(args);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
