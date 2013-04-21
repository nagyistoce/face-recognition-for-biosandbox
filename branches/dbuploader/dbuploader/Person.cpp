#include "stdafx.h"
#include "Person.h"

using namespace std;

Person::Person(wstring name, string id)
{
	this->Name = name;
	this->OpencvMatrixId = id;
}

void Person::Print()
{
	wcout << this->Name << endl;
	wcout << this->OpencvMatrixId.c_str() << endl;

	for (int i = 0; i < this->OpencvMatrix.size(); i++)
	{
		wcout << this->OpencvMatrix[i];
		wcout << "  ";
	}
}