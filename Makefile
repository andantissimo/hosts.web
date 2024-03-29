PREFIX        ?= /usr/local
SBINDIR       ?= $(PREFIX)/sbin
SYSCONFDIR    ?= $(PREFIX)/etc
SERVICE_USER  ?= $(shell id -un $(shell ps -p `pgrep dnsmasq || echo 1` -o uid=))
SERVICE_PORT  ?= 5053
CONFIGURATION ?= Release
RID           ?= linux-x64

PROJECT          := $(shell perl -ne '/"([^"]+\.csproj)"/ && print $$1 =~ s:\\:/:r' *.sln)
PROJECT_DIR      := $(shell dirname $(PROJECT))
PROJECT_NAME     := $(shell basename $(PROJECT) .csproj)
TARGET_FRAMEWORK := $(shell perl -ne '/<(TargetFramework)>(.*)<\/\1>/ && print $$2' $(PROJECT))
ASSEMBLY_NAME    := $(shell perl -ne '/<(AssemblyName)>(.*)<\/\1>/ && print $$2' $(PROJECT))
ifeq ($(ASSEMBLY_NAME),)
	ASSEMBLY_NAME = $(PROJECT_NAME)
endif

SRCS   := $(shell find $(PROJECT_DIR) -name *.cs -or -name *.html) $(PROJECT)
OUTDIR := $(PROJECT_DIR)/bin/$(CONFIGURATION)/$(TARGET_FRAMEWORK)/$(RID)/publish

all: $(OUTDIR)/$(ASSEMBLY_NAME) $(OUTDIR)/$(ASSEMBLY_NAME).service

install: all
	install -d -m 755 $(SYSCONFDIR)/$(ASSEMBLY_NAME)/wwwroot
	install -m 644 $(OUTDIR)/wwwroot/index.html $(SYSCONFDIR)/$(ASSEMBLY_NAME)/wwwroot/index.html
	install -m 644 $(OUTDIR)/appsettings.json $(SYSCONFDIR)/$(ASSEMBLY_NAME)/appsettings.json
	install -m 755 $(OUTDIR)/$(ASSEMBLY_NAME) $(SBINDIR)/$(ASSEMBLY_NAME)
	install -m 644 $(OUTDIR)/$(ASSEMBLY_NAME).service /etc/systemd/system/$(ASSEMBLY_NAME).service

clean:
	$(RM) -r $(PROJECT_DIR)/bin $(PROJECT_DIR)/obj

$(OUTDIR)/$(ASSEMBLY_NAME): $(SRCS)
	dotnet publish --nologo -c $(CONFIGURATION) -p:PublishSingleFile=true -p:PublishTrimmed=true -r $(RID) --sc

$(OUTDIR)/$(ASSEMBLY_NAME).service: hosts.web.service.in
	cat hosts.web.service.in \
	  | sed -e 's:@ASSEMBLY_NAME@:$(ASSEMBLY_NAME):' \
	        -e 's:@SBINDIR@:$(SBINDIR):' \
	        -e 's:@SYSCONFDIR@:$(SYSCONFDIR):' \
	        -e 's:@USER@:$(SERVICE_USER):' \
	        -e 's:@PORT@:$(SERVICE_PORT):' \
	  > $@
