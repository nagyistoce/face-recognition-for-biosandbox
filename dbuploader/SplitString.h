// Class SplitString which adds method split()

// define MAIN if this is a standalone program
#ifndef _SPLITSTRING_CLASS____
#define _SPLITSTRING_CLASS____

#include <string>
#include <vector>
#include <iostream>

using namespace std;


class SplitString : public string {
private:
	vector<string> flds;
public:
	SplitString(char *s) : string(s) { };
	SplitString(string s) : string(s) { };
	vector<string>& split(char delim, int noempty = 0, int rep = 0);
};

#endif