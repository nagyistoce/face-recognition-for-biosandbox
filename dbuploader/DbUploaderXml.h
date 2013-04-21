//
// DbUploaderXml
// =============
//
// Trieda reprezuntujuca xml subor s nastaveniami modulu dbuploader.xml
// Subor sa musi nachadzat v priecinku ktory je definovany v premennej prostredia BIOSANDBOX_HOME
// dbuplader.xml obsahuje nastavenia modulu a zoznam osob ktorych natrenovany vektor sa ma upoadnut do databazy.
//
// Pocet osob musi sediet s poctom vektorov v xml subore ktory generuje modul train

#ifndef _DBUPLOADERXML_CLASS____
#define _DBUPLOADERXML_CLASS____

#include "stdafx.h"
#include "XmlParsing.h"
#include "Person.h"

using namespace std;

class DbUploaderXml : public XmlParsing
{
private:
	string trainFile;
public:
	DbUploaderXml(string fileName, deque<Person>& persones);
	string GetTrainFile();
};

#endif