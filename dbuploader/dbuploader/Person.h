//
// Person
// ======
//
// Trieda uchovavajuca udaje o osobe ziskane zo suboru dbuploader.xml

#ifndef _PERSON_CLASS____
#define _PERSON_CLASS____

#include "stdafx.h"

using namespace std;

class Person
{
public:
	Person(wstring name, string id);
	wstring Name;
	string OpencvMetrixId;
};

#endif