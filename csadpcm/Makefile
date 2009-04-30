
ADPCM_SRC = \
	csadpcm/ImaAdpcm.cs \
	MoonAdpcm/AdpcmMediaStreamSource.cs \
	MoonAdpcm/WaveFormatExtensible.cs \
	MoonAdpcm/StringExtensions.cs

ADPCM_TEST_SRC = \
	MoonAdpcmTest/App.xaml \
	MoonAdpcmTest/App.xaml.cs \
	MoonAdpcmTest/Page.xaml \
	MoonAdpcmTest/Page.xaml.cs

all: build/MoonAdpcmTest.xap

build/MoonAdpcmTest.xap: build/MoonAdpcm.dll $(ADPCM_TEST_SRC)
	cp MoonAdpcmTest/*.xaml build
	cp MoonAdpcmTest/*.xaml.cs build
	cp MoonAdpcmTest/Properties/AppManifest.xml build/AppManifest.xaml
	cd build; mxap --application-name=MoonAdpcmTest; cd ..

build/MoonAdpcm.dll: $(ADPCM_SRC)
	smcs -t:library -out:MoonAdpcm.dll -debug $(ADPCM_SRC)
	mv MoonAdpcm.dll build
	mv MoonAdpcm.dll.mdb build

clean:
	rm build/*
