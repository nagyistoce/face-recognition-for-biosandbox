#include "stdafx.h"
#include "TrainDbXml.h"
#include "Person.h"
#include "SplitString.h"

using namespace std;
using namespace pugi;

TrainDbXml::TrainDbXml(string fileName, deque<Person>& persones) : XmlParsing(fileName)
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
		//wcout << persones[i].Name << endl;
		//wcout << persones[i].OpencvMatrixId.c_str() << endl;

		string rawTrainVector = this->xmlDocument.child("opencv_storage").child(persones[i].OpencvMatrixId.c_str()).child("data").text().as_string();

		//wcout << rawTrainVector.c_str() << endl;
		SplitString str(rawTrainVector);
		// split retazca trenovacieho vektora na retazce hodnot
		vector<string> sValues = str.split(' ', 1);
		
		// kontrola ci sa nacital spravny pocet poloziek vektoru
		// pocet poloziek trenovacieho vektora = rows * cols
		int rows = this->xmlDocument.child("opencv_storage").child(persones[i].OpencvMatrixId.c_str()).child("rows").text().as_int();
		if (rows <= 0)
		{
			this->ErrorMsg("[TrainDbXml::TrainDbXml(string fileName, deque<Person> persones)]: Error while parsing dbuploader.xml");
		}

		int cols = this->xmlDocument.child("opencv_storage").child(persones[i].OpencvMatrixId.c_str()).child("cols").text().as_int();
		if (cols <= 0)
		{
			this->ErrorMsg("[TrainDbXml::TrainDbXml(string fileName, deque<Person> persones)]: Error while parsing dbuploader.xml");
		}

		if (sValues.size() != rows * cols)
		{
			this->ErrorMsg("[TrainDbXml::TrainDbXml(string fileName, deque<Person> persones)]: Error while parsing dbuploader.xml");
		}	
		
		// prevod retazcov cisel na pole hodnot typu double

		for(int j = 0; j < sValues.size(); j++)
		{
			persones[i].OpencvMatrix.push_back(strtod(sValues[j].c_str(), 0));
		}
	}
}