
MOON_VORBIS_SRC = \
	MoonVorbis/OggMediaStreamSource.cs \
	MoonVorbis/WaveFormatExtensible.cs \
	MoonVorbis/StringExtensions.cs

MOON_VORBIS_TEST_SRC = \
	MoonVorbisTest/App.xaml \
	MoonVorbisTest/App.xaml.cs \
	MoonVorbisTest/Page.xaml \
	MoonVorbisTest/Page.xaml.cs

MOON_VORBIS_LIBS = build/csvorbis.dll build/csogg.dll

all: build/MoonVorbisTest.xap

build/MoonVorbisTest.xap: build/csogg.dll build/csvorbis.dll build/MoonVorbis.dll $(MOON_VORBIS_TEST_SRC)
	cp MoonVorbisTest/*.xaml build
	cp MoonVorbisTest/*.xaml.cs build
	cp MoonVorbisTest/Properties/AppManifest.xml build/AppManifest.xaml
	cd build; mxap --application-name=MoonVorbisTest; cd ..

build/MoonVorbis.dll: $(MOON_VORBIS_LIBS) $(MOON_VORBIS_SRC)
	smcs -t:library -out:MoonVorbis.dll -debug -r:build/csvorbis.dll -r:build/csogg.dll $(MOON_VORBIS_SRC)
	mv MoonVorbis.dll build
	mv MoonVorbis.dll.mdb build

build/csvorbis.dll: build/csogg.dll
	smcs -out:build/csvorbis.dll -t:library csvorbis/*.cs -r:build/csogg.dll -d:NET_2_1

build/csogg.dll:
	smcs -out:build/csogg.dll -t:library csogg/*.cs -d:NET_2_1

clean:
	rm build/*
