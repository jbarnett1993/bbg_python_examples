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
#include <blpapi_event.h>
#include <blpapi_message.h>
#include <blpapi_element.h>
#include <blpapi_name.h>
#include <blpapi_request.h>
#include <blpapi_exception.h>

#include <iostream>
#include <vector>
#include <string>
#include <algorithm>
#include <stdlib.h>
#include <string.h>

using namespace BloombergLP;
using namespace blpapi;

namespace
{
    const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
    const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
    const Name TOKEN_SUCCESS("TokenGenerationSuccess");
    const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name TOKEN("token");

	const Name SECURITY_DATA("securityData");
	const Name SECURITY_NAME("security");
	const Name DATE("date");

	const Name FIELD_ID("fieldId");
	const Name FIELD_DATA("fieldData");
	const Name FIELD_DESC("description");
	const Name FIELD_INFO("fieldInfo");
	const Name FIELD_ERROR("fieldError");
	const Name FIELD_MSG("message");
	const Name SECURITY_ERROR("securityError");
	const Name ERROR_MESSAGE("message");
	const Name FIELD_EXCEPTIONS("fieldExceptions");
	const Name ERROR_INFO("errorInfo");
	const Name RESPONSE_ERROR("responseError");

	const char *REFDATA_SVC = "//blp/refdata";
	const char *AUTH_SVC = "//blp/apiauth";
}

using BloombergLP::blpapi::Event;
using BloombergLP::blpapi::Element;
using BloombergLP::blpapi::Message;
using BloombergLP::blpapi::Name;
using BloombergLP::blpapi::Request;
using BloombergLP::blpapi::Service;
using BloombergLP::blpapi::Session;
using BloombergLP::blpapi::SessionOptions;

class HistoryExample
{
	std::vector<std::string>		d_hosts;		// IP Addresses of appliances
    int					d_port;
	std::string			d_authOption;	// authentication option user/application
	std::string			d_name;	        // DirectoryService/ApplicationName
    Identity			d_identity;
	Session				*d_session;
	std::vector<std::string> d_securities;
	std::vector<std::string> d_fields;
	std::string        		 d_startDate;
	std::string          	 d_endDate;

	void printUsage()
	{
	  std::cout << "Usage:" << std::endl
                << "   Retrieve historical data " << std::endl
                << "        [-s         <security = IBM US Equity>" << std::endl
                << "        [-f         <field = PX_LAST>" << std::endl
                << "        [-sd        <startDateTime = 20091026" << std::endl
                << "        [-ed        <endDateTime = 20091030" << std::endl
                << "        [-ip        <ipAddress = localhost>" << std::endl
                << "        [-p         <tcpPort = 8194>" << std::endl
				<< "        [-auth      <authenticationOption = LOGON (default) or NONE or APPLICATION or DIRSVC or USER_APP>]" << std::endl
                << "        [-n         <name = applicationName or directoryService>]" << std::endl
                << "Notes:" << std::endl
                << " -Specify only LOGON to authorize 'user' using Windows login name." << std::endl
                << " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
                << " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
	}

