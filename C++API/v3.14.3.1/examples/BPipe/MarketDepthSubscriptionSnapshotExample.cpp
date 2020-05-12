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
#include <blpapi_correlationid.h>

#include <vector>
#include <string>
#include <stdlib.h>
#include <time.h>
#include <stdio.h>
#include <iostream>
#include <iomanip>
#include "OrderBook.h"
#include "LevelBook.h"
#include "SyncIO.h"

using namespace std;
using namespace BloombergLP;
using namespace blpapi;


namespace {
    Name EXCEPTIONS("exceptions");
    Name FIELD_ID("fieldId");
    Name REASON("reason");
	Name SOURCE("source");
	Name ERROR_CODE("errorCode");
	Name SUBCATEGORY("reason");
    Name CATEGORY("category");
    Name DESCRIPTION("description");

	const Name TOKEN_SUCCESS("TokenGenerationSuccess");
	const Name TOKEN_FAILURE("TokenGenerationFailure");
	const Name AUTHORIZATION_SUCCESS("AuthorizationSuccess");
	const Name AUTHORIZATION_FAILURE("AuthorizationFailure");
	const Name TOKEN("token");

	const Name MKTDEPTH_EVENT_TYPE("MKTDEPTH_EVENT_TYPE");
	const Name MKTDEPTH_EVENT_SUBTYPE("MKTDEPTH_EVENT_SUBTYPE");
	const Name MD_GAP_DETECTED("MD_GAP_DETECTED");
	const Name MD_TABLE_CMD_RT("MD_TABLE_CMD_RT");
	const Name MD_BOOK_TYPE("MD_BOOK_TYPE");
	const Name MD_MULTI_TICK_UPD_RT("MD_MULTI_TICK_UPD_RT");
	const Name MBO_WINDOW_SIZE("MBO_WINDOW_SIZE");
	const Name MBO_ASK_POSITION_RT("MBO_ASK_POSITION_RT");
	const Name MBO_ASK_RT("MBO_ASK_RT");
	const Name MBO_ASK_BROKER_RT("MBO_ASK_BROKER_RT");
	const Name MBO_ASK_COND_CODE_RT("MBO_ASK_COND_CODE_RT");
	const Name MBO_ASK_SIZE_RT("MBO_ASK_SIZE_RT");
	const Name MBO_TABLE_ASK("MBO_TABLE_ASK");
	const Name MBO_BID_POSITION_RT("MBO_BID_POSITION_RT");
	const Name MBO_BID_RT("MBO_BID_RT");
	const Name MBO_BID_BROKER_RT("MBO_BID_BROKER_RT");
	const Name MBO_BID_COND_CODE_RT("MBO_BID_COND_CODE_RT");
	const Name MBO_BID_SIZE_RT("MBO_BID_SIZE_RT");
	const Name MBO_TABLE_BID("MBO_TABLE_BID");
	const Name MBO_TIME_RT("MBO_TIME_RT");
	const Name MBO_SEQNUM_RT("MBO_SEQNUM_RT");
	const Name MBL_WINDOW_SIZE("MBL_WINDOW_SIZE");
	const Name MBL_ASK_POSITION_RT("MBL_ASK_POSITION_RT");
	const Name MBL_ASK_RT("MBL_ASK_RT");
	const Name MBL_ASK_NUM_ORDERS_RT("MBL_ASK_NUM_ORDERS_RT");
	const Name MBL_ASK_COND_CODE_RT("MBL_ASK_COND_CODE_RT");
	const Name MBL_ASK_SIZE_RT("MBL_ASK_SIZE_RT");
	const Name MBL_TABLE_ASK("MBL_TABLE_ASK");
	const Name MBL_BID_POSITION_RT("MBL_BID_POSITION_RT");
	const Name MBL_BID_RT("MBL_BID_RT");
	const Name MBL_BID_NUM_ORDERS_RT("MBL_BID_NUM_ORDERS_RT");
	const Name MBL_BID_COND_CODE_RT("MBL_BID_COND_CODE_RT");
	const Name MBL_BID_SIZE_RT("MBL_BID_SIZE_RT");
	const Name MBL_TABLE_BID("MBL_TABLE_BID");
	const Name MBL_TIME_RT("MBL_TIME_RT");
	const Name MBL_SEQNUM_RT("MBL_SEQNUM_RT");
	const Name NONE("NONE");
	
	const Name ADD("ADD");
	const Name DEL("DEL");
	const Name DELALL("DELALL");
	const Name DELBETTER("DELBETTER");
	const Name DELSIDE("DELSIDE");
	const Name EXEC("EXEC");
	const Name MOD("MOD");
	const Name REPLACE("REPLACE");
	const Name REPLACE_BY_BROKER("REPLACE_BY_BROKER");
	const Name MARKET_BY_LEVEL("MARKET_BY_LEVEL");
	const Name MARKET_BY_ORDER("MARKET_BY_ORDER");
	const Name CLEARALL("CLEARALL");
	const Name REPLACE_CLEAR("REPLACE_CLEAR");
	const Name REPLACE_BY_PRICE("REPLACE_BY_PRICE");
	
	const Name ASK("ASK");
	const Name BID("BID");
	const Name ASK_RETRANS("ASK_RETRANS");
	const Name BID_RETRANS("BID_RETRANS");
	const Name TABLE_INITPAINT("TABLE_INITPAINT");
	const Name TABLE_UPDATE("TABLE_UPDATE");

    const char* authServiceName = "//blp/apiauth";
	const std::string mktDepthServiceName = "//blp/mktdepthdata";
	
	const int BIDSIDE = 0;
	const int ASKSIDE = 1;

	const int UNKNOWN = -1;
	const int BYORDER = 0;
	const int BYLEVEL = 1;

	const int bookSize = 2;

	Name PRICE_FIELD[2][2] = {MBO_BID_RT, MBO_ASK_RT, 
							  MBL_BID_RT, MBL_ASK_RT};
	Name SIZE_FIELD[2][2] = {MBO_BID_SIZE_RT, MBO_ASK_SIZE_RT, 
							 MBL_BID_SIZE_RT, MBL_ASK_SIZE_RT};
	Name POSITION_FIELD[2][2] = {MBO_BID_POSITION_RT, MBO_ASK_POSITION_RT, 
								 MBL_BID_POSITION_RT, MBL_ASK_POSITION_RT};
	Name ORDER_FIELD[2][2] = {NONE, NONE,
							  MBL_BID_NUM_ORDERS_RT, MBL_ASK_NUM_ORDERS_RT} ;
	Name BROKER_FIELD[2][2] = {MBO_BID_BROKER_RT, MBO_ASK_BROKER_RT, 
							   NONE, NONE};
	Name TIME_FIELD[2] = {MBO_TIME_RT, MBL_TIME_RT};

	/* Protects the cache and prevents simultaneous output to stdout.  */
	SyncIO syncio;
}


class SubscriptionEventHandler: public EventHandler
{
	Session *d_session;
    SubscriptionList &d_subscriptions; 
	ByOrderBook *d_orderBooks;
	ByLevelBook *d_levelBooks;
	int *d_marketDepthBook;
	int d_showTicks;
	int d_gapDetected;
	int d_askRetran;
	int d_bidRetran;
	int d_resubscribed;
	long d_sequenceNumber;
	/* prevents simultaneous output to stdout.  */
	SyncIO syncio;

