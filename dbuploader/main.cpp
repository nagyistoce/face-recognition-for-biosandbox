// dbuploader.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include "DbUploaderXml.h"
#include "TrainDbXml.h"
#include <io.h>
#include <fcntl.h>

using namespace std;

void pause()
{
	cout << "Press any key to exit." << endl;
	std::wcin.sync(); // Flush The Input Buffer Just In Case
	std::wcin.ignore(); // There's No Need To Actually Store The Users Input
}

int wmain(int argc, _TCHAR* argv[])
{
	// koli wstring, nastavi na UTF-8 aby zobrazoval v konzole diakritiku, ak bude robit problemy
	// alebo uz nebude treba zobrazovat v konzole diakritiku sa moze neskor odstranit
	_setmode(_fileno(stdout), _O_U8TEXT);
	
	deque<Person> persones;
	
	DbUploaderXml config("dbuploader.xml", persones);
	TrainDbXml trainDb(config.GetTrainFile(), persones);

	// vypis do konzoly osob a char. vektora pre osobu
	for(int i = 0; i < persones.size(); i++)
	{
		persones[i].Print();
	}

	// naspet nastavi encodovanie z UTF-8 na obycajne, ake presne neviem
	_setmode(_fileno(stdout), _O_TEXT);
	atexit(pause);
	
	return 0;
}

