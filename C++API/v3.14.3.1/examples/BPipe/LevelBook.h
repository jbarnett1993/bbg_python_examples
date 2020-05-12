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

/* --------------------------------------------------------------------
 * File: LevelBook.h
 *
 * Description: This file contains the code for handling marketdepth by level
 *				messages.
 *
 * Version	  : XX.XX.XX
 *
 *   NOTICE:
 *   Copyright (C) Bloomberg L.P., 2007
 *   All Rights Reserved.
 *   Property of Bloomberg L.P. (BLP)
 *   This software is made available solely pursuant to the
 *   terms of a BLP license agreement which governs its use.
 * ----------------------------------------------------------------- */

#ifndef __LevelBook_h__
#define __LevelBook_h__

#include <time.h>    // time_t

#include <deque>
#include <iostream>
#include <string>
#include <sstream>

#include "SyncIO.h"

using namespace std;

namespace BloombergLP
{

	/* --------------------------------------------------------------------
	 * Class/Struct : ByLevelBookEntry
	 * Description  : Defines constructors and fields contained in an orderbook
	 * --------------------------------------------------------------------*/
  struct ByLevelBookEntry
  {
	/*----------------------------------------------------------------
	 * Name			: ByLevelBookEntry default constructor
	 * Description	: Constructs a bylevel book entry with 0 price, size, orders
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	ByLevelBookEntry();

	/*----------------------------------------------------------------
	 * Name			: ByLevelBookEntry constructor
	 * Description	: Constructs a bylevel book entry instance 
	 * Arguments	: price is the price of the tick
	 *				  t is the tick time
	 *				  numOrders is the number of orders
	 *				  size is the size of bid/ask
	 * Returns		: none
	 *---------------------------------------------------------------*/
	ByLevelBookEntry(float price, std::string t, unsigned int numOders, unsigned int size);

	/*----------------------------------------------------------------
	 * Name			: ByLevelBookEntry copy constructor
	 * Description	: Copy constructor for ByLevelBookEntry
	 * Arguments	: cpy is a reference to the ByLevelBookEntry to be copied
	 * Returns		: none
	 *---------------------------------------------------------------*/
    ByLevelBookEntry(const ByLevelBookEntry& cpy);


	/*----------------------------------------------------------------
	 * Name			: ByLevelBookEntry assignment operator
	 * Description	: Assignment operator for ByLevelBookEntry
	 * Arguments	: rhs is a reference to a ByLevelBookEntry
	 * Returns		: a reference to the current object
	 *---------------------------------------------------------------*/
    ByLevelBookEntry& operator= (const ByLevelBookEntry& rhs);
    bool isValid() const;
   
	//price of the bid/ask
    double price_; 

	//tick time
	std::string time_;

	//number of orders
    unsigned int numOrders_;

	//order size
    unsigned int size_;

	//flag to indicate whether a book is valid
    bool isValid_;
  };


  /*----------------------------------------------------------------
   * Class		 : ByLevelBook
   * Description   : Class that contains the methods for maintaining
   *				   the order book
   *---------------------------------------------------------------*/
  class ByLevelBook
  {
  public:
	/*----------------------------------------------------------------
	 * Name			: ByLevelBook constructor
	 * Description	: Default constructor. Window Size is set to 0
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	ByLevelBook();

    enum BookType {AskPrice = 0, BidPrice = 1};

	//maximum window size
	unsigned int window_size;
	//this is to indicate whether we have received an
	//ack message
	bool valid;
	
	std::string book_type;

	/*----------------------------------------------------------------
	 * Name			: doAdd
	 * Description	: Add an entry to the order book.
	 *				  When you add this order in the market depth table, 
	 *				  you should shift all orders at the market depth position
	 *				  in the event and market depth orders or levels inferior to 
	 *				  event passed to one position inferior.
	 *				  For example, if a new order is added to position one of the 
	 *				  market depth table, then the previous order at position one is 
	 *				  shifted to position two. The order at position two is shifted to 
	 *				  position three and so on until you get to the market depth window size.
	 * Arguments	: pos is the position to add the entry
	 *				  entry is the new entry to add
	 * Returns		: none
	 *---------------------------------------------------------------*/
    void doAdd(unsigned int pos, const ByLevelBookEntry& entry);

