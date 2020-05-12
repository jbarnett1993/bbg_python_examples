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
 * File: OrderBook.h
 *
 * Description: This file contains the code for handling marketdepth by order
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
#include "OrderBook.h"

/* --------------------------------------------------------------------
 * Class/Struct : ByOrderBookEntry
 * Description  : Defines constructors and fields contained in an orderbook
 * --------------------------------------------------------------------*/

/*----------------------------------------------------------------
 * Name			: ByOrderBookEntry default constructor
 * Description	: Constructs a byorder book entry with 0 price, size, orders
 * Arguments	: none
 * Returns		: none
 *---------------------------------------------------------------*/
BloombergLP::ByOrderBookEntry::ByOrderBookEntry() :
 broker_(""), price_(0.0f), time_(""), numOrders_(0), size_(0), isValid_(false)
{}

/*----------------------------------------------------------------
 * Name			: ByOrderBookEntry constructor
 * Description	: Constructs a byorder book entry instance 
 * Arguments	: broker is the name of the broker in string
 *				  price is the price of the tick
 *				  t is the tick time
 *				  numOrders is the number of orders
 *				  size is the size of bid/ask
 * Returns		: none
 *---------------------------------------------------------------*/
 BloombergLP::ByOrderBookEntry::ByOrderBookEntry(std::string broker, float price, std::string t, unsigned int numOders, unsigned int size) :
broker_(broker), price_(price), time_(t), numOrders_(numOders), size_(size), isValid_(true)
{}

/*----------------------------------------------------------------
 * Name			: ByOrderBookEntry copy constructor
 * Description	: Copy constructor for ByOrderBookEntry
 * Arguments	: cpy is a reference to the ByOrderBookEntry to be copied
 * Returns		: none
 *---------------------------------------------------------------*/
BloombergLP::ByOrderBookEntry::ByOrderBookEntry(const ByOrderBookEntry& cpy)
{
  broker_ = cpy.broker_;
  price_ = cpy.price_;
  time_ = cpy.time_;
  numOrders_ = cpy.numOrders_;
  size_ = cpy.size_;
  isValid_ = cpy.isValid_;
}

/*----------------------------------------------------------------
 * Name			: ByOrderBookEntry assignment operator
 * Description	: Assignment operator for ByOrderBookEntry
 * Arguments	: rhs is a reference to a ByOrderBookEntry
 * Returns		: a reference to the current object
 *---------------------------------------------------------------*/
BloombergLP::ByOrderBookEntry& BloombergLP::ByOrderBookEntry::operator= (const ByOrderBookEntry& rhs)
{
  if (this == &rhs)
  {
    return *this;
  }

  broker_ = rhs.broker_;
  price_ = rhs.price_;
  time_ = rhs.time_;
  numOrders_ = rhs.numOrders_;
  size_ = rhs.size_;
  isValid_ = rhs.isValid_;

  return *this;
}

/*----------------------------------------------------------------
 * Name			: isValid
 * Description	: Checks whether the entry is valid
 * Arguments	: none
 * Returns		: true if the order  book is valid
 *				  false if the order book is not valid
 *---------------------------------------------------------------*/
bool BloombergLP::ByOrderBookEntry::isValid() const
{
  return isValid_;
}


/*----------------------------------------------------------------
 * Class		 : ByOrderBook
 * Description   : Class that contains the methods for maintaining
 *				   the order book
 *---------------------------------------------------------------*/

/*----------------------------------------------------------------
 * Name			: ByOrderBook constructor
 * Description	: Default constructor. Window Size is set to 0
 * Arguments	: none
 * Returns		: none
 *---------------------------------------------------------------*/
BloombergLP::ByOrderBook::ByOrderBook()
	: window_size(0)
	, valid(false)
	, book_type("")
	, max_num_decimals(3)
{
}


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
void BloombergLP::ByOrderBook::doAdd( unsigned int pos, const ByOrderBookEntry& entry )
{
	Guard guard(lock_cache_);
	if (entries_.size() <= pos)
	{
		entries_.resize(pos+1);
	}

	entries_.insert(entries_.begin()+pos, entry);

	//remove entries > window size
	if(window_size < entries_.size())
	{
		entries_.erase(entries_.begin() + window_size, entries_.end());
	}
}

