/* --------------------------------------------------------------------
 *                          DISCLAIMER OF WARRANTY
 *
 * THE CODE SAMPLES PROVIDED IN THIS SDK (IN SOURCE CODE AND BINARY FORM) ARE
 * PROVIDED "AS IS," WITHOUT A WARRANTY OF ANY KIND. ALL EXPRESS OR IMPLIED
 * CONDITIONS, REPRESENTATIONS AND WARRANTIES, INCLUDING ANY IMPLIED WARRANTY
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE OR NON-INFRINGEMENT,
 * ARE HEREBY EXCLUDED. BLOOMBERG AND ITS LICENSORS SHALL NOT BE LIABLE FOR
 * ANY DAMAGES SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING OR
 * DISTRIBUTING THE SOFTWARE OR ITS DERIVATIVES. IN NO EVENT WILL BLOOMBERG
 * OR ITS LICENSORS BE LIABLE FOR ANY LOST REVENUE, PROFIT OR DATA, OR FOR
 * DIRECT, INDIRECT, SPECIAL, CONSEQUENTIAL, INCIDENTAL OR PUNITIVE DAMAGES,
 * HOWEVER CAUSED AND REGARDLESS OF THE THEORY OF LIABILITY, ARISING OUT OF
 * THE USE OF OR INABILITY TO USE SOFTWARE, EVEN IF BLOOMBERG HAS BEEN ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGES.
 * THE CODE SAMPLES PROVIDED WITH THE B-PIPE SDK ARE NOT INTENDED FOR 
 * PRODUCTION USE, AND IF PUT TO SUCH USE THEY MAY RESULT IN SLOWNESS OR LOSS
 * OF DATA.
 * ----------------------------------------------------------------- */

/* --------------------------------------------------------------------
 * File: SyncIO.h
 *
 * Description: This source code defines the SyncIO class to control 
 *				acees to resources 
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

#ifndef _SYNCIO_H_
#define  _SYNCIO_H_
/* --------------------------------------------------------------------
 * Class/Struct : SyncIO
 * Description  : Class to control access to resource
 *				  Windows uses CRITICAL_SECTION
 *				  Unix uses pthread_mutex_t  for locking the resource
 * --------------------------------------------------------------------*/

#if defined(WIN32) || defined(_WIN32)
#include <windows.h>

class SyncIO
{
    public:
		/*-------------------------------------------------
		 * Name			: SyncIO
		 * Description	: Default constructor
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        SyncIO()
        {
            InitializeCriticalSection(&m_cs);
        }

		/*-------------------------------------------------
		 * Name			: SyncIO
		 * Description	: Destructor
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        ~SyncIO()
        {
            DeleteCriticalSection(&m_cs);
        }

		/*-------------------------------------------------
		 * Name			: lock
		 * Description	: Method to lock access to resource
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        void lock(void)
        {
            EnterCriticalSection(&m_cs);
        }

		/*-------------------------------------------------
		 * Name			: unlock
		 * Description	: Methos to unlock access to resource
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        void unlock(void)
        {
            LeaveCriticalSection(&m_cs);
        }

    protected:
        CRITICAL_SECTION m_cs;

    private:
        // Unimplemented
        SyncIO(const SyncIO&);
        SyncIO& operator=(const SyncIO&);
};
#else
#include <unistd.h>                               // sleep
#include <pthread.h>                              // should be present on a unix systems supported

class SyncIO
{
    public:
		/*-------------------------------------------------
		 * Name			: SyncIO
		 * Description	: Default constructor
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        SyncIO():lockCount_(0)
        {
            pthread_mutexattr_init(&mutexAttribute_);
            pthread_mutexattr_setpshared(&mutexAttribute_, PTHREAD_PROCESS_PRIVATE);
            pthread_mutexattr_settype(&mutexAttribute_, PTHREAD_MUTEX_RECURSIVE);
            pthread_mutex_init(&mutex_, &mutexAttribute_);
        }

        /*-------------------------------------------------
		 * Name			: ~SyncIO
		 * Description	: Destructor
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        virtual ~SyncIO() throw()
        {
            while (lockCount_)
                unlock();
            pthread_mutex_destroy(&mutex_);
            pthread_mutexattr_destroy(&mutexAttribute_);

        }

 		/*-------------------------------------------------
		 * Name			: lock
		 * Description	: Method to lock access to resource
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        void lock(void)
        {
            pthread_mutex_lock(&mutex_);          // get mutex
            ++lockCount_;                         // increment lock count
        }

  		/*-------------------------------------------------
		 * Name			: unlock
		 * Description	: Methos to unlock access to resource
		 * Arguments	: none
		 * Returns		: none
		 *-------------------------------------------------*/
        void unlock(void)
        {
            if (lockCount_)                       // something locked?
            {
                --lockCount_;                     // decrement lock count
                pthread_mutex_unlock(&mutex_);    // release mutex
            }
        }

    protected:
        unsigned long          lockCount_;
        pthread_mutexattr_t    mutexAttribute_;
        pthread_mutex_t        mutex_;

    private:
        // Unimplemented
        SyncIO(const SyncIO&);
        SyncIO& operator=(const SyncIO&);

};
#endif


/* --------------------------------------------------------------------
 * Class/Struct : SyncIO
 * Description  : Class wrapper around SyncIO calls to automatically
 *				  unlock a SyncIO object once it gets out of scope
 * --------------------------------------------------------------------*/
class Guard
{
public:

	Guard(SyncIO& mutex) : mutex_(mutex)
	{
		mutex_.lock();
	}

	virtual ~Guard() throw ()
	{
		mutex_.unlock();
	}

private:
	SyncIO& mutex_;

};
#endif
