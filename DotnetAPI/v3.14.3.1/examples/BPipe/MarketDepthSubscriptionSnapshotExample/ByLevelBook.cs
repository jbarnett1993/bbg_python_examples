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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bloomberglp.Blpapi.Examples
{
    /// <summary>
    /// Class		 : ByLevelBook
    /// Description  : Class that contains the methods for maintaining
    ///                the order book
    /// </summary>
    class ByLevelBook
    {
        private int d_windowSize;
        private bool d_isValid;
        private string d_bookType;

        private List<BookEntry> d_entries;

        #region "Properties"
        public int WindowSize
        {
            get { return d_windowSize; }
            set { d_windowSize = value; }
        }

        public bool IsValid
        {
            get { return d_isValid; }
            set { d_isValid = value; }
        }

        public string BookType
        {
            get { return d_bookType; }
            set { d_bookType = value; }
        }


        public int BookSize
        {
            get { return d_entries.Count; }
        }
        #endregion "Properties"

        #region "Constructors"
        /// <summary>
        /// Name		: ByLevelBook default constructor
        /// Description	: Default constructor. Window Size is set to 0
        /// </summary>
        public ByLevelBook()
        {
            WindowSize = 0;
            IsValid = false;
            BookType = string.Empty;
            d_entries = new List<BookEntry>();
        }
        #endregion "Constructors"

        #region "public functions"

        /// <summary>
        /// Name		: doAdd
        /// Description	: Add an entry to the order book.
        ///               When you add this order in the market depth table,
        ///               you should shift all orders at the market depth position
        ///               in the event and market depth orders or levels inferior to 
        ///               event passed to one position inferior.
        ///               For example, if a new order is added to position one of the 
        ///               market depth table, then the previous order at position one is 
        ///               shifted to position two. The order at position two is shifted to 
        ///               position three and so on until you get to the market depth window size.
        /// </summary>
        /// <param name="pos">position to add the entry</param>
        /// <param name="entry">entry to add</param>
        public void doAdd(int pos, BookEntry entry)
        {
            lock (d_entries)
            {
        	    if (d_entries.Count < pos)
        	    {
        		    // gap detected
        		    System.Console.WriteLine("Gap detected in cache for level " + (pos + 1) + 
        				    ". Cache size is " + d_entries.Count + ".");
        		    return;
        	    }
                d_entries.Insert(pos, entry);
                if (WindowSize < d_entries.Count)
                {
                    //remove entries > window size
                    d_entries.RemoveRange(WindowSize, d_entries.Count - WindowSize);
                }
            }
        }

        /// <summary>
        /// Name		: doClearAll()
        /// Description	: Clears all the orderbook for the specified side
        ///               This market depth table command is issued by Bloomberg
        ///               when market depth recovery is occuring. This table command
        ///               has the same effect on the cache as DELETEALL which 
        ///               means all order or levels should be cleared from the cache.
        /// </summary>
        public void doClearAll()
        {
            lock (d_entries)
            {
                d_entries.Clear();
            }
        }

        /// <summary>
        /// Name		: doDel()
        /// Description	: Delete this event from the market depth cache. 
        ///     		  The delete should occur at the position passed in the 
        ///     		  market depth event. When cached market event at the 
        ///     		  position passed in the delete is removed, all position 
        ///     		  inferior should have their positions shifted by one. 
        ///     		  For example, if position one is deleted from a market 
        ///     		  by order or market by price event, the position two 
        ///     		  becomes one, position three becomes two, etc. 
        /// </summary>
        /// <param name="pos">position to be deleted</param>
        public void doDel(int pos)
        {
            lock (d_entries)
            {
                if (d_entries.Count > pos)
                {
                    d_entries.RemoveAt(pos);
                }
            }
        }

        /// <summary>
        /// Name			: doDelAll()
        /// Description	    : Delete all events from the cache. This is a market depth
        ///                   flush usually passed at the start or end of trading or when
        ///                   a trading halt occurs.
        /// </summary>
        public void doDelAll()
        {
            lock (d_entries)
            {
                d_entries.Clear();
            }
        }

        /// <summary>
        /// Name		: doDelBetter()
        /// Description	: Delete this order and any superior orders. The order id at 
        /// 			  pos + 1 is now the best order. This differs from the EXEC
        /// 			  command in that it delets the current order, where the 
        /// 			  EXEC command modifies the current order.
        /// </summary>
        /// <param name="pos">position to be deleted</param>
        public void doDelBetter(int pos)
        {
            lock (d_entries)
            {
                d_entries.RemoveRange(0, pos + 1);
            }
        }

        /// <summary>
        /// Name		: doDelSide()
        /// Description	: Delete all events on the corresponding side of the depth cache.
        /// </summary>
        public void doDelSide()
        {
            lock (d_entries)
            {
                d_entries.Clear();
            }
        }

        /// <summary>
        /// Name		: doExec
        /// Description	: Trade Execution. Find the corresponding order in the cache
        ///               replace the entry with the new entry and delete orders with
        ///               greater priority
        /// </summary>
        /// <param name="pos">position to be modified</param>
        /// <param name="entry">new book entry</param>
        public void doExec(int pos, ref BookEntry entry)
        {
            lock (d_entries)
            {
                d_entries[pos] = entry;
                if (pos != 0)
                {
                    d_entries.RemoveRange(0, pos);
                }
            }
        }

        /// <summary>
        /// Name		: doMod
        /// Description	: Modify an existing event in the market depth cache. 
        ///               Find the cached market depth entry by the position in 
        ///               new the market depth cache and replace the cached event
        ///               by the fields and data in the new event.
        /// </summary>
        /// <param name="pos">position to be modified</param>
        /// <param name="entry">new book entry</param>
        public void doMod(int pos, ref BookEntry entry)
        {
            lock (d_entries)
            {
                d_entries[pos] = entry;
            }
        }

        /// <summary>
        ///  Name			: doReplace
        ///  Description	: Replace previous price level or order at this position.
        ///                   Add price level or order if you do not have it currently in
        ///                   the cache. A 0 price and size will be sent when there is
        ///                   no active price or order at this level.
        /// </summary>
        /// <param name="pos">position to be modified</param>
        /// <param name="entry">new book entry</param>
        public void doReplace(int pos, ref BookEntry entry)
        {
            lock (d_entries)
            {
                if (d_entries.Count <= pos)
                {
                    // fill in entries
                    for (int i = d_entries.Count; i <= pos; ++i)
                    {
                        d_entries.Add(new BookEntry());
                    }
                }
                d_entries[pos] = entry;
            }
        }

        /// <summary>
        /// Name		: doReplaceClear
        /// Description	: The REPLACE_CLEAR table command is intended to remove an order or
        ///               more often a level in the market depth cache. The REPLACE_CLEAR
        ///               should be indexed by the MarketDepth.ByLevel/ByOrder.Bid/Ask.Position
        ///               field. The cache should NOT be shifted up after the level is
        ///               cleared. A clear means all orders at that position have been deleted
        ///               from the orderbook. It is possible that a order or level at a
        ///               superior or most superior position to be cleared prior to more
        ///               inferior levels. After the level is cleared in this case, it is
        ///               expected that subsequent market depth event(s) will be passed
        ///               to clear the orders or levels at positions inferior to the one just
        ///               cleared.
        /// </summary>
        /// <param name="pos">position to be modified</param>
        public void doReplaceClear(int pos)
        {
            lock (d_entries)
            {
                if (d_entries.Count <= pos)
                {
                    // fill in entries
                    for (int i = d_entries.Count; i <= pos; ++i)
                    {
                        d_entries.Add(new BookEntry());
                    }
                }
                else
                {
                    BookEntry entry = new BookEntry();
                    d_entries[pos] = entry;
                }
            }
        }

        /// <summary>
        /// Name		: getEtnry
        /// Description	: Returns the entry at the specified position
        /// </summary>
        /// <param name="pos">position of the entry</param>
        /// <returns>true is the cache is valid, false if the cache is invalid</returns>
        public BookEntry getEntry(int pos)
        {
            BookEntry entry = null;
            lock (d_entries)
            {
                if (pos >= d_entries.Count)
                {
                    entry = null;
                }
                else
                {
                    entry = d_entries[pos];
                    if (!entry.IsValid)
                    {
                        // invalid entry
                        entry = null;
                    }
                }
            }
            return entry;
        }

        #endregion "public functions"
    }
}
