#include "stdafx.h"
#include "DbUploaderXml.h"
#include "Person.h"

using namespace std;
using namespace pugi;

DbUploaderXml::DbUploaderXml(string fileName) : XmlParsing(fileName)
{
	// ziskanie nazvu suboru s train databazou
	this->trainFile = this->xmlDocument.child("dbuploader").child("Configuration").child("Input").child("Train").attribute("file").value();

	if (trainFile.empty())
	{
		this->ErrorMsg("[DbUploaderXml::DbUploaderXml(string fileName)]: Error while parsing dbuploader.xml");
	}
	
	// naplnenie dynamickeho pola persons
	xml_node upload = this->xmlDocument.child("dbuploader").child("Upload");
	for (xml_node person = upload.child("Person"); person; person = person.next_sibling("Person"))
	{
		
		wstring wname = as_wide(person.child("Name").attribute("value").value());
		string id = person.child("opencv-matrix").attribute("id").value();

		if (wname.empty() || id.empty())
		{
			this->ErrorMsg("[DbUploaderXml::DbUploaderXml(string fileName)]: Error while parsing dbuploader.xml");
		}

		Person *p = new Person(wname, id);
		this->persones.push_back(*p);
		delete p;

		//std::wcout << "Name: " << wname << endl;
		//std::wcout << "opencv-matrix: " << id.c_str() << endl;
	}
}





// getre

string DbUploaderXml::GetTrainFile()
{
	return this->trainFile;
}

deque<Person> DbUploaderXml::GetPersons()
{
	return this->persones;
}