//
// TrainDbXml
// ==========
//
// Trieda reprezuntujuca xml subor s trenovacimi vzorkami, ktory generuje modul train.bat
//
// Pocet osob musi sediet s poctom vektorov v xml subore ktory generuje modul train

#ifndef _TRAINDBXML_CLASS____
#define _TRAINDBXML_CLASS____

#include "stdafx.h"
#include "XmlParsing.h"
#include "Person.h"

using namespace std;

class TrainDbXml : public XmlParsing
{
private:

public:
	TrainDbXml(string fileName, deque<Person> persones);
};

#endif