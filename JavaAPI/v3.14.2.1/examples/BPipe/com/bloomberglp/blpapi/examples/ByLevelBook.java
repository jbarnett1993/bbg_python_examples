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
package com.bloomberglp.blpapi.examples;

import java.util.ArrayList;

public class ByLevelBook {
	private int d_windowSize;
	private String d_bookType;
	private boolean d_isValid;

	private ArrayList<BookEntry>  d_entries;

	public int getWindowSize()
	{
		return d_windowSize;
	}
	
	public void setWindowSize(int windowSize)
	{
		d_windowSize = windowSize;
	}
	
	public String getBookType()
	{
		return d_bookType;
	}
	
	public void setBookType(String bookType)
	{
		d_bookType = bookType;
	}
	
	public boolean getIsValid()
	{
		return d_isValid;
	}
	
	public void setIsValid(boolean isValid)
	{
		d_isValid = isValid;
	}

	public int getBookSize()
	{
		return d_entries.size();
	}

    // Name			: ByOrderBook default constructor
    // Description	: Default constructor. Window Size is set to 0
    public ByLevelBook()
    {
        
        setWindowSize(0);
        setIsValid(false);
        setBookType("");
        d_entries = new ArrayList<BookEntry>();
    }

    // Name			: doAdd
    // Description	: Add an entry to the order book.
	//		  		  When you add this order in the market depth table, 
    //		  		  you should shift all orders at the market depth position
    //		  		  in the event and market depth orders or levels inferior to 
    //		  		  event passed to one position inferior.
    //		  		  For example, if a new order is added to position one of the 
    //		  		  market depth table, then the previous order at position one is 
    //		  		  shifted to position two. The order at position two is shifted to 
    //       		  position three and so on until you get to the market depth window size.
    // Parameters :
    // 		pos = position to add the entry
    // 		entry = entry to add
    public void doAdd(int pos, BookEntry entry)
    {
        synchronized (d_entries) {
        	if (d_entries.size() < pos)
        	{
        		// gap detected
        		System.out.println("Gap detected in cache for level " + (pos + 1) + 
        				". Cache size is " + d_entries.size() + ".");
        		return;
        	}
            d_entries.add(pos, entry);
            if (getWindowSize() < d_entries.size())
            {
                //remove entries > window size
            	for (int i = d_entries.size() - 1; i >= getWindowSize(); --i)
            	{
            		d_entries.remove(i);
            	}
            }
        }
    }
 
    // Name			: doClearAll()
    // Description	: Clears all the orderbook for the specified side
    //                This market depth table command is issued by Bloomberg
    //                when market depth recovery is occuring. This table command
    //                has the same effect on the cache as DELETEALL which 
    //                means all order or levels should be cleared from the cache.
    public void doClearAll()
    {
    	synchronized (d_entries) {
            d_entries.clear();
        }
    }

    // Name			: doDel()
    // Description	: Delete this event from the market depth cache. 
    //     		  	  The delete should occur at the position passed in the 
    //     		  	  market depth event. When cached market event at the 
    //     		  	  position passed in the delete is removed, all position 
    //     		  	  inferior should have their positions shifted by one. 
    //     		  	  For example, if position one is deleted from a market 
    //     		  	  by order or market by price event, the position two 
    //     		  	  becomes one, position three becomes two, etc. 
    // Parameters 	:
    // 		pos = position to be deleted
    public void doDel(int pos)
    {
    	synchronized (d_entries) {
            if (d_entries.size() > pos)
            {
                d_entries.remove(pos);
            }
        }
    }
    
    // Name			: doDelAll()
    // Description	: Delete all events from the cache. This is a market depth
    //                flush usually passed at the start or end of trading or when
    //                a trading halt occurs.
    public void doDelAll()
    {
    	synchronized (d_entries)
        {
            d_entries.clear();
        }
    }

