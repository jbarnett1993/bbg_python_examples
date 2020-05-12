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

// SimpleHistoryExample.cpp : Defines the entry point for the console application.
//
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

std::string              d_host;
int                      d_port;
std::vector<std::string> d_securities;
std::vector<std::string> d_fields;
std::string        		 d_startDate;
std::string          	 d_endDate;

namespace
{
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
}

using BloombergLP::blpapi::Event;
using BloombergLP::blpapi::Element;
using BloombergLP::blpapi::Message;
using BloombergLP::blpapi::Name;
using BloombergLP::blpapi::Request;
using BloombergLP::blpapi::Service;
using BloombergLP::blpapi::Session;
using BloombergLP::blpapi::SessionOptions;

class SimpleHistoryExample
{
	std::string         d_host;
    int                 d_port;

	void printUsage()
	{
	  std::cout << "Usage:" << std::endl
				  << "	Retrieve historical data " << std::endl
			  << "		[-s		<security	= IBM US Equity>" << std::endl
			  << "		[-f		<field		= PX_LAST>" << std::endl
			  << "		[-sd  <startDateTime  = 20091026" << std::endl
			  << "		[-ed  <endDateTime    = 20091030" << std::endl
				  << "		[-ip 	<ipAddress	= localhost>" << std::endl
				  << "		[-p 	<tcpPort	= 8194\n>" << std::endl;
	}

	bool parseCommandLine(int argc, char **argv)
	{
		std::string date;
		for (int i = 1; i < argc; ++i)
		{
		  if (!std::strcmp(argv[i],"-s") && i + 1 < argc)
		  {
			d_securities.push_back(argv[++i]);
		  }
		  else if (!std::strcmp(argv[i],"-f") && i + 1 < argc)
		  {
			d_fields.push_back(argv[++i]);
		  }
		  else if (!std::strcmp(argv[i],"-sd") && i + 1 < argc)
		  {
			d_startDate = argv[++i];
		  }
		  else if (!std::strcmp(argv[i],"-ed") && i + 1 < argc)
		  {
			d_endDate = argv[++i];
		  }
		  else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc)
		  {
				d_host = argv[++i];
		  }
		  else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc)
		  {
				d_port = std::atoi(argv[++i]);
		  }
		  else
		  {
			// only do this if no valid arguments have been passed in
			// VS passes in the file path as an argument during debug
			printUsage();
			return false;
		  }
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

    public:
    void run(int argc, char **argv)
    {
        d_host = "localhost";
        d_port = 8194;
        if (!parseCommandLine(argc, argv)) return;

        SessionOptions sessionOptions;
        sessionOptions.setServerHost(d_host.c_str());
        sessionOptions.setServerPort(d_port);

        std::cout << "Connecting to " <<  d_host << ":" << d_port << std::endl;
        Session session(sessionOptions);
        if (!session.start())
		{
            std::cerr <<"Failed to start session." << std::endl;
            return;
        }
        if (!session.openService("//blp/refdata"))
		{
            std::cerr <<"Failed to open //blp/refdata" << std::endl;
            return;
        }
        Service refDataService = session.getService("//blp/refdata");
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

        std::cout << "Sending Request: " << request << std:: endl;
        session.sendRequest(request);

        while (true)
		{
            Event event = session.nextEvent();
            MessageIterator msgIter(event);
            while (msgIter.next())
			{
                Message &msg = msgIter.message();

				if ((event.eventType() != Event::PARTIAL_RESPONSE) && (event.eventType() != Event::RESPONSE))
				{
					continue;
				}
				Element securityData = msg.getElement(SECURITY_DATA);
				Element securityName = securityData.getElement(SECURITY_NAME);
				std::cout << securityName << "\n\n";

				//only process field data if no errors have occurred
				if(!ProcessErrors(msg))
				{
					ProcessExceptions(msg);
					ProcessFields(msg);
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
    std::cout << "SimpleHistoryExample" << std::endl;
    SimpleHistoryExample example;
    example.run(argc, argv);
    // wait for enter key to exit application
    std::cout << "Press ENTER to quit" << std::endl;
    char dummy[2];
    std::cin.getline(dummy, 2);
    return 0;
}
