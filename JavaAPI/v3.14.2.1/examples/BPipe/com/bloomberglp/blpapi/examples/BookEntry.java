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

public class BookEntry {
	// properties
    private String d_broker;        //the broker code [Order Book only]
    private double d_price;         //price of the bid/ask
    private String d_time;          //tick time
    private int d_numberOrders;     //number of orders
    private int d_size;             //order size
    private boolean d_isValid;      //flag to indicate whether a book is valid

    // public methods
    public String getBroker()
    {
    	return d_broker;
    }
    
    public void setBroker(String broker)
    {
    	d_broker = broker;
    }
    
    public double getPrice()
    {
    	return d_price;
    }
    
    public void setPrice(double price)
    {
    	d_price = price;
    }
    
    public String getTime()
    {
    	return d_time;
    }
    
    public void setTime(String time)
    {
    	d_time = time;
    }
    
    public int getNumberOrders()
    {
    	return d_numberOrders;
    }
    
    public void setNumberOrders(int numberOrders)
    {
    	d_numberOrders = numberOrders; 
    }
    
    public int getSize()
    {
    	return d_size;
    }
    
    public void setSize(int size)
    {
    	d_size = size;
    }
    
    public boolean getIsValid()
    {
    	return d_isValid;
    }
    
    public void setIsValid(boolean isValid)
    {
    	d_isValid = isValid;
    }
    
    /// <summary>
    /// Name		: ByLevelBookEntry default constructor
    /// Description	: Constructs a bylevel book entry with 0 price, size, orders
    /// </summary>
    public BookEntry()
    {
    	setBroker("");
        setPrice(0);
        setTime("");
        setNumberOrders(0);
        setSize(0);
        setIsValid(false);
    }

    // *****************************************************
    // Name		: BookEntry constructor
    // Description	: Constructs a bylevel book entry instance 
    // Parameters :
    // price	  : bid/ask price of the tick
    // time		  : tick time
    // numOrders  : number of orders
    // size		  : order size
    // *****************************************************
    public BookEntry(double price, String time, int numOrders, int size)
    {
        setPrice(price);
        setTime(time);
        setNumberOrders(numOrders);
        setSize(size);
        setIsValid(true);
    }

    // *****************************************************
    // Name		: BookEntry constructor
    // Description	: Constructs a bylevel book entry instance 
    // Parameters :
    // price	  : bid/ask price of the tick
    // time		  : tick time
    // numOrders  : number of orders
    // size		  : order size
    // *****************************************************
    public BookEntry(String broker, double price, String time, int numOrders, int size)
    {
        setBroker(broker);
        setPrice(price);
        setTime(time);
        setNumberOrders(numOrders);
        setSize(size);
        setIsValid(true);
    }

    // *****************************************************
    // Name		: BookEntry copy constructor
    // Description	: Copy constructor for ByLevelBookEntry
    // Parameters :
    // copy 	: BookEntry to be copied
    // *****************************************************
    public BookEntry(BookEntry copy)
    {
        setPrice(copy.getPrice());
        setTime(copy.getTime());
        setNumberOrders(copy.getNumberOrders());
        setSize(copy.getSize());
        setIsValid(copy.getIsValid());
    }
}
