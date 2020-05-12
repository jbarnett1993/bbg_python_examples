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

#include <blpapi_session.h>
#include <blpapi_eventdispatcher.h>

#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_element.h>
#include <blpapi_name.h>
#include <blpapi_request.h>
#include <blpapi_subscriptionlist.h>
#include <blpapi_exception.h>
#include <blpapi_defs.h>

#include <ctime>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>

using namespace BloombergLP;
using namespace blpapi;

namespace {
    const std::string dateTimeFormat("%d-%d-%dT%d:%d:%d");
    const char *MSG1_SERVICE = "//blp/msgscrape";
    const char *AUTH_SVC = "//blp/apiauth";
    const char *REPLAY = "replay";
    const char *STATUS_INFO = "statusInfo";

    const Name ERROR_RESPONSE("errorResponse");
    const Name REPLAY_RESPONSE("replayResponse");
    const Name ERROR_MESSAGE("errorMsg");
    const Name MARKET_DATAS("marketDatas");
    const Name START("start");
    const Name END("end");
    const Name FILTER("filter");
    const Name SERIAL("serial");
    const Name TIME("time");
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");

    enum FilterChoiceType
    {
        ALL,
        LAST_UPDATE_ONLY,
    };
};

class Msg1RecoveryExample 
{
    std::vector<std::string> d_hosts;        // IP Addresses of appliances
    int d_port;
    std::string d_authOption;
    std::string d_dsName;
    std::string d_name;
    std::string d_token;
    Identity d_identity;
    std::string d_requestType;

    Name d_startType;
    Name d_endType;
    int d_startSerial;
    int d_endSerial;
    Datetime d_startTime;
    Datetime d_endTime;
    FilterChoiceType d_filter;

