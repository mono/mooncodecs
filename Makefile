all:
	(cd csvorbis; make -f Makefile.moon) || exit
	(cd csadpcm; make) || exit
	(cd csdirac; make) || exit

clean:
	(cd csvorbis; make -f Makefile.moon clean) || exit
	(cd csadpcm; make clean) || exit
	(cd csdirac; make clean) || exit
