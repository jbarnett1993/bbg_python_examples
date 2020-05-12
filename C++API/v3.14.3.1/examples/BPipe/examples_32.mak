BIN =   BulkRefDataExample.exe \
		CorrelationExample.exe \
		PageDataExample.exe \
		MSGScrapeSubscriptionExample.exe \
		ServerMode_EntitlementsVerificationSubscriptionTokenExample.exe \
		ServerMode_EntitlementsVerificationTokenExample.exe \
		ServerMode_GetAuthorizationToken.exe \
        RefDataExample.exe \
        HistoryExample.exe \
        IntradayBarExample.exe \
        IntradayTickExample.exe \
		LocalMktdataSubscriptionExample.exe \
		MarketDepthSubscriptionExample.exe \
		MarketListSnapshotExample.exe \
		MarketListSubscriptionExample.exe \
		MktBarSubscriptionWithEvents.exe \
		RefDataTableOverrideExample.exe \
		SecurityLookupExample.exe \
        SimpleBlockingRequestExample.exe \
        SimpleCategorizedFieldSearchExample.exe \
        SimpleFieldInfoExample.exe \
        SimpleFieldSearchExample.exe \
        SimpleRefDataExample.exe \
        SimpleRefDataOverrideExample.exe \
        SimpleSubscriptionExample.exe \
        SimpleSubscriptionIntervalExample.exe \
		SnapshotRequestTemplateExample.exe \
        SubscriptionCorrelationExample.exe \
        SubscriptionWithEventHandlerExample.exe

LFLAGS = /EHsc /O2 /D WIN32 /I..\..\include
CPPFLAGS = $(LFLAGS) ws2_32.lib ..\..\lib\blpapi3_32.lib

all: $(BIN)

clean:
	-@erase *.obj $(BIN)
