#include "stdafx.h"
#include "Person.h"

using namespace std;

Person::Person(wstring name, string id)
{
	this->Name = name;
	this->OpencvMetrixId = id;
}