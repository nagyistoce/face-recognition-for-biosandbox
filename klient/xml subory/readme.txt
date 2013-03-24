s tymito subormi pracuje klient alebo server
nachadzaju sa v adresari ktory je specifikovany premennou prostredia BIOSANDBOX_HOME

persones.xml - obsahuje udaj o osobe, ktorej trenovaci vektor sa odosiela do databazy
db.xml - vytiahne trenovaci vektor podla udaju z persones.xml
z tychto dvoch xml suborov si klient dynamicky vytvori nove xml, v ktorom su vsetky udaje co sa uploaduju do databazy

test.xml - obsahuje testovaci vektor ktory sa porovna s databazou, server vrati vysledok