	/*------------------------------------------------------------------------------------
	 * Name			: getTimeStamp
	 * Description	: get current time stamp
	 * Arguments	: buffer is the return time stamp
	 *              : size of buffer
	 * Returns		: status value of strftime()
	 *------------------------------------------------------------------------------------*/
    size_t getTimeStamp(char *buffer, size_t bufSize)
    {
        const char *format = "%Y/%m/%d %X";
		size_t i;

        time_t now = time(0);
#ifdef WIN32
        //tm *timeInfo = localtime(&now);
		struct tm timeInfo;
		localtime_s(&timeInfo, &now);
        i = strftime(buffer, bufSize, format, &timeInfo);
#else
        tm _timeInfo;
        tm *timeInfo = localtime_r(&now, &_timeInfo);
        i = strftime(buffer, bufSize, format, timeInfo);
#endif
		return i;
	}

	/*------------------------------------------------------------------------------------
	 * Name			: printFragType
	 * Description	: print fragment name
	 * Arguments	: type is the fragment type
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	void printFragType(int type)
	{
		if(type == Message::FRAGMENT_NONE)
		{
			std::cout << "Fragment Type - Message::FRAGMENT_NONE" << std::endl;
		}
		else if(type == Message::FRAGMENT_START)
		{ 
		  std::cout << "Fragment Type - Message::FRAGMENT_START" << std::endl;
		}
		else if(type == Message::FRAGMENT_INTERMEDIATE)
		{
		  std::cout << "Fragment Type - Message::FRAGMENT_INTERMEDIATE" << std::endl;
		}
		else if(type == Message::FRAGMENT_END)
		{
		  std::cout << "Fragment Type - Message::FRAGMENT_END" << std::endl;
		}
		else
		{
		  std::cout << "Fragment Type - Unknown" << std::endl;
		}
	}

	/*------------------------------------------------------------------------------------
	 * Name			: processSubscriptionStatus
	 * Description	: process subscription status events
	 * Arguments	: none
	 * Returns		: true - successful
	 *------------------------------------------------------------------------------------*/
    void processSubscriptionStatus(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
			const char* msg_type = msg.messageType().string();
			std::string *topic = reinterpret_cast<std::string*>(msg.correlationId().asPointer());
			syncio.lock();
			std::cout << timeBuffer << ": " << topic->c_str() << " - " << msg.messageType().string() << std::endl;
            if (msg.hasElement(REASON, true)) {
                // This can occur on SubscriptionFailure.
				msg.print(std::cout);
            }
			syncio.unlock();

            if (msg.hasElement(EXCEPTIONS, true)) {
                // This can occur on SubscriptionStarted if at least
                // one field is good while the rest are bad.
                Element exceptions = msg.getElement(EXCEPTIONS);
                for (size_t i = 0; i < exceptions.numValues(); ++i) {
                    Element exInfo = exceptions.getValueAsElement(i);
                    Element fieldId = exInfo.getElement(FIELD_ID);
                    Element reason = exInfo.getElement(REASON);
					syncio.lock();
					std::cout << "        " << fieldId.getValueAsString() << ": " 
						<< reason.getElement(CATEGORY).getValueAsString() << std::endl;
					syncio.unlock();
                }
            }
        }
    }

	/*------------------------------------------------------------------------------------
	 * Name			: processSubscriptionDataEvent
	 * Description	: process market depth data events
	 * Arguments	: event is the data event
	 *              : session is the API session
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    bool processSubscriptionDataEvent(const Event &event, Session *session)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));
		
        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
			const char* msg_type = msg.messageType().string();
			if(strcmp(msg_type,"MarketDepthUpdates") == 0){
				// Market Depth data
				if (d_showTicks > 0)
				{
					// output tick message
					syncio.lock();
					std::cout << timeBuffer << ": ";
					printFragType(msg.fragmentType()); 

					msg.print(std::cout);
					std::cout << std::flush;
					syncio.unlock();
				}

				// setup book type before processing data
				if (*d_marketDepthBook == UNKNOWN)
				{
					Element bookType;
					if (!msg.asElement().getElement(&bookType, MKTDEPTH_EVENT_TYPE))
					{
						Name value;
						if (!bookType.getValueAs(&value, 0))
						{
							if (value == MARKET_BY_ORDER)
							{
								*d_marketDepthBook = BYORDER;
							}
							else if (value == MARKET_BY_LEVEL)
							{
								*d_marketDepthBook = BYLEVEL;
							}
						}
					}
				}

				// process base on book type
				switch (*d_marketDepthBook)
				{
					case BYLEVEL:
						processByLevelMessage(msg, session);
						break;
					case BYORDER:
						processByOrderMessage(msg, session);
						break;
					default:
						// display unknown book type message
						syncio.lock();
						std::cout << timeBuffer << ": Unknown book type. Can not process message." << std::endl;
						std::cout << timeBuffer << ": ";
						printFragType(msg.fragmentType()); 

						msg.print(std::cout);
						std::cout << std::flush;
						syncio.unlock();
						break;
				}
			}
        }
        return true;
    }

	/*------------------------------------------------------------------------------------
	 * Name			: processByOrderEvent
	 * Description	: process by order message
	 * Arguments	: msg is the tick data message
	 *              : session is the API session
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	void processByOrderMessage(const Message &msg, Session *session)
    {
        int side = -1;
		int position = -1;
		int bidRetran = 0;
		int askRetran = 0;

		// get gap detection flag (AMD book only)
	    if (msg.hasElement(MD_GAP_DETECTED, true) && !d_gapDetected) {
	    	d_gapDetected = true;
	    	syncio.lock();
			std::cout << "Bloomberg detected a gap in data stream." << std::endl;
			syncio.unlock();
	    }

		// get event sub type
		Name subType = msg.getElement(MKTDEPTH_EVENT_SUBTYPE).getValueAsName();
        // get retran flags
		bidRetran = (subType == BID_RETRANS) ? 1 : 0;
		askRetran = (subType == ASK_RETRANS) ? 1 : 0;
		// BID/ASK message
		if (subType == BID || subType == ASK || 
			bidRetran || askRetran) {
			if(subType == BID || bidRetran) {
				// bid side
				side = BIDSIDE;
			} else if (subType == ASK || askRetran) {
				// ask side
				side = ASKSIDE;
			}

			// get position
			int position = -1;
			if (msg.hasElement(POSITION_FIELD[BYORDER][side], true)) {
				position = msg.getElement(POSITION_FIELD[BYORDER][side]).getValueAsInt32();
				if (position > 0) --position;
			}

		    //  BID/ASK retran message
		    if (askRetran || bidRetran) {
			    // check for multi tick
		    	if (msg.hasElement(MD_MULTI_TICK_UPD_RT, true)) {
		    		// multi tick
		    		if (msg.getElement(MD_MULTI_TICK_UPD_RT).getValueAsInt32() == 0 ) {
				    	// last multi tick message, reset sequence number so next non-retran
		    			// message sequence number will be use as new starting number
				    	d_sequenceNumber = 0;
		    			if (askRetran && d_askRetran) {
		    				// end of ask retran
		    				d_askRetran = false;
		    		    	syncio.lock();
							std::cout << "Ask retran completed." << std::endl;
		    				syncio.unlock();
		    			} else if (bidRetran && d_bidRetran) {
		    				// end of ask retran
		    				d_bidRetran = false;
		    		    	syncio.lock();
							std::cout << "Bid retran completed." << std::endl;
		    				syncio.unlock();
		    			}
		    			if (!(d_askRetran || d_bidRetran)) {
		    				// retran completed
		    		    	syncio.lock();
	    		    		if (d_gapDetected) {
	    		    			// gap detected retran completed
	    		    			d_gapDetected = false;
								std::cout << "Gap detected retran completed." << std::endl;
	    		    		} else {
	    		    			// normal retran completed
								std::cout << "Retran completed." << std::endl;
	    		    		}
		    				syncio.unlock();
		    			}
		    		} else {
		    			if (askRetran && !d_askRetran) {
		    				// start of ask retran
		    				d_askRetran = true;
		    		    	syncio.lock();
							std::cout << "Ask retran started." << std::endl;
		    				syncio.unlock();
		    			} else if (bidRetran && !d_bidRetran) {
		    				// start of ask retran
		    				d_bidRetran = true;
		    		    	syncio.lock();
							std::cout << "Bid retran started." << std::endl;
		    				syncio.unlock();
		    			}
		    		}
		    	}
		    } else if (msg.hasElement(MBO_SEQNUM_RT, true)) {
		    	// get sequence number
		    	long currentSequence = (long)msg.getElementAsInt64(MBO_SEQNUM_RT);
		    	if (d_sequenceNumber == 0 || d_sequenceNumber == 1 ||
                        (currentSequence == 1 && d_sequenceNumber > 1)) {
		    		// use current sequence number
		    		d_sequenceNumber = currentSequence;
		    	} else if ((d_sequenceNumber + 1 != currentSequence) && !d_gapDetected) {
		    		if (!d_resubscribed)
		    		{
				    	// previous tick sequence can not be smaller than current tick 
				    	// sequence number - 1 and NOT in gap detected mode. 
			    		syncio.lock();
						std::cout << "Warning: Gap detected - previous sequence number is " << 
		    					d_sequenceNumber << " and current tick sequence number is " <<
								currentSequence << ")." << std::endl;
			    		syncio.unlock();
			    		// gap detected, re-subscribe to securities
						session->resubscribe(d_subscriptions);
						d_resubscribed = true;
		    		}
		    	} else if (d_sequenceNumber >= currentSequence) {
		    		// previous tick sequence number can not be greater or equal
		    		// to current sequence number
		    		syncio.lock();
					std::cout << "Warning: Current Sequence number (" << currentSequence <<
	    					") is smaller or equal to previous tick sequence number (" <<
							d_sequenceNumber << ")." << std::endl;
		    		syncio.unlock();
		    	} else {
		    		// save current sequence number
		    		d_sequenceNumber = currentSequence;
		    	}
		    }

			// get command
			Name cmd = msg.getElement(MD_TABLE_CMD_RT).getValueAsName();
			if (cmd == CLEARALL) {
				d_orderBooks[side].doClearAll();
			} else if (cmd == DEL) {
				d_orderBooks[side].doDel(position);
			} else if (cmd == DELALL) {
				d_orderBooks[side].doDelAll();
			} else if (cmd == DELBETTER) {
				d_orderBooks[side].doDelBetter(position);
			} else if (cmd == DELSIDE) {
				d_orderBooks[side].doDelSide();
			} else if (cmd == REPLACE_CLEAR) {
				d_orderBooks[side].doReplaceClear(position);
			} else {
				// process other data commands
				// get price
				double fPrice = msg.getElement(PRICE_FIELD[BYORDER][side]).getValueAsFloat64();
				// get size
				unsigned int nSize = 0;
				if (msg.hasElement(SIZE_FIELD[BYORDER][side], true)) {
					nSize = (unsigned int)msg.getElement(SIZE_FIELD[BYORDER][side]).getValueAsInt64();
				}
				// get broker
				std::string sBroker = "";
				if (msg.hasElement(BROKER_FIELD[BYORDER][side], true)) {
					sBroker = msg.getElement(BROKER_FIELD[BYORDER][side]).getValueAsString();
				}
				// get time
				Datetime timeStamp = msg.getElement(TIME_FIELD[BYORDER]).getValueAsDatetime();
				std::stringstream ssTime;
				ssTime << setfill('0') << setw(2) << timeStamp.hours() << 
					":" << setfill('0') << setw(2) << timeStamp.minutes() << 
					":" << setfill('0') << setw(2) << timeStamp.seconds() <<
					"." << setfill('0') << setw(3) << timeStamp.milliSeconds();
				// create entry
				ByOrderBookEntry entry(sBroker, (float)fPrice, ssTime.str(), 	0, nSize);

				// process data command
				if(cmd == ADD)
					d_orderBooks[side].doAdd(position, entry);
				else if(cmd == MOD)
					d_orderBooks[side].doMod(position, entry);
				else if(cmd == REPLACE)
					d_orderBooks[side].doReplace(position, entry);
				else if(cmd == REPLACE_BY_BROKER)
					d_orderBooks[side].doReplaceByBroker(entry);
				else if(cmd == EXEC)
					d_orderBooks[side].doExec(position, entry);
			}
		} else {
			if (subType == TABLE_INITPAINT) {
				if (msg.fragmentType() == Message::FRAGMENT_START ||
					msg.fragmentType() == Message::FRAGMENT_NONE) {
					// init paint
					if (msg.hasElement(MBO_WINDOW_SIZE, true) ){
						d_orderBooks[ASKSIDE].window_size = (unsigned int) msg.getElementAsInt64(MBO_WINDOW_SIZE);
						d_orderBooks[BIDSIDE].window_size = d_orderBooks[ASKSIDE].window_size;
					}
					d_orderBooks[ASKSIDE].book_type = msg.getElementAsString(MD_BOOK_TYPE);
					d_orderBooks[BIDSIDE].book_type = d_orderBooks[ASKSIDE].book_type;
					// clear cache
					d_orderBooks[ASKSIDE].doClearAll();
					d_orderBooks[BIDSIDE].doClearAll();
				}

				// ASK table
				Element askTable;
				if ((msg.asElement().getElement(&askTable, MBO_TABLE_ASK) == 0) && !askTable.isNull()){
					// has ask table array
					size_t numOfItems = askTable.numValues();
					for (size_t index = 0; index < numOfItems; ++index) {
						Element ask = askTable.getValueAsElement(index);
						// get command
						Name cmd = ask.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
						// get position
						int position = -1;
						if (ask.hasElement(POSITION_FIELD[BYORDER][ASKSIDE], true)) {
							position = ask.getElement(POSITION_FIELD[BYORDER][ASKSIDE]).getValueAsInt32();
							if (position > 0) --position;
						}
						// get price
						double askPrice = ask.getElement(PRICE_FIELD[BYORDER][ASKSIDE]).getValueAsFloat64();
						// get size
						unsigned int askSize = 0;
						if (ask.hasElement(SIZE_FIELD[BYORDER][ASKSIDE], true)) {
							askSize = (unsigned int)ask.getElement(SIZE_FIELD[BYORDER][ASKSIDE]).getValueAsInt64();
						}
						// get broker
						std::string askBroker = "";
						if (ask.hasElement(BROKER_FIELD[BYORDER][ASKSIDE], true)) {
							askBroker = ask.getElement(BROKER_FIELD[BYORDER][ASKSIDE]).getValueAsString();
						}
						// get time
						Datetime timeStamp = ask.getElement(TIME_FIELD[BYORDER]).getValueAsDatetime();
						std::stringstream askTime;
						askTime << setfill('0') << setw(2) << timeStamp.hours() << 
							":" << setfill('0') << setw(2) << timeStamp.minutes() << 
							":" << setfill('0') << setw(2) << timeStamp.seconds() <<
							"." << setfill('0') << setw(3) << timeStamp.milliSeconds();
						// create entry
						ByOrderBookEntry entry(askBroker, (float)askPrice, askTime.str(), 	0, askSize);

						// process data command
						if(cmd == ADD)
							d_orderBooks[ASKSIDE].doAdd(position, entry);
						else if(cmd == MOD)
							d_orderBooks[ASKSIDE].doMod(position, entry);
						else if(cmd == REPLACE)
							d_orderBooks[ASKSIDE].doReplace(position, entry);
						else if(cmd == REPLACE_BY_BROKER)
							d_orderBooks[ASKSIDE].doReplaceByBroker(entry);
						else if(cmd == EXEC)
							d_orderBooks[ASKSIDE].doExec(position, entry);
					}
				}
				// BID table
				Element bidTable;
				if ((msg.asElement().getElement(&bidTable, MBO_TABLE_BID) == 0) && !bidTable.isNull()){
					// has bid table array
					size_t numOfItems = bidTable.numValues();
					for (size_t index = 0; index < numOfItems; ++index) {
						Element bid = bidTable.getValueAsElement(index);
						// get command
						Name cmd = bid.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
						// get position
						int position = -1;
						if (bid.hasElement(POSITION_FIELD[BYORDER][BIDSIDE], true)) {
							position = bid.getElement(POSITION_FIELD[BYORDER][BIDSIDE]).getValueAsInt32();
							if (position > 0) --position;
						}
						// get price
						double bidPrice = bid.getElement(PRICE_FIELD[BYORDER][BIDSIDE]).getValueAsFloat64();
						// get size
						unsigned int bidSize = 0;
						if (bid.hasElement(SIZE_FIELD[BYORDER][BIDSIDE], true)) {
							bidSize = (unsigned int)bid.getElement(SIZE_FIELD[BYORDER][BIDSIDE]).getValueAsInt64();
						}
						// get broker
						std::string bidBroker = "";
						if (bid.hasElement(BROKER_FIELD[BYORDER][BIDSIDE], true)) {
							bidBroker = bid.getElement(BROKER_FIELD[BYORDER][BIDSIDE]).getValueAsString();
						}
						// get time
						Datetime timeStamp = bid.getElement(TIME_FIELD[BYORDER]).getValueAsDatetime();
						std::stringstream bidTime;
						bidTime << setfill('0') << setw(2) << timeStamp.hours() << 
							":" << setfill('0') << setw(2) << timeStamp.minutes() << 
							":" << setfill('0') << setw(2) << timeStamp.seconds() <<
							"." << setfill('0') << setw(3) << timeStamp.milliSeconds();
						// create entry
						ByOrderBookEntry entry(bidBroker, (float)bidPrice, bidTime.str(), 0, bidSize);

						// process data command
						if(cmd == ADD)
							d_orderBooks[BIDSIDE].doAdd(position, entry);
						else if(cmd == MOD)
							d_orderBooks[BIDSIDE].doMod(position, entry);
						else if(cmd == REPLACE)
							d_orderBooks[BIDSIDE].doReplace(position, entry);
						else if(cmd == REPLACE_BY_BROKER)
							d_orderBooks[BIDSIDE].doReplaceByBroker(entry);
						else if(cmd == EXEC)
							d_orderBooks[BIDSIDE].doExec(position, entry);
					}
				}
			}
		}
	}

	/*------------------------------------------------------------------------------------
	 * Name			: processByLevelEvent
	 * Description	: process by level message
	 * Arguments	: msg is the tick data message
	 *              : session is the API session
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    void processByLevelMessage(const Message &msg, Session *session)
    {
        int side = -1;
		int position = -1;
		int bidRetran = 0;
		int askRetran = 0;

	    // get gap detection flag (AMD book only)
	    if (msg.hasElement(MD_GAP_DETECTED, true) && !d_gapDetected) {
	    	d_gapDetected = true;
	    	syncio.lock();
			std::cout << "Bloomberg detected a gap in data stream." << std::endl;
			syncio.unlock();
	    }

		// get event subtype
		Name subType = msg.getElement(MKTDEPTH_EVENT_SUBTYPE).getValueAsName();
        // get retran flags
		bidRetran = (subType == BID_RETRANS) ? 1 : 0;
        askRetran = (subType == ASK_RETRANS) ? 1 : 0;
        // BID or ASK message
		if (subType == BID || subType == ASK ||
			bidRetran || askRetran) {
			// set book size
			if(subType == BID || bidRetran) {
				side = BIDSIDE;
			} else if (subType == ASK ||askRetran) {
				side = ASKSIDE;
			}

			// get position
			int position = -1;
			if (msg.hasElement(POSITION_FIELD[BYLEVEL][side], true)) {
				position = msg.getElement(POSITION_FIELD[BYLEVEL][side]).getValueAsInt32();
				if (position > 0) --position;
			}

		    //  BID/ASK retran message
		    if (askRetran || bidRetran) {
			    // check for multi tick
		    	if (msg.hasElement(MD_MULTI_TICK_UPD_RT, true)) {
		    		// multi tick
		    		if (msg.getElement(MD_MULTI_TICK_UPD_RT).getValueAsInt32() == 0 ) {
				    	// last multi tick message, reset sequence number so next non-retran
		    			// message sequence number will be use as new starting number
				    	d_sequenceNumber = 0;
		    			if (askRetran && d_askRetran) {
		    				// end of ask retran
		    				d_askRetran = false;
		    		    	syncio.lock();
							std::cout << "Ask retran completed." << std::endl;
		    				syncio.unlock();
		    			} else if (bidRetran && d_bidRetran) {
		    				// end of ask retran
		    				d_bidRetran = false;
		    		    	syncio.lock();
							std::cout << "Bid retran completed." << std::endl;
		    				syncio.unlock();
		    			}
		    			if (!(d_askRetran || d_bidRetran)) {
		    				// retran completed
		    		    	syncio.lock();
	    		    		if (d_gapDetected) {
	    		    			// gap detected retran completed
	    		    			d_gapDetected = false;
								std::cout << "Gap detected retran completed." << std::endl;
	    		    		} else {
	    		    			// normal retran completed
								std::cout << "Retran completed." << std::endl;
	    		    		}
		    				syncio.unlock();
		    			}
		    		} else {
		    			if (askRetran && !d_askRetran) {
		    				// start of ask retran
		    				d_askRetran = true;
		    		    	syncio.lock();
							std::cout << "Ask retran started." << std::endl;
		    				syncio.unlock();
		    			} else if (bidRetran && !d_bidRetran) {
		    				// start of ask retran
		    				d_bidRetran = true;
		    		    	syncio.lock();
							std::cout << "Bid retran started." << std::endl;
		    				syncio.unlock();
		    			}
		    		}
		    	}
		    } else if (msg.hasElement(MBL_SEQNUM_RT, true)) {
		    	// get sequence number
		    	long currentSequence = (long)msg.getElementAsInt64(MBL_SEQNUM_RT);
		    	if (d_sequenceNumber == 0 || d_sequenceNumber == 1 ||
                        (currentSequence == 1 && d_sequenceNumber > 1)) {
		    		// use current sequence number
		    		d_sequenceNumber = currentSequence;
		    	} else if ((d_sequenceNumber + 1 != currentSequence) && !d_gapDetected) {
		    		if (!d_resubscribed)
		    		{
				    	// previous tick sequence can not be smaller than current tick 
				    	// sequence number - 1 and NOT in gap detected mode. 
						syncio.lock();
						std::cout << "Warning: Gap detected - previous sequence number is " << 
		    					d_sequenceNumber << " and current tick sequence number is " <<
								currentSequence << ")." << std::endl;
			    		syncio.unlock();
			    		// gap detected, re-subscribe to securities
						session->resubscribe(d_subscriptions);
						d_resubscribed = true;
		    		}
		    	} else if (d_sequenceNumber >= currentSequence) {
		    		// previous tick sequence number can not be greater or equal
		    		// to current sequence number
		    		syncio.lock();
					std::cout << "Warning: Current Sequence number (" << currentSequence << 
    						") is smaller or equal to previous tick sequence number (" <<
							d_sequenceNumber << ")." << std::endl;
		    		syncio.unlock();
		    	} else {
		    		// save current sequence number
		    		d_sequenceNumber = currentSequence;
		    	}
		    }

			// get command
			Name cmd = msg.getElement(MD_TABLE_CMD_RT).getValueAsName();
			if (cmd == CLEARALL) {
				d_levelBooks[side].doClearAll();
			} else if (cmd == DEL) {
				if (position != -1)
					d_levelBooks[side].doDel(position);
			} else if (cmd == DELALL) {
				d_levelBooks[side].doDelAll();
			} else if (cmd == DELBETTER) {
				d_levelBooks[side].doDelBetter(position);
			} else if (cmd == DELSIDE) {
				d_levelBooks[side].doDelSide();
			} else if (cmd == REPLACE_CLEAR) {
				d_levelBooks[side].doReplaceClear(position);
			} else {
				// process other commands
				// get price
				double fPrice = msg.getElement(PRICE_FIELD[BYLEVEL][side]).getValueAsFloat64();
				// get size
				unsigned int nSize = 0;
				if (msg.hasElement(SIZE_FIELD[BYLEVEL][side], true)) {
					nSize = (unsigned int)msg.getElement(SIZE_FIELD[BYLEVEL][side]).getValueAsInt64();
				}
				// get number of order
				unsigned int nNumOrder = 0;
				if (msg.hasElement(ORDER_FIELD[BYLEVEL][side], true)) {
					nNumOrder = (unsigned int)msg.getElement(ORDER_FIELD[BYLEVEL][side]).getValueAsInt64();
				}
				// get time
				Datetime timeStamp = msg.getElement(TIME_FIELD[BYLEVEL]).getValueAsDatetime();
				std::stringstream ssTime;
				ssTime << setfill('0') << setw(2) << timeStamp.hours() << 
					":" << setfill('0') << setw(2) << timeStamp.minutes() << 
					":" << setfill('0') << setw(2) << timeStamp.seconds() <<
							"." << setfill('0') << setw(3) << timeStamp.milliSeconds();
				// create entry
				ByLevelBookEntry entry((float)fPrice, ssTime.str(), nNumOrder, nSize);

				// process data command
				if(cmd == ADD)
					d_levelBooks[side].doAdd(position, entry);
				else if(cmd == MOD)
					d_levelBooks[side].doMod(position, entry);
				else if(cmd == REPLACE)
					d_levelBooks[side].doReplace(position, entry);
				else if(cmd == EXEC)
					d_levelBooks[side].doExec(position, entry);
			}
		} else {
			if (subType == TABLE_INITPAINT) {
				if (msg.fragmentType() == Message::FRAGMENT_START ||
					msg.fragmentType() == Message::FRAGMENT_NONE) {
					// init paint
					if (msg.hasElement(MBL_WINDOW_SIZE, true)){
						d_levelBooks[ASKSIDE].window_size = (unsigned int) msg.getElementAsInt64(MBL_WINDOW_SIZE);
						d_levelBooks[BIDSIDE].window_size = d_levelBooks[ASKSIDE].window_size;
					}
					d_levelBooks[ASKSIDE].book_type = msg.getElementAsString(MD_BOOK_TYPE);
					d_levelBooks[BIDSIDE].book_type = d_levelBooks[ASKSIDE].book_type;
					// clear cache
					d_levelBooks[ASKSIDE].doClearAll();
					d_levelBooks[BIDSIDE].doClearAll();
				}

				// ASK table
				Element askTable;
				if ((msg.asElement().getElement(&askTable, MBL_TABLE_ASK) == 0) && !askTable.isNull()){
					// has ask table array
					size_t numOfItems = askTable.numValues();
					for (size_t index = 0; index < numOfItems; ++index) {
						Element ask = askTable.getValueAsElement(index);
						// get command
						Name cmd = ask.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
						// get position
					    position = -1;
					    if (ask.hasElement(POSITION_FIELD[BYLEVEL][ASKSIDE], true)) {
						    position = ask.getElement(POSITION_FIELD[BYLEVEL][ASKSIDE]).getValueAsInt32();
						    if (position > 0) --position;
					    }
						// get price
						double askPrice = ask.getElement(PRICE_FIELD[BYLEVEL][ASKSIDE]).getValueAsFloat64();
						// get size
						unsigned int askSize = 0;
						if (ask.hasElement(SIZE_FIELD[BYLEVEL][ASKSIDE], true)) {
							askSize = (unsigned int)ask.getElement(SIZE_FIELD[BYLEVEL][ASKSIDE]).getValueAsInt64();
						}
						// get number of order
						unsigned int askNumOrder = 0;
						if (ask.hasElement(ORDER_FIELD[BYLEVEL][ASKSIDE], true)) {
							askNumOrder = (unsigned int)ask.getElement(ORDER_FIELD[BYLEVEL][ASKSIDE]).getValueAsInt64();
						}		
						// get time
						Datetime timeStamp = ask.getElement(TIME_FIELD[BYLEVEL]).getValueAsDatetime();
						std::stringstream askTime;
						askTime << setfill('0') << setw(2) << timeStamp.hours() << 
							":" << setfill('0') << setw(2) << timeStamp.minutes() << 
							":" << setfill('0') << setw(2) << timeStamp.seconds() <<
							"." << setfill('0') << setw(3) << timeStamp.milliSeconds();
						// create entry
						ByLevelBookEntry entry((float)askPrice, askTime.str(), askNumOrder, askSize);

						// process data command
						if(cmd == ADD)
							d_levelBooks[ASKSIDE].doAdd(position, entry);
						else if(cmd == MOD)
							d_levelBooks[ASKSIDE].doMod(position, entry);
						else if(cmd == REPLACE)
							d_levelBooks[ASKSIDE].doReplace(position, entry);
						else if(cmd == EXEC)
							d_levelBooks[ASKSIDE].doExec(position, entry);
					}
				}
				// BID table
				Element bidTable;
				if ((msg.asElement().getElement(&bidTable, MBL_TABLE_BID) == 0) && !bidTable.isNull()){
					// has bid table array
					size_t numOfItems = bidTable.numValues();
					for (size_t index = 0; index < numOfItems; ++index) {
						Element bid = bidTable.getValueAsElement(index);
						// get command
						Name cmd = bid.getElement(MD_TABLE_CMD_RT).getValueAsName(); 
						// get position
						int position = -1;
						if (bid.hasElement(POSITION_FIELD[BYLEVEL][BIDSIDE], true)) {
							position = bid.getElement(POSITION_FIELD[BYLEVEL][BIDSIDE]).getValueAsInt32();
							if (position > 0) --position;
						}
						// get price
						double bidPrice = bid.getElement(PRICE_FIELD[BYLEVEL][BIDSIDE]).getValueAsFloat64();
						// get size
						unsigned int bidSize = 0;
						if (bid.hasElement(SIZE_FIELD[BYLEVEL][BIDSIDE], true)) {
							bidSize = (unsigned int)bid.getElement(SIZE_FIELD[BYLEVEL][BIDSIDE]).getValueAsInt64();
						}
						// get number of order
						unsigned int bidNumOrder = 0;
						if (bid.hasElement(ORDER_FIELD[BYLEVEL][BIDSIDE], true)) {
							bidNumOrder = (unsigned int)bid.getElement(ORDER_FIELD[BYLEVEL][BIDSIDE]).getValueAsInt64();
						}
						// get time
						Datetime timeStamp = bid.getElement(TIME_FIELD[BYLEVEL]).getValueAsDatetime();
						std::stringstream bidTime;
						bidTime << setfill('0') << setw(2) << timeStamp.hours() << 
							":" << setfill('0') << setw(2) << timeStamp.minutes() << 
							":" << setfill('0') << setw(2) << timeStamp.seconds() <<
							"." << setfill('0') << setw(3) << timeStamp.milliSeconds();
						// create entry
						ByLevelBookEntry entry((float)bidPrice, bidTime.str(), bidNumOrder, bidSize);

						// process data command
						if(cmd == ADD)
							d_levelBooks[BIDSIDE].doAdd(position, entry);
						else if(cmd == MOD)
							d_levelBooks[BIDSIDE].doMod(position, entry);
						else if(cmd == REPLACE)
							d_levelBooks[BIDSIDE].doReplace(position, entry);
						else if(cmd == EXEC)
							d_levelBooks[BIDSIDE].doExec(position, entry);
					}
				}
			}
		}
    }

	/*------------------------------------------------------------------------------------
	 * Name			: processMiscEvents
	 * Description	: process misc
	 * Arguments	: event is the API event
	 * Returns		: true - successful, false - failed
	 *------------------------------------------------------------------------------------*/
	bool processMiscEvents(const Event &event)
    {
        char timeBuffer[64];
        getTimeStamp(timeBuffer, sizeof(timeBuffer));

        MessageIterator msgIter(event);
        while (msgIter.next()) {
            Message msg = msgIter.message();
			syncio.lock();
			std::cout << timeBuffer << ": " << msg.messageType().string() << std::endl;
			syncio.unlock();
        }
        return true;
    }

