#include "stdafx.h"
#include "SplitString.h"

// split: receives a char delimiter; returns a vector of strings
// By default ignores repeated delimiters, unless argument rep == 1.
// noempty ak je na 1 tak nevrati ziaden prazdny retazec
vector<string>& SplitString::split(char delim, int noempty, int rep) 
{
	if (!flds.empty()) 
		flds.clear();  // empty vector if necessary

	string work = data();
	string buf = "";

	int i = 0;
	while (i < work.length()) 
	{
		if (work[i] != delim)
			buf += work[i];
		else if (rep == 1) 
		{
			if (noempty && (buf.empty() || char(buf[0]) < 32))
				buf = "";
			else 
			{
				flds.push_back(buf);
				buf = "";
			}
		} 
		else if (buf.length() > 0) 
		{
			// pridal som aby mi neprodukoval prazdne vysledky
			if (noempty && (buf.empty() || char(buf[0]) < 32))
				buf = "";
			else 
			{
				flds.push_back(buf);
				buf = "";
			}
		}
		i++;
	}

	if (!buf.empty())
		flds.push_back(buf);

	return flds;
}