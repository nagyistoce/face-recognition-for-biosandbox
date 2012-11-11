#include "stdafx.h"
#include "TrainDbXml.h"
#include "Person.h"

using namespace std;
using namespace pugi;

TrainDbXml::TrainDbXml(string fileName, deque<Person> persones) : XmlParsing(fileName)
{
	// najprv sa overi ci pocet osob sedi s poctom trenovacich vektorov
	int trainVectors = this->xmlDocument.child("opencv_storage").child("trainPersonNumMat").child("cols").text().as_int();

	if (trainVectors <= 0)
	{
		this->ErrorMsg("[TrainDbXml::TrainDbXml(string fileName, deque<Person> persones)]: Error while parsing dbuploader.xml");
	}
	if (trainVectors != persones.size())
	{
		this->ErrorMsg("[TrainDbXml::TrainDbXml(string fileName, deque<Person> persones)]: Number of persons in dbuploader.xml does not match number of vectors in " + fileName);
	}

	// vypisanie jednotlivych trenovacich vektorov k osobam ku ktorym patria podla definicie osob v dbuploader.xml
	for (int i = 0; i < persones.size(); i++)
	{
		wcout << persones[i].Name << endl;
		wcout << persones[i].OpencvMetrixId.c_str() << endl;

		string rawTrainVector = this->xmlDocument.child("opencv_storage").child(persones[i].OpencvMetrixId.c_str()).child("data").text().as_string();

		wcout << rawTrainVector.c_str() << endl;
	}
}