public:
	/*------------------------------------------------------------------------------------
	 * Name			: SubscriptionEventHandler
	 * Description	: event handler constructor
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	SubscriptionEventHandler(ByOrderBook *orderBooks, 
		ByLevelBook *levelBooks, int &book, int showTicks, 
		SubscriptionList &subscriptions) 
		: d_orderBooks(orderBooks), 
		d_levelBooks(levelBooks), 
		d_marketDepthBook(&book),
		d_showTicks(showTicks),
		d_subscriptions(subscriptions)
    {
		d_gapDetected = 0;
		d_resubscribed = 0;
		d_sequenceNumber = 0;
	}

	void setSession(Session &session)
	{
		d_session = &session;
	}

	/*------------------------------------------------------------------------------------
	 * Name			: showTicks
	 * Description	: show tick data flag
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	void showTicks(int tick)
	{
		syncio.lock();
		d_showTicks = tick;
		syncio.unlock();
	}

	/*------------------------------------------------------------------------------------
	 * Name			: processEvent
	 * Description	: process events
	 * Arguments	: none
	 * Returns		: true - successful, false - failed
	 *------------------------------------------------------------------------------------*/
    bool processEvent(const Event &event, Session *session)
    {
		bool flag = true;
        try {
            switch (event.eventType())
            {                
            case Event::SUBSCRIPTION_DATA:
                processSubscriptionDataEvent(event, session);
                break;
            case Event::SUBSCRIPTION_STATUS:
                processSubscriptionStatus(event);
                break;
            default:
                processMiscEvents(event);
                break;
            }
        } catch (Exception &e) {
			syncio.lock();
			std::cout << "Library Exception !!! " << e.description().c_str() << std::endl;
			syncio.unlock();
			flag = false;
        }
		return flag;
	}
};

