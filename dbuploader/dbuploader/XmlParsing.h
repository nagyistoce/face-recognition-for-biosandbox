//
// XmlParsing
// ==========
//
// Spolocna trieda pre vsetky triedy, ktore reprezentuju XML subory.
// Momentalne DbUploaderXml a TrainDbXml

#ifndef _XMLPARSING_CLASS____
#define _XMLPARSING_CLASS____

#include "stdafx.h"

using namespace std;
using namespace pugi;

class XmlParsing
{
protected:

	// pugi xmldocument
	xml_document xmlDocument;

	XmlParsing(string fileName);
	// vypise chybovu hlasku a ukonci modul
	void ErrorMsg(string msg);
	// vrati obsah premennej prostredia BIOSANDBOX_HOME 
	string GetBiosandboxHome();
};

#endif