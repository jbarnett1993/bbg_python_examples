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
#include <blpapi_session.h>
#include <blpapi_eventdispatcher.h>

#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_element.h>
#include <blpapi_name.h>
#include <blpapi_request.h>
#include <blpapi_subscriptionlist.h>
#include <blpapi_defs.h>
#include <blpapi_exception.h>

#include <iostream>
#include <vector>
#include <string>
#include <stdlib.h>
#include <string.h>

using namespace BloombergLP;
using namespace blpapi;

namespace {
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");

	const Name FIELD_ID("id");
    const Name FIELD_MNEMONIC("mnemonic");
    const Name FIELD_DATA("fieldData");
    const Name FIELD_DESC("description");
    const Name FIELD_INFO("fieldInfo");
    const Name FIELD_ERROR("fieldError");
    const Name FIELD_MSG("message");
    const Name CATEGORY("category");
    const Name CATEGORY_NAME("categoryName");
    const Name CATEGORY_ID("categoryId");
    const Name FIELD_SEARCH_ERROR("fieldSearchError");

	const char *AUTH_SVC = "//blp/apiauth";
};

class SimpleCategorizedFieldSearchExample
{
    int ID_LEN;
    int MNEMONIC_LEN;
    int DESC_LEN;
    int CAT_NAME_LEN;
    std::string PADDING;
    std::string APIFLDS_SVC;
	std::vector<std::string> d_hosts;		// IP Addresses of appliances
    int					d_port;
	std::string			d_authOption;	// authentication option user/application
	std::string			d_name;	        // DirectoryService/ApplicationName
    Identity			d_identity;
	Session				*d_session;

    void printUsage()
    {
        std::cout << "Usage:" << std::endl
            << "    Categorized Field Search Example " << std::endl
            << "        [-ip        <ipAddress  = localhost>" << std::endl
            << "        [-p         <tcpPort    = 8194>" << std::endl
			<< "        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]" << std::endl
            << "        [-n         <name = applicationName or directoryService>]" << std::endl
            << "Notes:" << std::endl
            << " -Specify only LOGON to authorize 'user' using Windows login name." << std::endl
            << " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
            << " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
    }

    bool parseCommandLine(int argc, char **argv)
    {
        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
            }else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
			} else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
				d_name = argv[++i];
			} else if (!std::strcmp(argv[i], "-h")) {
				printUsage();
				return false;
			}
        }
		// check for hosts
        if (d_hosts.size() == 0) {
			 std::cout << "Missing host ip." << std::endl;
			 printUsage();
             return false;
        }

		// check for appliation name
		if ((!std::strcmp(d_authOption.c_str(),"APPLICATION") || !std::strcmp(d_authOption.c_str(), "USER_APP")) && (!std::strcmp(d_name.c_str(), ""))){
			 std::cout << "Application name cannot be NULL for application authorization." << std::endl;
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

    std::string padString(std::string str, unsigned int width)
    {
        if (str.length() >= width || str.length() >= PADDING.length() ) return str;
        else return str + PADDING.substr(0, width-str.length());
    }

    void printField (const Element &field)
    {
        std::string  fldId       = field.getElementAsString(FIELD_ID);
        if (field.hasElement(FIELD_INFO)) {
            Element fldInfo          = field.getElement (FIELD_INFO) ;
            std::string  fldMnemonic = 
                fldInfo.getElementAsString(FIELD_MNEMONIC);
            std::string  fldDesc     =
                fldInfo.getElementAsString(FIELD_DESC);

            std::cout << padString(fldId, ID_LEN) 
                << padString (fldMnemonic, MNEMONIC_LEN)
                << padString (fldDesc, DESC_LEN) << std::endl;
        }
        else {
            Element fldError = field.getElement(FIELD_ERROR) ;
            std::string  errorMsg = fldError.getElementAsString(FIELD_MSG) ;

            std::cout << std::endl << " ERROR: " << fldId << " - "
                << errorMsg << std::endl ;
        }
    }

    void printHeader ()
    {
        std::cout << padString("FIELD ID", ID_LEN) +
            padString("MNEMONIC", MNEMONIC_LEN) +
            padString("DESCRIPTION", DESC_LEN)
            << std::endl;
        std::cout << padString("-----------", ID_LEN) +
            padString("-----------", MNEMONIC_LEN) +
            padString("-----------", DESC_LEN)
            << std::endl;
    }

	bool startSession(){

        SessionOptions sessionOptions;
		std::string authOptions;

		std::cout << "Connecting to port " << d_port << " on server: "; 
        for (size_t i = 0; i < d_hosts.size(); ++i) {
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
			std::cout << d_hosts[i].c_str();
        }
		std::cout << std::endl;

        sessionOptions.setServerPort(d_port);
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());
		authOptions = getAuthOptions();
		if (authOptions.size() > 0) {
			sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}
        d_session = new Session(sessionOptions);

        if (!d_session->start()) {
            std::cerr << "Failed to connect!" << std::endl;
            return false;
        }
        
        return true;
    }

	// construct authentication option string
	std::string getAuthOptions()
	{
		std::string authOptions;
		if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
		} else if (!strcmp(d_authOption.c_str(), "NONE")) {
			// do nothing
		} else if (!strcmp(d_authOption.c_str(), "USER_APP")) {
            // Set User and Application Authentication Option
            authOptions = "AuthenticationMode=USER_AND_APPLICATION;";
            authOptions += "AuthenticationType=OS_LOGON;";
            authOptions += "ApplicationAuthenticationType=APPNAME_AND_KEY;";
            // ApplicationName is the entry in EMRS.
            authOptions += "ApplicationName=" + d_name;
		} else if (!strcmp(d_authOption.c_str(), "DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
		} else {
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
		}

		std::cout << "Authentication Options = " << authOptions << std::endl;
		return authOptions;
	}

	bool authorize()
    {
        EventQueue tokenEventQueue;
        d_session->generateToken(CorrelationId(), &tokenEventQueue);
        std::string token;
        Event event = tokenEventQueue.nextEvent();
        if (event.eventType() == Event::TOKEN_STATUS) {
            MessageIterator iter(event);
            while (iter.next()) {
                Message msg = iter.message();
                msg.print(std::cout);
                if (msg.messageType() == TOKEN_SUCCESS) {
                    token = msg.getElementAsString(TOKEN);
                }
                else if (msg.messageType() == TOKEN_FAILURE) {
                    break;
                }
            }
        }
        if (token.length() == 0) {
            std::cout << "Failed to get token" << std::endl;
            return false;
        }

        d_session->openService(AUTH_SVC);
		Service authService = d_session->getService(AUTH_SVC);
        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

        EventQueue authQueue;
		d_identity = d_session->createIdentity();
        d_session->sendAuthorizationRequest(
            authRequest, &d_identity, CorrelationId(), &authQueue);

        while (true) {
            Event event = authQueue.nextEvent();
            if (event.eventType() == Event::RESPONSE ||
                event.eventType() == Event::REQUEST_STATUS ||
                event.eventType() == Event::PARTIAL_RESPONSE) {
                    MessageIterator msgIter(event);
                    while (msgIter.next()) {
                        Message msg = msgIter.message();
                        msg.print(std::cout);
                        if (msg.messageType() == AUTHORIZATION_SUCCESS) {
                            return true;
                        }
                        else {
                            std::cout << "Authorization failed" << std::endl;
                            return false;
                        }
                    }
            }
        }
    }