class MarketDepthSubscriptionSnapshotExample
{
	std::vector<std::string>	 d_hosts;			// IP Addresses of the Managed B-Pipes
    int							 d_port;
	std::string					 d_authOption;		// authentication option user/application
	std::string					 d_name;	        // DirectoryService/ApplicationName
    SessionOptions               d_sessionOptions;
    Session                     *d_session;
    SubscriptionEventHandler    *d_eventHandler;
    std::string				     d_security;
	std::vector<std::string>     d_options;
    SubscriptionList             d_subscriptions; 
	ByOrderBook				     d_orderBooks[bookSize];
	ByLevelBook				     d_levelBooks[bookSize];
	int							 d_marketDepthBook;
	int							 d_pricePrecision;
	int							 d_showTicks;


	/*------------------------------------------------------------------------------------
	 * Name			: createSession
	 * Description	: The create session with session option provided
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	void createSession() { 
		std::string authOptions;

        SessionOptions sessionOptions;
		if (!std::strcmp(d_authOption.c_str(),"APPLICATION")) { //  Authenticate application
			// Set Application Authentication Option
			authOptions = "AuthenticationMode=APPLICATION_ONLY;";
			authOptions+= "ApplicationAuthenticationType=APPNAME_AND_KEY;";
			// ApplicationName is the entry in EMRS.
			authOptions+= "ApplicationName=" + d_name;
		} else {
			// Set User authentication option
			if (!strcmp(d_authOption.c_str(), "LOGON")) {   			
				// Authenticate user using windows/unix login name
				authOptions = "AuthenticationType=OS_LOGON";
			} else if (!strcmp(d_authOption.c_str(), "DIRSVC")) {		
				// Authenticate user using active directory service property
				authOptions = "AuthenticationType=DIRECTORY_SERVICE;";
				authOptions += "DirSvcPropertyName=" + d_name;
			} else {
				// default to no auth
				d_authOption = "NONE";
			}
		}

		syncio.lock();
		std::cout << "Authentication Options = " << authOptions << std::endl;
		syncio.unlock();

		// Add the authorization options to the sessionOptions
		if (d_authOption != "NONE")
		{
			sessionOptions.setAuthenticationOptions(authOptions.c_str());
		}
        sessionOptions.setAutoRestartOnDisconnection(true);
        sessionOptions.setNumStartAttempts(d_hosts.size());

        for (size_t i = 0; i < d_hosts.size(); ++i) { // override default 'localhost:8194'
            sessionOptions.setServerAddress(d_hosts[i].c_str(), d_port, i);
        }

		// set host and port
		syncio.lock();
        std::cout << "Connecting to port " << d_port << " on ";
        for (size_t i = 0; i < sessionOptions.numServerAddresses(); ++i) {
            unsigned short port;
            const char *host;
            sessionOptions.getServerAddress(&host, &port, i);
            std::cout << (i? ", ": "") << host;
        }
        std::cout << std::endl;
		syncio.unlock();

		// create event handler
		d_eventHandler = new SubscriptionEventHandler(d_orderBooks, d_levelBooks, 
			d_marketDepthBook, d_showTicks, d_subscriptions);
		// create session
		d_session = new Session(sessionOptions, d_eventHandler);
		// pass session to event handler
		d_eventHandler->setSession(*d_session);
		// start sesson
        bool sessionStarted = d_session->start();
        if (!sessionStarted) {
			syncio.lock();
            std::cerr << "Failed to start session. Exiting..." << std::endl;
			syncio.unlock();
            std::exit(-1);
        }
    }   

	/*------------------------------------------------------------------------------------
	 * Name			: parseCommandLine
	 * Description	: process command line parameters
	 * Arguments	: none
	 * Returns		: true - successful, false - failed
	 *------------------------------------------------------------------------------------*/
    bool parseCommandLine(int argc, char **argv)
    {
		std::string subscriptionOptions = "";
		std::string tmpOption;
		std::string tmpSecurity;

        for (int i = 1; i < argc; ++i) {
            if (!std::strcmp(argv[i],"-s") && i + 1 < argc) {
                d_security = argv[++i];
            } else if (!std::strcmp(argv[i],"-o") && i + 1 < argc) {
                d_options.push_back(argv[++i]);
			} else if (!std::strcmp(argv[i],"-pr") &&  i + 1 < argc) {
                d_pricePrecision = std::atoi(argv[++i]);
			} else if (!std::strcmp(argv[i],"-st") &&  i < argc) {
				d_showTicks = 1;
            } else if (!std::strcmp(argv[i],"-ip") && i + 1 < argc) {
                d_hosts.push_back(argv[++i]);
			} else if (!std::strcmp(argv[i],"-p") &&  i + 1 < argc) {
                d_port = std::atoi(argv[++i]);
			} else if (!std::strcmp(argv[i],"-auth") &&  i + 1 < argc) {
				d_authOption = argv[++i];
			} else if (!std::strcmp(argv[i],"-n") &&  i + 1 < argc) {
				d_name = argv[++i];
			} else {
				printUsage();
				return false;
			}
        }

		syncio.lock();
		// check for appliation name
		if ((!std::strcmp(d_authOption.c_str(),"APPLICATION")) && (!std::strcmp(d_name.c_str(), ""))){
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
		if (d_hosts.size() == 0)
		{
			std::cout << "Missing host IP address." << std::endl;
			printUsage();
            return false;
		}

        if (d_security.length() == 0) {
            d_security = mktDepthServiceName + "/ticker/VOD LN Equity";
        }

		if (d_options.size() == 0) {
			// by order
			d_options.push_back("type=MBO");
		}

		for (size_t j = 0; j < d_options.size(); ++j) {
			tmpOption = d_options[j];
			if (subscriptionOptions.length() == 0)
			{
				subscriptionOptions = "?" + tmpOption;
			}
			else
			{
				subscriptionOptions = subscriptionOptions + "&" + tmpOption;
			}
		}

		// default to unknow book type
		d_marketDepthBook = UNKNOWN;

		// add market depth service to security
		int index = (int)d_security.find("/");
		if (index != 0)
		{
			d_security = "/" + d_security;
		}
		index = (int)d_security.find("//");
		if (index != 0)
		{
			d_security = mktDepthServiceName + d_security;
		}
        // add subscription to subscription list
		tmpSecurity = d_security + subscriptionOptions;
        d_subscriptions.add(tmpSecurity.c_str(), CorrelationId(&d_security));
		std::cout << "Subscription string: " << d_subscriptions.topicStringAt(0) << std::endl;
		syncio.unlock();

        return true;
    }

	/*------------------------------------------------------------------------------------
	 * Name			: authorize
	 * Description	: authorize user/application
	 * Arguments	: identity of user/app authorized
	 * Returns		: true - successful, false - failed
	 *------------------------------------------------------------------------------------*/
    bool authorize(Identity * identity)
    {
        EventQueue tokenEventQueue;
		// generate token
        d_session->generateToken(CorrelationId(), &tokenEventQueue);
        std::string token;
		// get token request event
        Event event = tokenEventQueue.nextEvent();
        if (event.eventType() == Event::TOKEN_STATUS) {
            MessageIterator iter(event);
            while (iter.next()) {
                Message msg = iter.message();
                if (msg.messageType() == TOKEN_SUCCESS) {
                    token = msg.getElementAsString(TOKEN);
                }
                else if (msg.messageType() == TOKEN_FAILURE) {
		            syncio.lock();
					msg.print(std::cout);
					syncio.unlock();
                    break;
                }
            }
        }
        if (token.length() == 0) {
			syncio.lock();
            std::cout << "Failed to get token" << std::endl;
			syncio.unlock();
            return false;
		} else {
			syncio.lock();
			std::cout << "Token: " << token << std::endl;
			syncio.unlock();
		}

		// open authorization service
		if (!d_session->openService(authServiceName)) {
			syncio.lock();
			cerr <<"Failed to open " << authServiceName << endl;
			syncio.unlock();
			return false;
		}

		// get authorization service and create auth request
        Service authService = d_session->getService(authServiceName);
        Request authRequest = authService.createAuthorizationRequest();
        authRequest.set(TOKEN, token.c_str());

		// send authorization request
        EventQueue authQueue;
        d_session->sendAuthorizationRequest(
            authRequest, identity, CorrelationId(), &authQueue);

        while (true) {
			// process authorization event
            Event event = authQueue.nextEvent();
            if (event.eventType() == Event::RESPONSE ||
                event.eventType() == Event::REQUEST_STATUS ||
                event.eventType() == Event::PARTIAL_RESPONSE) {
                    MessageIterator msgIter(event);
                    while (msgIter.next()) {
                        Message msg = msgIter.message();
                        if (msg.messageType() == AUTHORIZATION_SUCCESS) {
							// success
							syncio.lock();
							std::cout << "Authorization success: seat type is " << identity->getSeatType() << std::endl;
							syncio.unlock();
                            return true;
                        } else if (msg.messageType() == AUTHORIZATION_FAILURE) {
							// authorization failed
							syncio.lock();
                            std::cout << "Authorization failed" << std::endl;
	                        msg.print(std::cout);
							syncio.unlock();
                            return false;
						} else {
							syncio.lock();
							msg.print(std::cout);
							syncio.unlock();
						}
                    }
            }
        }
    }

	/*------------------------------------------------------------------------------------
	 * Name			: printUsage
	 * Description	: prints the usage of the program on command line
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    void printUsage()
    {
		syncio.lock();
		cout << "Usage:" << std::endl
            << "    Retrieve realtime market depth data using Bloomberg V3 API" << std::endl
			<< std::endl
            << "      [-s    <security   = ""/ticker/VOD LN Equity"">" << std::endl
			<< "      [-o    <type=MBO, type=MBL, type=TOP or type=MMQ>" << std::endl
			<< "      [-pr   <precision  = 4>" << std::endl
			<< "      [-st   <show ticks>" << std::endl
            << "      [-ip   <ipAddress  = localhost>" << std::endl
            << "      [-p    <tcpPort    = 8194>" << std::endl
			<< "      [-auth      <authenticationOption = NONE or LOGON or APPLICATION or DIRSVC>]" << std::endl
			<< "      [-n         <name = applicationName or directoryService>]" << std::endl
			<< "Notes:" << std::endl
			<< " -Specify only LOGON to authorize 'user' using Windows/unix login name." << std::endl
			<< " -Specify DIRSVC and name(Directory Service Property) to authorize user using directory Service." << std::endl
			<< " -Specify APPLICATION and name(Application Name) to authorize application." << std::endl;
		syncio.unlock();
    }

	/*------------------------------------------------------------------------------------
	 * Name			: printMenu
	 * Description	: print usage menu
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
	void printMenu()
	{
		syncio.lock();
		cout << "-------------------------------------------------------------" << endl;
		cout << " Enter 'v' or 'V' to show the current market depth cache book" << endl;
		cout << " Enter 't' or 'T' to toggle show ticks on/off" << endl;
		cout << " Enter 'q' or 'Q' to quit" << endl;
		cout << "-------------------------------------------------------------" << endl;
		syncio.unlock();
	}

	/*----------------------------------------------------------------
	 * Name			: ShowByOrderBook
	 * Description	: dumps the current order book to the console
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void ShowByOrderBook()
	{
		register unsigned int i;
		unsigned int uSize, auSize[2];

	    syncio.lock();
		ByOrderBook *book = d_orderBooks;

		// get BID/ASK size
		auSize[BIDSIDE] = book[BIDSIDE].size();
		auSize[ASKSIDE] = book[ASKSIDE].size();
		uSize = auSize[BIDSIDE] > auSize[ASKSIDE] ? auSize[BIDSIDE] : auSize[ASKSIDE];

	    int offset = 0;
	    if (d_pricePrecision < 4)
			offset = 0;
	    else
		    offset = d_pricePrecision - 4;

		cout << "-------------------------------------------------------------------------------------------------" << endl;
		cout << "MAXIMUM WINDOW SIZE: " << book->window_size << endl
			 << "BOOK TYPE          : " << book->book_type << endl;
		cout << "-------------------------------------------------------------------------------------------------" << endl;
		cout << "                 --- BID ---                                     --- ASK ---" << endl
			<< " POS  BROKER  PRICE" << setw(offset + 4) << "" << "SIZE      TIME         ---     BROKER  PRICE" << 
			setw(offset + 4) << "" << "SIZE      TIME   " << endl;

		for (i=0; i<uSize; ++i)
		{
			stringstream ss;
			ss.precision(d_pricePrecision);
			ss.setf(ios::fixed,ios::floatfield);

			// format book for bid side
			ByOrderBookEntry entry;
			if (book[BIDSIDE].getEntry(i, entry)) 
			{
				ss <<  setw(7) << entry.broker_.c_str()  <<  " "
				   << setw(8) << entry.price_  << " "
				   << setw(6) << entry.size_ <<  " "
				   << setw(13) << entry.time_;
			}
			else
				ss << setw(37 + offset) << "";

			// format book or ask side
			if (book[ASKSIDE].getEntry(i, entry))
			{
				ss << "     ---   "
					<<  setw(7) << entry.broker_.c_str()  <<  " "
					<< setw(8) << entry.price_  << " "
					<< setw(6) << entry.size_ <<  " "
					<< setw(13) << entry.time_;
			}
			// display row
			cout << setw(3) << i + 1 << " " << ss.str().c_str() << endl;
	    }
		syncio.unlock();
	}

	/*----------------------------------------------------------------
	 * Name			: ShowByLevelBook
	 * Description	: dumps the current order book to the console
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void ShowByLevelBook()
	{
		register unsigned int i;
		unsigned int uSize, auSize[2];

	    syncio.lock();
		ByLevelBook *book = d_levelBooks;

		// get BID/ASK size
	    auSize[BIDSIDE] = book[BIDSIDE].size();
	    auSize[ASKSIDE] = book[ASKSIDE].size();
	    uSize = auSize[BIDSIDE] > auSize[ASKSIDE] ? auSize[BIDSIDE] : auSize[ASKSIDE];
	    
		int offset = 0;
	    if (d_pricePrecision < 4)
			offset = 0;
	    else
		    offset = d_pricePrecision - 4;
	   
		cout << "------------------------------------------------------------------------" << endl;
	    cout << "MAXIMUM WINDOW SIZE: " << book->window_size << endl
			 << "BOOK TYPE          : " << book->book_type << endl;
	    cout << "------------------------------------------------------------------------" << endl;
	    cout << "                --- BID ---                             --- ASK ---\n"
			 << " POS   PRICE" << setw(offset + 3) << "" << "SIZE    #-ORD      TIME     ---    PRICE" << setw(offset + 3) << "" << "SIZE    #-ORD      TIME" << endl;

	    for (i=0; i<uSize; ++i)
	    {
		    stringstream ss;
		    ss.precision(d_pricePrecision);
		    ss.setf(ios::fixed,ios::floatfield);
		    // format book for bid side
		    ByLevelBookEntry entry;
		    if (book[BIDSIDE].getEntry(i, entry)) 
		    {
				ss << setw(8) << entry.price_  << " "
				   << setw(6) << entry.size_ <<  " "
				   << setw(6) << entry.numOrders_ << " " 
				   << setw(13) << entry.time_;
			 }
			 else
				ss << setw(36 + offset) << "";

			 // format book or ask side
			 if (book[ASKSIDE].getEntry(i, entry))
			 {
			 	ss << "   --- "
					<< setw(8) << entry.price_  << " "
					<< setw(6) << entry.size_ <<  " "
					<< setw(6) << entry.numOrders_ << " " 
					<< setw(13) << entry.time_;
			 }
			 // display row
			 cout << setw(3) << i + 1 << " " << ss.str().c_str() << endl;
		}
		syncio.unlock();
	}

public:

	/*------------------------------------------------------------------------------------
	 * Name			: MarketDepthSubscriptionSnapshotExample
	 * Description	: constructor
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    MarketDepthSubscriptionSnapshotExample()
    : d_session(0)
    , d_eventHandler(0)
	, d_pricePrecision(4)
	, d_port(8194)
	, d_showTicks(0)
    {
    }

	/*------------------------------------------------------------------------------------
	 * Name			: MarketDepthSubscriptionSnapshotExample
	 * Description	: destructor
	 * Arguments	: none
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    ~MarketDepthSubscriptionSnapshotExample()
    {
        if (d_session) delete d_session;
        if (d_eventHandler) delete d_eventHandler ;
    }

	/*------------------------------------------------------------------------------------
	 * Name			: run 
	 * Description	: start application process
	 * Arguments	: argc is number arguments
	 *              : argv are the argument values
	 * Returns		: none
	 *------------------------------------------------------------------------------------*/
    void run(int argc, char **argv)
    {
		// process command line
        if (!parseCommandLine(argc, argv)) return;
        
		// create session 
		createSession();

		// check if authentication was used
		if (d_authOption == "NONE")
		{
			syncio.lock();
			std::cout << "Subscribing without Identity..." << std::endl;
			syncio.unlock();
			// subscribe to market depth
			d_session->subscribe(d_subscriptions);
		}
		else
		{
			// Authorize all the users that are interested in receiving data
			Identity identity = d_session->createIdentity();
			if (authorize(&identity)) {
				// subscribe
				syncio.lock();
				std::cout << "Subscribing with Identity..." << std::endl;
				syncio.unlock();
				// subscribe to market depth with identity
				d_session->subscribe(d_subscriptions, identity);
			}
		}

	   //wait for user input
		while(true)
		{
			printMenu();
			char c;
			std::cin >> c;
			if (c)
			{
				if ((c == 'v') || (c == 'V'))
				{
					// view market depth book
					switch (d_marketDepthBook)
					{
						case BYLEVEL:
							ShowByLevelBook();
							break;
						case BYORDER:
							ShowByOrderBook();
							break;
						default:
							syncio.lock();
							std::cout << "Unknown book type" << std::endl;
							syncio.unlock();
							break;
					}
				}
				else if ( (c == 't') || (c == 'T') )
				{
					// show ticks
					d_showTicks = (d_showTicks == 0) ? 1 : 0;
					d_eventHandler->showTicks(d_showTicks);
				}
				else if ( (c == 'q') || (c == 'Q') )
				{
					// quite
					break;
				}
				else
				{
					// unknow command
					syncio.lock();
					std::cout << "Unknown command: '" << c << "'" << std::endl;
					syncio.unlock();
				}
			}
		}

		// unsubscribe 
		d_session->unsubscribe(d_subscriptions);
		// stop session
        d_session->stop();
		syncio.lock();
		std::cout << "Exiting..." << std::endl;
		syncio.unlock();
    }
};

/*------------------------------------------------------------------------------------
 * Name			: main
 * Description	: main function
 * Arguments	: argc is number arguments
 *              : argv are the argument values
 * Returns		: none
 *------------------------------------------------------------------------------------*/
int main(int argc, char **argv)
{
    setvbuf(stdout, NULL, _IOFBF, BUFSIZ);
	syncio.lock();
	std::cout << "MarketDepthSubscriptionSnapshotExample" << std::endl;
	syncio.unlock();
    MarketDepthSubscriptionSnapshotExample example;
    try {
        example.run(argc, argv);
    } catch (Exception &e) {
		syncio.lock();
		std::cout << "Library Exception!!! " << e.description().c_str() << std::endl;
		syncio.unlock();
    }
    return 0;
}
