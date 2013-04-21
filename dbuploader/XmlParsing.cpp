#include "stdafx.h"
#include "XmlParsing.h"

using namespace std;
using namespace pugi;

XmlParsing::XmlParsing(string fileName)
{
	string path = GetBiosandboxHome() + "/" + fileName;

	xml_parse_result result = this->xmlDocument.load_file(path.c_str());
	
	if (!result)
	{
		string msg = result.description();
		ErrorMsg("[XmlParsing::XmlParsing(string fileName)]: " + msg);
	}
}

void XmlParsing::ErrorMsg(string msg)
{
	wcerr << msg.c_str() << endl;
	exit(-1);
}

string XmlParsing::GetBiosandboxHome()
{
	char * pPath;
	pPath = getenv ("BIOSANDBOX_HOME");

	if (pPath == NULL) 
	{
		ErrorMsg("[string GetBiosandboxHome()]: Enviroment variable BIOSANDBOX_HOME not found.");
	}
	else
		return pPath;
}