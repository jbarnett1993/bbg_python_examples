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

namespace Bloomberglp.Blpapi.Examples
{
    class BookEntry
    {
        private string d_broker;        //the broker code [Order Book only]
        private double d_price;         //price of the bid/ask
        private string d_time;          //tick time
        private int d_numberOrders;     //number of orders
        private int d_size;             //order size
        private bool d_isValid;         //flag to indicate whether a book is valid

        #region "Public Properties"
        /// <summary>
        /// the broker code [Order Book only]
        /// </summary>
        public string Broker
        {
            get { return d_broker; }
            set { d_broker = value; }
        }

        /// <summary>
        /// price of the bid/ask
        /// </summary>
        public double Price
        {
            get { return d_price; }
            set { d_price = value; }
        }

        /// <summary>
        /// tick time
        /// </summary>
        public string Time
        {
            get { return d_time; }
            set { d_time = value; }
        }

        /// <summary>
        /// number of orders
        /// </summary>
        public int NumberOrders
        {
            get { return d_numberOrders; }
            set { d_numberOrders = value; }
        }

        /// <summary>
        /// bid or ask size
        /// </summary>
        public int Size
        {
            get { return d_size; }
            set { d_size = value; }
        }

        /// <summary>
        /// indicates whether the entry is valid or not
        /// </summary>
        public bool IsValid
        {
            get { return d_isValid; }
            set { d_isValid = value; }
        }
        #endregion "Public Properties"

        #region "Constructors"
        /// <summary>
        /// Name		: BookEntry default constructor
        /// Description	: Constructs a bylevel book entry with 0 price, size, orders
        /// </summary>
        public BookEntry()
        {
            Broker = string.Empty;
            Price = 0;
            Time = string.Empty;
            NumberOrders = 0;
            Size = 0;
            IsValid = false;
        }

        /// <summary>
        /// Name		: BookEntry constructor
        /// Description	: Constructs a bylevel book entry instance 
        /// </summary>
        /// <param name="price">bid/ask price of the tick</param>
        /// <param name="time">tick time</param>
        /// <param name="numOrders">number of orders</param>
        /// <param name="size">size of bid/ask</param>
        public BookEntry(double price, string time, int numOrders, int size)
        {
            Price = price;
            Time = time;
            NumberOrders = numOrders;
            Size = size;
            IsValid = true;
        }

        public BookEntry(string broker, double price, string time, int numOrders, int size)
        {
            Broker = broker;
            Price = price;
            Time = time;
            NumberOrders = numOrders;
            Size = size;
            IsValid = true;
        }

        /// <summary>
        /// Name		: BookEntry copy constructor
        /// Description	: Copy constructor for ByLevelBookEntry
        /// </summary>
        /// <param name="copy">BookEntry to be copied</param>
        public BookEntry(BookEntry copy)
        {
            Price = copy.Price;
            Time = copy.Time;
            NumberOrders = copy.NumberOrders;
            Size = copy.Size;
            IsValid = copy.IsValid;
        }
        #endregion "Constructors"
    }
}
