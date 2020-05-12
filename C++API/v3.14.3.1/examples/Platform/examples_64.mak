BIN =   ContributionsFullPageExample.exe \
        ContributionsMktdataExample.exe \
		ContributionsPageExample.exe \
		LocalMktdataSubscriptionExample.exe \
		LocalPageSubscriptionExample.exe \
		MktdataBroadcastPublisherExample.exe \
		MktdataPublisher.exe \
		ContributionsGDCOSecurityExample.exe \
		PagedataInteractivePublisherExample.exe \
		RequestServiceExample.exe\
		PagedataInteractivePublisherExample.exe \
		PagePublisherExample.exe


LFLAGS = /EHsc /O2 /D WIN32 /I..\..\include
CPPFLAGS = $(LFLAGS) ws2_32.lib ..\..\lib\blpapi3_64.lib

all: $(BIN)

clean:
	-@erase *.obj $(BIN)