	bool parseCommandLine(int argc, char **argv)
	{
		std::string date;
		for (int i = 1; i < argc; ++i)
		{
		  if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
			d_securities.push_back(argv[++i]);
		  } else if (!std::strcmp(argv[i],"-f") && i + 1 < argc){
			d_fields.push_back(argv[++i]);
		  } else if (!std::strcmp(argv[i],"-sd") && i + 1 < argc){
			d_startDate = argv[++i];
		  } else if (!std::strcmp(argv[i],"-ed") && i + 1 < argc){
			d_endDate = argv[++i];
		  } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc){
				d_hosts.push_back(argv[++i]);
		  } else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc){
				d_port = std::atoi(argv[++i]);
		  } else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
		  } else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
                d_name = argv[++i];
		  } else if (!std::strcmp(argv[i], "-h") && i < argc) {
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

		//default arguments
		if (d_securities.size() == 0)
		{
		  d_securities.push_back("IBM US Equity");
		}
		if (d_fields.size() == 0)
		{
		  d_fields.push_back("PX_LAST");
		}
		if (d_startDate.empty())
		{
			d_startDate = "20100101";
		}
		if (d_endDate.empty())
		{
			d_endDate = "20101231";
		}

		return true;
	}//end parseCommandLine

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
        if (!d_session->openService(REFDATA_SVC)) {
			std::cerr << "Failed to open " << REFDATA_SVC << std::endl;
            d_session->stop();
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
	HistoryExample() : d_session(0) {}

	~HistoryExample()
	{
		if (d_session) delete d_session;
	}

    void run(int argc, char **argv)
    {
        d_port = 8194;
		d_name = "";
        if (!parseCommandLine(argc, argv)) return;

        if (!startSession()) return;

		if (strcmp(d_authOption.c_str(), "NONE")) {
			if (!authorize()) {
				return;
			}
		}

		Service refDataService = d_session->getService(REFDATA_SVC);
        Request request = refDataService.createRequest("HistoricalDataRequest");

		for(int i = 0; i < (int)d_securities.size(); i++)
		{
			request.getElement("securities").appendValue(d_securities[i].c_str());			
		}
		for(int k = 0; k < (int)d_fields.size(); k++)
		{
			request.getElement("fields").appendValue(d_fields[k].c_str());
		}

        request.set("periodicitySelection", "DAILY");
        request.set("startDate", d_startDate.c_str());
        request.set("endDate", d_endDate.c_str());

		if (!strcmp(d_authOption.c_str(), "NONE")) {
			std::cout << "Sending request: " << request << std::endl;
			d_session->sendRequest(request);
		} else {
			std::cout << "Sending request with user's Identity: " << request << std::endl;
			if (d_identity.getSeatType() == Identity::BPS) {
				std::cout << "BPS User" << std::endl;
			} else if (d_identity.getSeatType() == Identity::NONBPS) {
				std::cout << "NON-BPS User" << std::endl;
			} else if (d_identity.getSeatType() == Identity::INVALID_SEAT) {
				std::cout << "Invalid User" << std::endl;
			}
			d_session->sendRequest(request, d_identity);
		}

        while (true)
		{
            Event event = d_session->nextEvent();
            MessageIterator msgIter(event);
            while (msgIter.next())
			{
                Message msg = msgIter.message();

				if ((event.eventType() != Event::PARTIAL_RESPONSE) && (event.eventType() != Event::RESPONSE))
				{
					msg.print(std::cout);
					continue;
				}
				if (msg.hasElement(RESPONSE_ERROR, true))
				{
					// response error
					msg.print(std::cout);
				} else {
					Element securityData = msg.getElement(SECURITY_DATA);
					Element securityName = securityData.getElement(SECURITY_NAME);
					std::cout << securityName << "\n\n";

					//only process field data if no errors have occurred
					if(!ProcessErrors(msg))
					{
						ProcessExceptions(msg);
						ProcessFields(msg);
					}
				}

				std::cout << "\n\n";
            }
            if (event.eventType() == Event::RESPONSE) {
                break;
            }
        }
    }

	bool ProcessExceptions(Message msg)
	{
		Element securityData = msg.getElement(SECURITY_DATA);
        Element field_exceptions = securityData.getElement(FIELD_EXCEPTIONS);

		if (field_exceptions.numValues() > 0)
        {
            Element element = field_exceptions.getValueAsElement(0);
            Element field_id = element.getElement(FIELD_ID);
            Element error_info = element.getElement(ERROR_INFO);
            Element error_message = error_info.getElement(ERROR_MESSAGE);
            std::cout <<  field_id << "\n";
            std::cout << error_message << "\n";
			return true;
        }
		return false;
	}

	bool ProcessErrors(Message msg)
	{
		Element securityData = msg.getElement(SECURITY_DATA);

		if (securityData.hasElement(SECURITY_ERROR))
        {
            Element security_error = securityData.getElement(SECURITY_ERROR);
            Element error_message = security_error.getElement(ERROR_MESSAGE);
            std::cout << error_message << "\n";
			return true;
        }
		return false;
	}

	void ProcessFields(Message msg)
	{
		const char *delimiter = "\t\t";

		// print out the date column header
        std::cout << "DATE" << *delimiter << *delimiter;

		// print out the field column headers
	    for(int k = 0; k < (int)d_fields.size(); k++)
	    {
		  std::cout << d_fields[k].c_str() << *delimiter;
	    }
	    std::cout << "\n\n";
		Element securityData = msg.getElement(SECURITY_DATA);
        Element fieldData = securityData.getElement(FIELD_DATA);

		// retrieve each field dependant on it's datatype
		if(fieldData.numValues() > 0)
		{
			for(int j = 0; j < (int)fieldData.numValues(); j++)
			{
				int datatype;

				Element element = fieldData.getValueAsElement(j);
				Datetime date =  element.getElementAsDatetime(DATE);
				std::cout << date.day() << '/' << date.month() << '/' << date.year() << *delimiter;

				for(int k = 0; k < (int)d_fields.size(); k++)
				{
					const char *temp_field_str = d_fields[k].c_str();
					if(element.hasElement(temp_field_str))
					{
						Element temp_field = element.getElement(temp_field_str);
						const Name TEMP_FIELD_STR(temp_field_str);

						datatype = temp_field.datatype();

						switch(datatype)
						{
							case BLPAPI_DATATYPE_BOOL://Bool
							{
								blpapi_Bool_t field1;
								field1 = element.getElementAsBool(TEMP_FIELD_STR);
								std::cout << field1 << *delimiter;
								break;
							}
							case BLPAPI_DATATYPE_CHAR://Char
							{
								char field1;
								field1 = element.getElementAsChar(TEMP_FIELD_STR);
								std::cout << field1 << *delimiter;
								break;
							}
							case BLPAPI_DATATYPE_INT32://Int32
							{
								blpapi_Int32_t field1;
								field1 = element.getElementAsInt32(TEMP_FIELD_STR);
								std::cout << field1 << *delimiter;
								break;
							}
							case BLPAPI_DATATYPE_INT64://Int64
							{
								 blpapi_Int64_t field1;
								 field1 = element.getElementAsInt64(TEMP_FIELD_STR);
								 std::cout << field1 << *delimiter;
								 break;
							}
							case BLPAPI_DATATYPE_FLOAT32://Float32
							{
								 blpapi_Float32_t field1;
								 field1 = element.getElementAsFloat32(TEMP_FIELD_STR);
								 std::cout << field1 << *delimiter;
								 break;
							}
							case BLPAPI_DATATYPE_FLOAT64://Float64
							{
								 blpapi_Float64_t field1;
								 field1 = element.getElementAsFloat64(TEMP_FIELD_STR);
								 std::cout << field1 << *delimiter;
								 break;
							}
							case BLPAPI_DATATYPE_STRING://String
							{
								 const char *field1;
								 field1 = element.getElementAsString(0);
								 std::cout << field1 << *delimiter;
								 break;
							}
							case BLPAPI_DATATYPE_DATE://Date
							{
								 Datetime field1;
								 field1 = element.getElementAsDatetime(TEMP_FIELD_STR);
								 std::cout << field1.year() << '/' << field1.month() << '/' << field1.day() << *delimiter;
								 break;
							}
							case BLPAPI_DATATYPE_TIME://Time
							{
								 Datetime field1;
								 field1 = element.getElementAsDatetime(TEMP_FIELD_STR);
								 std::cout << field1.hours() << '/' << field1.minutes() << '/' << field1.seconds() << *delimiter;
								 break;
							}
							case BLPAPI_DATATYPE_DATETIME://Datetime
							{
								 Datetime field1;
								 field1 = element.getElementAsDatetime(TEMP_FIELD_STR);
								 std::cout << field1.year() << '/' << field1.month() << '/' << field1.day() << '/';
								 std::cout << field1.hours() << '/' << field1.minutes() << '/' << field1.seconds() << *delimiter;
								 break;
							}
							default:
							{
								 const char *field1;
								 field1 = element.getElementAsString(TEMP_FIELD_STR);
								 std::cout << field1 << *delimiter;
								 break;
							}
						}//end of switch
					}//end of if
				}//enf of for
			printf("\n");
			}//end of for
		}//end of if
	}//end of method
};

int main(int argc, char **argv)
{
    std::cout << "HistoryExample" << std::endl;
    HistoryExample example;
    example.run(argc, argv);
    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0; 
}