	/*----------------------------------------------------------------
	 * Name			: doClearAll()
	 * Description	: Clears all the orderbook for the specified side
	 *				  This market depth table command is issued by Bloomberg
	 *				  when market depth recovery is occuring. This table command
	 *				  has the same effect on the cache as DELETEALL which 
	 *				  means all order or levels should be cleared from the cache.
	 *				  During LVC recovery you will generally see 2 CLEARALLs - 1 for Bid 
	 *				  side and 1 for Ask side. Should the client of market depth 
	 *				  need to process a recovery of market depth differently, this 
	 *				  table command allows the user to differentiate from the 
	 *				  source/exchange produced DELETEALL. 
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void doClearAll();

	/*----------------------------------------------------------------
	 * Name			: doDel()
	 * Description	: Delete this event from the market depth cache. 
	 *				  The delete should occur at the position passed in the 
	 *				  market depth event. When cached market event at the 
	 *				  position passed in the delete is removed, all position 
	 *				  inferior should have their positions shifted by one. 
	 *				  For example, if position one is deleted from a market 
	 *				  by order or market by price event, the position two 
	 *				  becomes one, position three becomes two, etc. 
	 * Arguments	: pos is the position to be deleted
	 * Returns		: none
	 *---------------------------------------------------------------*/
    void doDel(unsigned int pos);

	/*----------------------------------------------------------------
	 * Name			: doDelAll()
	 * Description	: Delete all events from the cache. This is a market depth
	 *				  flush usually passed at the start or end of trading or when
	 *				  a trading halt occurs.
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void doDelAll();

	/*----------------------------------------------------------------
	 * Name			: doDelBetter()
	 * Description	: Delete this order and any superior orders. The order id at 
	 *				  pos - 1 is now the best order. This differs from the EXEC
	 *				  command in that it delets the current order, where the 
	 *				  EXEC command modifies the current order.
	 * Arguments	: pos is the position to be deleted
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void doDelBetter(unsigned int pos);

	/*----------------------------------------------------------------
	 * Name			: doDelSide()
	 * Description	: Delete all events on the corresponding side of the depth cache.
	 * Arguments	: none
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void doDelSide();

	/*----------------------------------------------------------------
	 * Name			: doExec
	 * Description	: Trade Execution. Find the corresponding order in the cache
	 *				  replace the entry with the new entry and delete orders with
	 *				  greater priority
	 * Arguments	: pos is the position to be modified
	 *				  entry is the new book entry
	 * Returns		: none
	 *---------------------------------------------------------------*/
	void doExec(unsigned int pos, const ByLevelBookEntry& entry); 

	/*----------------------------------------------------------------
	 * Name			: doMod
	 * Description	: Modify an existing event in the market depth cache. 
	 *				  Find the cached market depth entry by the position in 
	 *				  new the market depth cache and replace the cached event
	 *				  by the fields and data in the new event.
	 * Arguments	: pos is the position to be modified
	 *				  entry is the new book entry
	 * Returns		: none
	 *---------------------------------------------------------------*/
    void doMod(unsigned int pos, const ByLevelBookEntry& entry);

	/*----------------------------------------------------------------
	 * Name			: doReplace
	 * Description	: Replace previous price level or order at this position.
	 *				  Add price level or order if you do not have it currently in
	 *				  the cache. A 0 price and size will be sent when there is
	 *				  no active price or order at this level.
	  * Arguments	: pos is the position to be modified
	 *				  entry is the new book entry
	 * Returns		: none
	 *---------------------------------------------------------------*/
    void doReplace(unsigned int pos, const ByLevelBookEntry& entry);

	/*----------------------------------------------------------------
	 * Name			: doReplaceClear
	 * Description	: The REPLACE_CLEAR table command is intended to remove an order or
	 *				  more often a level in the market depth cache. The REPLACE_CLEAR
	 *				  should be indexed by the MarketDepth.ByLevel/ByOrder.Bid/Ask.Position
	 *				  field. The cache should NOT be shifted up after the level is
	 *				  cleared. A clear means all orders at that position have been deleted
	 *				  from the orderbook. It is possible that a order or level at a
	 *				  superior or most superior position to be cleared prior to more
	 *				  inferior levels. After the level is cleared in this case, it is
	 *				  expected that subsequent market depth event(s) will be passed
	 *				  to clear the orders or levels at positions inferior to the one just
	 *				  cleared.
	 * Arguments	: pos is the position to be modified
	 * Returns		: none
	 *---------------------------------------------------------------*/
    void doReplaceClear(unsigned int pos);

	//----helper functions---
	/*----------------------------------------------------------------
	 * Name			: size
	 * Description	: Returns the curent size of the order book
	 * Arguments	: none
	 * Returns		: size of the order book as unsigned int
	 *---------------------------------------------------------------*/
    unsigned int size();
	/*----------------------------------------------------------------
	 * Name			: getEtnry
	 * Description	: Returns the entry at the specified position
	 * Arguments	: pos is the position of the entry
	 *				  entry is a reference to where the entry should be stored
	 * Returns		: true is the cache is valid
	 *				  false if the cache is invalid
	 *---------------------------------------------------------------*/
    bool getEntry(unsigned int pos, ByLevelBookEntry& entry);

	//maximum number of decimal places for output
	unsigned int max_num_decimals;


  protected:
    std::deque<ByLevelBookEntry> entries_;
	SyncIO lock_cache_;
	
  };
}

#endif
