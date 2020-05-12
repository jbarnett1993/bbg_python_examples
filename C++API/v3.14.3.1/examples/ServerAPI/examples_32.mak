BIN =   BulkRefDataExample.exe \
		ConnectionAndAuthExample.exe \
		CorrelationExample.exe \
		EqsDataExample.exe \
        EntitlementsVerificationExample.exe \
        EntitlementsVerificationSubscriptionExample.exe \
        HistoryExample.exe \
        IntradayBarExample.exe \
        IntradayTickExample.exe \
		MktBarSubscriptionWithEvents.exe \
		PageDataExample.exe \
        RefDataExample.exe \
		RefDataTableOverrideExample.exe \
		RequestServiceExample.exe \
        SecurityLookupExample.exe \
		SecurityEntitlementsExample.exe \
        SimpleBlockingRequestExample.exe \
        SimpleCategorizedFieldSearchExample.exe \
		SimpleCustomVWAPExample.exe \
        SimpleFieldInfoExample.exe \
        SimpleFieldSearchExample.exe \
        SimpleHistoryExample.exe \
        SimpleIntradayBarExample.exe \
        SimpleIntradayTickExample.exe \
        SimpleRefDataExample.exe \
        SimpleRefDataOverrideExample.exe \
        SimpleSubscriptionExample.exe \
        SimpleSubscriptionIntervalExample.exe \
		SnapshotRequestTemplateExample.exe \
        SubscriptionCorrelationExample.exe \
        SubscriptionWithEventHandlerExample.exe \
		TechnicalAnalysisHistoricalStudyExample.exe \
		TechnicalAnalysisIntradayStudyExample.exe \
		TechnicalAnalysisRealtimeStudyExample.exe \
        UserModeExample.exe 

LFLAGS = /EHsc /O2 /D WIN32 /I..\..\include
CPPFLAGS = $(LFLAGS) ws2_32.lib ..\..\lib\blpapi3_32.lib

all: $(BIN)

clean:
	-@erase *.obj $(BIN)