    // Name			: doDelBetter()
    // Description	: Delete this order and any superior orders. The order id at 
    //				  pos + 1 is now the best order. This differs from the EXEC
    //				  command in that it delets the current order, where the 
    //                EXEC command modifies the current order.
    // Parameters	:
    // 		pos = position to be deleted
    public void doDelBetter(int pos)
    {
    	synchronized (d_entries) {
    		for (int i = pos; i >= 0; --i) {
    			d_entries.remove(i);
    		}
        }
    }
    
    // Name			: doDelSide()
    // Description	: Delete all events on the corresponding side of the depth cache.
    public void doDelSide()
    {
    	synchronized (d_entries) {
            d_entries.clear();
        }
    }
    
    // Name			: doExec
    // Description	: Trade Execution. Find the corresponding order in the cache
    //                replace the entry with the new entry and delete orders with
    //                greater priority
    // Parameters 	:
    // 		pos = position to be modified
    // 		entry = new book entry
    public void doExec(int pos, BookEntry entry)
    {
    	synchronized (d_entries) {
            d_entries.set(pos, entry);
            if (pos != 0) {
            	for (int i = pos - 1; i >= 0; --i) {
            		d_entries.remove(i);
            	}
            }
        }
    }
    
    // Name		: doMod
    // Description	: Modify an existing event in the market depth cache. 
    //              Find the cached market depth entry by the position in 
    //               new the market depth cache and replace the cached event
    //               by the fields and data in the new event.
    // Parameter :
    // pos = position to be modified
    // entry = new book entry
    public void doMod(int pos, BookEntry entry)
    {
    	synchronized (d_entries) {
            d_entries.set(pos, entry);
        }
    }

    //  Name		: doReplace
    //  Description	: Replace previous price level or order at this position.
    //                Add price level or order if you do not have it currently in
    //                the cache. A 0 price and size will be sent when there is
    //                no active price or order at this level.
    // Parameters 	:
    // 		pos = position to be modified
    // 		entry = new book entry
    public void doReplace(int pos, BookEntry entry)
    {
    	synchronized (d_entries) {
            if (d_entries.size() <= pos)
            {
                // fill in entries
                for (int i = d_entries.size(); i <= pos; ++i)
                {
                    d_entries.add(new BookEntry());
                }
            }
    		d_entries.set(pos, entry);
        }
    }

    // Name			: doReplaceClear
    // Description	: The REPLACE_CLEAR table command is intended to remove an order or
    //                more often a level in the market depth cache. The REPLACE_CLEAR
    //                should be indexed by the MarketDepth.ByLevel/ByOrder.Bid/Ask.Position
    //                field. The cache should NOT be shifted up after the level is
    //                cleared. A clear means all orders at that position have been deleted
    //                from the orderbook. It is possible that a order or level at a
    //                superior or most superior position to be cleared prior to more
    //                inferior levels. After the level is cleared in this case, it is
    //                expected that subsequent market depth event(s) will be passed
    //                to clear the orders or levels at positions inferior to the one just
    //                cleared.
    // Parameters 	:
    // 		pos = position to be modified
    public void doReplaceClear(int pos)
    {
    	synchronized (d_entries) {
            if (d_entries.size() <= pos)
            {
                // fill in entries
                for (int i = d_entries.size(); i <= pos; ++i)
                {
                    d_entries.add(new BookEntry());
                }
            }
            else
            {
	            BookEntry entry = new BookEntry();
	            d_entries.set(pos, entry);
            }
        }
    }

    // Name			: getEtnry
    // Description	: Returns the entry at the specified position
    // Parameters 	:
    // 		pos = position of the entry
    // Return 		: true is the cache is valid, false if the cache is invalid
    public BookEntry getEntry(int pos)
    {
        BookEntry entry = null;
        synchronized (d_entries)
        {
            if (pos >= d_entries.size()) {
                entry = null;
            } else {
                entry = d_entries.get(pos);
                if (!entry.getIsValid())
                {
                    // invalid entry
                    entry = null;
                }
            }
        }
        return entry;
    }

}