    SessionOptions d_sessionOptions;
    Session *d_session;

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_requestType = REPLAY;
                std::string startArg(argv[++i]);
                int startSerial = atoi(startArg.c_str());
				std::stringstream buffer;
				buffer << startSerial;
                if (startArg == buffer.str())
                {
                    d_startType = SERIAL;
                    d_startSerial = startSerial;
                }
                else
                {
                    try
                    {
                        int Y = 0, M = 0, D = 0, h = 0, m = 0, s = 0;
                        sscanf(startArg.c_str(), dateTimeFormat.c_str(), &Y, &M, &D, &h, &m, &s);
                        d_startTime = Datetime(Y, M, D, h, m, s);
                        d_startType = TIME;
                    }
                    catch (...)
                    {
                        std::cout << "Error: '" << startArg << "' is not in the proper Datetime format: " << dateTimeFormat << std::endl;
                        printUsage();
                        return false;
                    }
                }
            } else if (!std::strcmp(argv[i],"-e") && i + 1 < argc) {
                d_requestType = REPLAY;
                std::string endArg(argv[++i]);
                int endSerial = atoi(endArg.c_str());
				std::stringstream buffer;
				buffer << endSerial;
                if (endArg == buffer.str())
                {
                    d_endType = SERIAL;
                    d_endSerial = endSerial;
                }
                else
                {
                    try
                    {
                        int Y = 0, M = 0, D = 0, h = 0, m = 0, s = 0;
                        sscanf(endArg.c_str(), dateTimeFormat.c_str(), &Y, &M, &D, &h, &m, &s);
                        d_endTime = Datetime(Y, M, D, h, m, s);
                        d_endType = TIME;
                    }
                    catch (...)
                    {
                        std::cout << "Error: '" << endArg << "' is not in the proper Datetime format: " << dateTimeFormat << std::endl;
                        printUsage();
                        return false;
                    }
                }
            } else if (!std::strcmp(argv[i],"-f") && i + 1 < argc) {
                d_requestType = REPLAY;
                std::string filter(argv[++i]);
                if (filter == "ALL")
                {
                    d_filter = ALL;
                }
                else if (filter == "LAST_UPDATE_ONLY")
                {
                    d_filter = LAST_UPDATE_ONLY;
                }
                else
                {
                    std::cout << "Error: '{0}' is not a supported filter type." << filter << std::endl;
                    printUsage();
                    return false;
                }
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
            } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
            } else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
                d_authOption = argv[++i];
            } else if (!std::strcmp(argv[i],"-ds") &&  i + 1 < argc) {
                d_dsName = argv[++i];
            } else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
                d_name = argv[++i];
            } else if (!std::strcmp(argv[i], "-h")) {
                printUsage();
                return false;
            }
        }

        // check for hosts
        if (d_hosts.size() == 0) {
            d_hosts.push_back("localhost");
        }

        // check for appliation name
        if ((!std::strcmp(d_authOption.c_str(),"APPLICATION") || !std::strcmp(d_authOption.c_str(), "USER_APP")) && (!std::strcmp(d_name.c_str(), ""))){
             std::cout << "Application name cannot be NULL for application authorization." << std::endl;
             printUsage();
             return false;
        }
        if (!std::strcmp(d_authOption.c_str(),"USER_DS_APP") && (!std::strcmp(d_name.c_str(), "") || !std::strcmp(d_dsName.c_str(), ""))){
             std::cout << "Application or DS name cannot be NULL for application authorization." << std::endl;
             printUsage();
             return false;
        }
        // check for Directory Service name
        if ((!std::strcmp(d_authOption.c_str(),"DIRSVC")) && (!std::strcmp(d_name.c_str(), ""))){
             std::cout << "Directory Service property name cannot be NULL for DIRSVC authorization." << std::endl;
             printUsage();
             return false;
        }

        return true;
    }

    void printErrorInfo(const char *leadingStr, const Element &errorInfo)
    {
        const Name CATEGORY("category");
        const Name MESSAGE("message");

        std::cout << leadingStr
            << errorInfo.getElementAsString(CATEGORY)
            << " ("
            << errorInfo.getElementAsString(MESSAGE)
            << ")" << std::endl;
    }

    void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "    Retrieve MSG1 data " << std::endl
            << "      [-s     <start        = 0>" << std::endl
            << "      [-e     <end          = " << d_endTime << ">(in " << dateTimeFormat << " format)" << std::endl
            << "      [-f     <filter       = LAST_UPDATE_ONLY>" << std::endl
            << "      [-ip    <ipAddress    = localhost>" << std::endl
            << "      [-p     <tcpPort      = 8194>" << std::endl
            << "      [-auth  <authenticationOption = LOGON(default) or APPLICATION or DIRSVC or USER_APP or USER_DS_APP>]" << std::endl
            << "      [-n     <name         = applicationName>]" << std::endl
            << "      [-ds    <dsName       = directoryService>]" << std::endl
            << "Notes:" << std::endl
            << "1) This example client make a status infomation query by default." << std::endl
            << "2) Specify start and end to request MSG1 recovory request." << std::endl
            << "Notes on MSG1 recovery:" << std::endl
            << "1) Specify start as either a number (as serial id) or time (as timestamp)." << std::endl
            << "2) Specify end as either a number (as serial id) or time (as timestamp)." << std::endl
            << "3) Specify filter as 'ALL' or 'LAST_UPDATE_ONLY'.\n" << std::endl
            << "Notes on authorization:" << std::endl
            << "1) Specify only LOGON to authorize 'user' using Windows login name." << std::endl
            << "2) Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
            << "3) Specify APPLICATION and name(Application Name) to authorize application.\n" << std::endl;
    }

    bool createSession(){
        std::string authOptions;
        if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) 
        { 
            //  Authenticate application
            // Set Application Authentication Option
            authOptions = "AuthenticationMode=APPLICATION_ONLY;";
            authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions+= "ApplicationName=" + d_name;
        } 
        else if (!strcmp(d_authOption.c_str(), "USER_APP")) 
        {
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=OS_LOGON;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
        } 
        else if (!strcmp(d_authOption.c_str(), "USER_DS_APP")) 
        {
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=DIRECTORY_SERVICE;";
            authOptions += "DirSvcPropertyName=" + d_dsName + ";";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
        } 
        else if (!strcmp(d_authOption.c_str(), "DIRSVC")) 
        {        
			// Authenticate user using active directory service property
            authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
            authOptions += "DirSvcPropertyName=" + d_name;
        } 
        else 
        {
            // Authenticate user using windows/unix login name
            authOptions = "AuthenticationType=OS_LOGON";
        }

        std::cout << "Authentication Options = " << authOptions << std::endl;

        for (size_t i = 0; i < d_hosts.size(); ++i) {
            d_sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
            std::cout << d_hosts[i].c_str();
        }
        std::cout << std::endl;

        d_sessionOptions.setServerPort(d_port);
        std::cout << "Connecting to port " << d_port << " on server: "; 
        d_sessionOptions.setAutoRestartOnDisconnection(true);
        d_sessionOptions.setNumStartAttempts(d_hosts.size());
        if (authOptions.size() > 0) {
            d_sessionOptions.setAuthenticationOptions(authOptions.c_str());
        }

        if (d_session) delete d_session;
        d_session = new Session(d_sessionOptions);

        if (!d_session->start()) {
            std::cerr << "Failed to connect!" << std::endl;
            return false;
        }
        if (!d_session->openService(MSG1_SERVICE)) {
            std::cerr << "Failed to open " << MSG1_SERVICE << std::endl;
            d_session->stop();
            return false;
        }
        
        return true;
    }
    
    bool generateToken(std::string &token)
    {
        bool isTokenSuccess = false;
        bool isRunning = false;

        token.clear();
        EventQueue tokenEventQueue;
        d_session->generateToken(CorrelationId(), &tokenEventQueue);

        while (!isRunning)
        {
            Event eventObj = tokenEventQueue.nextEvent();
            if (eventObj.eventType() == Event::TOKEN_STATUS)
            {
                std::cout << "processTokenEvents" << std::endl;
                MessageIterator iter(eventObj);
                while (iter.next()) 
                {
                    Message msg = iter.message();
                    msg.print(std::cout);
                    if (msg.messageType() == TOKEN_SUCCESS)
                    {
                        token = msg.getElementAsString("token");
                        isTokenSuccess = true;
                        isRunning = true;
                        break;
                    }
                    else if (msg.messageType() == TOKEN_FAILURE)
                    {
                        std::cout << "Received : " << TOKEN_FAILURE << std::endl;
                        isRunning = true;
                        break;
                    }
                    else
                    {
                        std::cout << "Error while Token Generation" << std::endl;
                        isRunning = true;
                        break;
                    }
                }
            }
        }

        return isTokenSuccess;
    }
    
    bool isBPipeAuthorized(std::string token, Identity &identity)
    {
        bool isAuthorized = false;
        bool isRunning = true;

        if (!d_session->openService(AUTH_SVC))
        {
            std::cout << "Failed to open " << AUTH_SVC << std::endl;
            return (isAuthorized = false);

        }
        Service authService = d_session->getService(AUTH_SVC);


        Request authRequest = authService.createAuthorizationRequest();

        authRequest.set("token", token.c_str());
        identity = d_session->createIdentity();
        EventQueue authEventQueue;

        d_session->sendAuthorizationRequest(authRequest, &identity, CorrelationId(1), &authEventQueue);

        while (isRunning)
        {
            Event eventObj = authEventQueue.nextEvent();
            std::cout << "processEvent" << std::endl;
            if (eventObj.eventType() == Event::RESPONSE || eventObj.eventType() == Event::REQUEST_STATUS)
            {
                MessageIterator iter(eventObj);
                while (iter.next()) 
                {
                    Message msg = iter.message();
                    msg.print(std::cout);
                    if (msg.messageType() == AUTHORIZATION_SUCCESS)
                    {
                        std::cout << "Authorization SUCCESS" << std::endl;
                        isAuthorized = true;
                        isRunning = false;
                        break;
                    }
                    else if (msg.messageType() == AUTHORIZATION_FAILURE)
                    {
                        std::cout << "Authorization FAILED" << std::endl;
                        isRunning = false;
                        break;
                    }
                    else
                    {
                        std::cout << msg << std::endl;
                    }        
                }
            }
        }
        return isAuthorized;
    }


    void sendMSG1StatusRequest(Session &session)
    {
        Service service = session.getService(MSG1_SERVICE);
        Identity id = session.createIdentity();

        Request request = service.createRequest(STATUS_INFO);
        std::cout << "Sending request: " << request << std::endl;
        d_session->sendRequest(request, d_identity);
    }

    void sendMSG1RecoverRequest(Session &session)
    {
        Service service = session.getService(MSG1_SERVICE);
        Identity id = session.createIdentity();

        Request request = service.createRequest(REPLAY);            
        if (d_startType == TIME)
        {
            request.getElement(START).setChoice(TIME).setValue(d_startTime);
        }
        else if (d_startType == SERIAL)
        {
            request.getElement(START).setChoice(SERIAL).setValue(d_startSerial);
        }
        if (d_endType == TIME)
        {
            request.getElement(END).setChoice(TIME).setValue(d_endTime);
        }
        else if (d_endType == SERIAL)
        {
            request.getElement(END).setChoice(SERIAL).setValue(d_endSerial);
        }
        request.getElement(FILTER).setValue(d_filter);

        std::cout << "Sending request: " << request << std::endl;
        // request data with identity object
        d_session->sendRequest(request, d_identity);
    }

    void processResponseEvent(Event event)
    {
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
            if (msg.asElement().hasElement(ERROR_RESPONSE)) {
                printErrorInfo("REQUEST FAILED: ", 
                    msg.getElement(ERROR_RESPONSE));
                continue;
            }
            else if (msg.asElement().hasElement(REPLAY_RESPONSE))
            {
                std::cout << "# of Recovered data: " << msg.getElement(MARKET_DATAS).numValues() << std::endl;
                continue;
            }
            else
            {
                std::cout << "Received Response: " << msg << std::endl;
                continue;
            }
        }
    }

    void eventLoop(Session &session)
    {
        bool done = false;
        while (!done) {
            Event event = session.nextEvent();
            if (event.eventType() == Event::PARTIAL_RESPONSE) {
                std::cout << "Processing Partial Response" << std::endl;
                processResponseEvent(event);
            }
            else if (event.eventType() == Event::RESPONSE) {
                std::cout << "Processing Response" << std::endl;
                processResponseEvent(event);
                done = true;
            } else {
                MessageIterator msgIter(event);
                while (msgIter.next()) {
                    Message msg = msgIter.message();
                    if (event.eventType() == Event::SESSION_STATUS) {
                        if (msg.messageType() == "SessionTerminated") {
                            done = true;
                        }
                    }
                }
            }
        }
    }