/*----------------------------------------------------------------
 * Name			: doClearAll()
 * Description	: Clears all the orderbook for the specified side
 *				  This market depth table command is issued by Bloomberg
 *				  when market depth recovery is occuring. This table command
 *				  has the same effect on the cache as DELETEALL which 
 *				  means all order or levels should be cleared from the cache.
 * Arguments	: none
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doClearAll()
{
	Guard guard(lock_cache_);
	entries_.clear();
}

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
void BloombergLP::ByOrderBook::doDel( unsigned int pos )
{
	Guard guard(lock_cache_);
	if (entries_.size() <= pos)
	{
		return;
	}

	entries_.erase(entries_.begin()+pos);
}

/*----------------------------------------------------------------
 * Name			: doDelAll()
 * Description	: Delete all events from the cache. This is a market depth
 *				  flush usually passed at the start or end of trading or when
 *				  a trading halt occurs.
 * Arguments	: none
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doDelAll()
{
	Guard guard(lock_cache_);
	entries_.clear();
}

/*----------------------------------------------------------------
 * Name			: doDelBetter()
 * Description	: Delete this order and any superior orders. The order id at 
 *				  pos + 1 is now the best order. This differs from the EXEC
 *				  command in that it delets the current order, where the 
 *				  EXEC command modifies the current order.
 * Arguments	: pos is the position to be deleted
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doDelBetter(unsigned int pos)
{
	Guard guard(lock_cache_);
	entries_.erase(entries_.begin(), entries_.begin() + pos);
}

/*----------------------------------------------------------------
 * Name			: doDelSide()
 * Description	: Delete all events on the corresponding side of the depth cache.
 * Arguments	: none
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doDelSide()
{
	Guard guard(lock_cache_);
	entries_.clear();
}


/*----------------------------------------------------------------
 * Name			: doExec
 * Description	: Trade Execution. Find the corresponding order in the cache
 *				  replace the entry with the new entry and delete orders with
 *				  greater priority
 * Arguments	: pos is the position to be modified
 *				  entry is the new book entry
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doExec(unsigned int pos, const ByOrderBookEntry& entry)
{
	Guard guard(lock_cache_);
	entries_[pos] = entry;
	entries_.erase(entries_.begin(), entries_.begin() + pos);
}

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
void BloombergLP::ByOrderBook::doMod( unsigned int pos, const ByOrderBookEntry& entry )
{
	Guard guard(lock_cache_);
	entries_[pos] = entry;
}

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
void BloombergLP::ByOrderBook::doReplace( unsigned int pos, const ByOrderBookEntry& entry )
{
	Guard guard(lock_cache_);
	if(entries_.size() <= pos)
	{
		entries_.resize(pos + 1);
	}
	entries_[pos] = entry;
}

/*----------------------------------------------------------------
 * Name			: doReplaceByBroker
 * Description	: This table command is used for top of file feeds 
 *				  where the action is to replace by the broker mnemonic. 
 *				  The recipient needs to find the broker in their cache 
 *				  and replace the quote with the one in the market depth event. 
 *				  If that broker is not present, it should added to the cache. 
 *				  If the price and size for a broker is set to 0, 
 *				  the broker should be deleted from the cache. 
 * Arguments	: entry is the new book entry
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doReplaceByBroker(ByOrderBookEntry& entry )
{
	Guard guard(lock_cache_);
	if(entry.price_ == 0 && entry.size_ == 0)
	{
		doDeleteByBroker(entry.broker_);
		return;
	}

	std::deque<ByOrderBookEntry>::iterator it;
	for(it = entries_.begin(); it!= entries_.end(); ++it)
	{
		if(it->broker_ == entry.broker_)
		{
			break;
		}
	}
  
	if(it == entries_.end())
	{
		//add entry
		entries_.push_back(entry);
	}
	else
	{
		//modify entry
		*it = entry;
	}
}

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
void BloombergLP::ByOrderBook::doReplaceClear( unsigned int pos )
{
	Guard guard(lock_cache_);
	ByOrderBookEntry clearEntry;
	entries_[pos] = clearEntry;
}

//----helper functions---
/*----------------------------------------------------------------
 * Name			: doDeleteByBroker
 * Description	: Delete the entry for the specified broker
 * Arguments	: broker is the broker code in string
 * Returns		: none
 *---------------------------------------------------------------*/
void BloombergLP::ByOrderBook::doDeleteByBroker(std::string& broker)
{
	Guard guard(lock_cache_);
	if(broker.empty())
		return;
	
	std::deque<ByOrderBookEntry>::iterator it;
	for(it = entries_.begin(); it!= entries_.end(); ++it)
	{
		if(it->broker_ == broker)
		{
			break;
		}
	}

	if(it != entries_.end())
	{
		entries_.erase(it);
	}
}

/*----------------------------------------------------------------
 * Name			: size
 * Description	: Returns the curent size of the order book
 * Arguments	: none
 * Returns		: size of the order book as unsigned int
 *---------------------------------------------------------------*/
unsigned int BloombergLP::ByOrderBook::size()
{
	Guard guard(lock_cache_);
	return (unsigned int) entries_.size();
}

/*----------------------------------------------------------------
 * Name			: getEtnry
 * Description	: Returns the entry at the specified position
 * Arguments	: pos is the position of the entry
 *				  entry is a reference to where the entry should be stored
 * Returns		: true is the cache is valid
 *				  false if the cache is invalid
 *---------------------------------------------------------------*/
bool BloombergLP::ByOrderBook::getEntry( unsigned int pos, ByOrderBookEntry& entry )
{
	Guard guard(lock_cache_);
  if (pos < entries_.size())
  {
    entry = entries_[pos];
    return entry.isValid();
  }
  return false;
}