public:
      SimpleCategorizedFieldSearchExample(): d_session(0)
		  , PADDING("                                            ")
          , APIFLDS_SVC("//blp/apiflds")
		  , d_name("") 
	  {
              ID_LEN         = 13;
              MNEMONIC_LEN   = 36;
              DESC_LEN       = 40;
              CAT_NAME_LEN   = 40;
      }
	  
	  ~SimpleCategorizedFieldSearchExample()
	  {
		  if (d_session) delete d_session;
	  }

      void run(int argc, char **argv)
      {
          d_port = 8194;
          if (!parseCommandLine(argc, argv)) return;

		  if (!startSession()) return;

		  if (strcmp(d_authOption.c_str(), "NONE")) {
			  if (!authorize()) {
				return;
			  }
		  }
          if (!d_session->openService(APIFLDS_SVC.c_str())) {
              std::cerr <<"Failed to open " << APIFLDS_SVC << std::endl;
              return;
          }

          Service fieldInfoService = d_session->getService(APIFLDS_SVC.c_str());
          Request request = fieldInfoService.createRequest(
              "CategorizedFieldSearchRequest");
          request.set ("searchSpec", "last price");
          Element exclude = request.getElement("exclude");
          exclude.setElement("fieldType", "Static");
          request.set ("returnFieldDocumentation", false);

		  if (!strcmp(d_authOption.c_str(), "NONE")) {
			  std::cout << "Sending Request: "  << request << std::endl;
			  d_session->sendRequest(request, CorrelationId(this));
		} else {
			std::cout << "Sending request with user's Identity: " << request << std::endl;
			if (d_identity.getSeatType() == Identity::BPS) {
				std::cout << "BPS User" << std::endl;
			} else if (d_identity.getSeatType() == Identity::NONBPS) {
				std::cout << "NON-BPS User" << std::endl;
			} else if (d_identity.getSeatType() == Identity::INVALID_SEAT) {
				std::cout << "Invalid User" << std::endl;
			}
			d_session->sendRequest(request, d_identity, CorrelationId(this));
		}

          while (true) {
              Event event = d_session->nextEvent();
              if (event.eventType() != Event::RESPONSE &&
                  event.eventType() != Event::PARTIAL_RESPONSE) {
                      continue;
              }

              MessageIterator msgIter(event);
              while (msgIter.next()) {
                  Message msg = msgIter.message();
                  if (msg.hasElement(FIELD_SEARCH_ERROR)) {
                      msg.print(std::cout);
                      continue;
                  }

                  Element categories = msg.getElement("category");
                  int numCategories = categories.numValues();

                  for (int catIdx=0; catIdx < numCategories; ++catIdx) {

                      Element category = categories.getValueAsElement(catIdx);
                      std::string Name = 
                          category.getElementAsString("categoryName");
                      std::string Id = 
                          category.getElementAsString("categoryId");

                      std::cout << "\n  Category Name:" << 
                          padString (Name, CAT_NAME_LEN) <<
                          "\tId:" << Id << std::endl;

                      Element fields = category.getElement("fieldData");
                      int numElements = fields.numValues();

                      printHeader();
                      for (int i=0; i < numElements; i++) {
                          printField (fields.getValueAsElement(i));
                      }
                  }
                  std::cout << std::endl;
              }
              if (event.eventType() == Event::RESPONSE) {
                  break;
              }
          }
      }
};

int main(int argc, char **argv){
    SimpleCategorizedFieldSearchExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
        std::cerr << "Library Exception!!! " << e.description() << std::endl;
    }
    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
