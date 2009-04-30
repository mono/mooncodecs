
MOON_DIRAC_SRC = \
	MoonDirac/DiracStreamSource.cs

MOON_DIRAC_TEST_SRC = \
	MoonDiracTest/App.xaml \
	MoonDiracTest/App.xaml.cs \
	MoonDiracTest/Page.xaml \
	MoonDiracTest/Page.xaml.cs

MOON_DIRAC_LIBS = build/csdirac.dll

all: build/MoonDiracTest.xap

build/MoonDiracTest.xap: build/csdirac.dll build/MoonDirac.dll $(MOON_DIRAC_TEST_SRC)
	cp MoonDiracTest/*.xaml build
	cp MoonDiracTest/*.xaml.cs build
	cp MoonDiracTest/Properties/AppManifest.xml build/AppManifest.xaml
	cd build; mxap --application-name=MoonDiracTest; cd ..

build/MoonDirac.dll: $(MOON_DIRAC_LIBS) $(MOON_DIRAC_SRC)
	smcs -t:library -out:MoonDirac.dll -debug -r:build/csdirac.dll $(MOON_DIRAC_SRC)
	mv MoonDirac.dll build
	mv MoonDirac.dll.mdb build

build/csdirac.dll:
	smcs -out:build/csdirac.dll -t:library csdirac/*.cs Math/*.cs Properties/*.cs -d:NET_2_1

clean:
	rm build/*
