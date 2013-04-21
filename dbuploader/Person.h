//
// Person
// ======
//
// Trieda uchovavajuca udaje o osobe ziskane zo suboru dbuploader.xml
// K udajom o osobe prida trenovaci vektor z vystupu modulu train.bat

#ifndef _PERSON_CLASS____
#define _PERSON_CLASS____

#include "stdafx.h"

using namespace std;

class Person
{
public:
	Person(wstring name, string id);
	wstring Name;
	string OpencvMatrixId;

	// trenovaci vektor (string z xml suboru uz rozparsovany do pola hodnot typu double)
	vector<double> OpencvMatrix;
	
	// vypise udaje na konzolu
	void Print();
};

#endif