public:
    Msg1RecoveryExample() : d_session(0)
    {
        d_port = 8194;
        d_startType = SERIAL;
        d_startSerial = 0;
        d_endType = TIME;
        time_t t = time(0);   // get time now
        struct tm * now = localtime(&t);
        if(now)
        {
            d_endTime = Datetime(now->tm_year + 1900, now->tm_mon + 1, now->tm_mday, now->tm_hour, now->tm_min, now->tm_sec);
        }
        d_filter = LAST_UPDATE_ONLY;
        d_requestType = STATUS_INFO;
        d_authOption = "LOGON";
        d_dsName = "";
        d_name = "";
    }

    ~Msg1RecoveryExample()
    {
        if (d_session) delete d_session;
    }

    void run(int argc, char **argv)
    {
        if (!parseCommandLine(argc, argv)) return;

        if (!createSession()){
            std::cerr << "Failed to open session" << std::endl;
            return;
        }
        
        //// Authenticate user using Generate Token Request 
        //if (!generateToken(d_token)) return;

        ////Authorization : pass Token into authorization request. Returns User handle with user's entitlements info set by server.
        //if (!isBPipeAuthorized(d_token, d_identity)) return;

        if (!d_session->openService(MSG1_SERVICE))
        {
            std::cerr << "Failed to open " << MSG1_SERVICE << std::endl;
            return;
        }

        try {
            if (d_requestType == STATUS_INFO)
            {
                sendMSG1StatusRequest(*d_session);
            }
            else if (d_requestType == REPLAY)
            {
                sendMSG1RecoverRequest(*d_session);
            }
        } catch (Exception &e) {
            std::cerr << "Library Exception !!!" << e.description() << std::endl;
        } catch (...) {
            std::cerr << "Unknown Exception !!!" << std::endl;
        }

        // wait for events from session.
        try {
            eventLoop(*d_session);
        } catch (Exception &e) {
            std::cerr << "Library Exception !!!" << e.description() << std::endl;
        } catch (...) {
            std::cerr << "Unknown Exception !!!" << std::endl;
        }

        d_session->stop();
    }
};

int main(int argc, char **argv)
{
    std::cout << "Msg1RecoveryExample" << std::endl;
    Msg1RecoveryExample example;
    example.run(argc, argv);

    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
