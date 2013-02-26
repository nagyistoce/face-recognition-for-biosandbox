<?php
// Pull in the NuSOAP code
require_once('../nusoap/lib/nusoap.php');
include_once('config.php');
include_once('dibi.min.php');
header('Content-Type: text/xml; charset=utf8');
// Create the server instance
$server = new soap_server;
$server->xml_encoding = "utf-8";
$server->soap_defencoding = "utf-8";
// Register the method to expose
$server->configureWSDL('recognitionwsdl', 'urn:recognitionwsdl');

$server->register('uploadAndTest',                // method name
    array('xmlfile' => 'xsd:string'),        // input parameters
    array('return' => 'xsd:string'),      // output parameters
    'urn:recognitionwsdl',                      // namespace
    'urn:recognitionwsdl#connectAndTest',                // soapaction
    'rpc',                                // style
    'encoded',                            // use
    'Upload new person and train vectors'            // documentation
);

$server->register('udfRecognitionTest',                // method name
    array('xmlfile' => 'xsd:string'),        // input parameters
    array('return' => 'xsd:string'),      // output parameters
    'urn:recognitionwsdl',                      // namespace
    'urn:recognitionwsdl#udfRecognitionTest',                // soapaction
    'rpc',                                // style
    'encoded',                            // use
    'Test for peson with udf'            // documentation
);

function udfRecognitionTest($xmlfile){

  $dbusername = "tp";
  $dbpassword = "tp2012";
  $dbserver = "localhost";
  $dbid = "face_recognition";
  dibi::connect(array(
      'driver'   => 'mysql',
      'host'     => $dbserver,
      'username' => $dbusername,
      'password' => $dbpassword,
      'database' => $dbid,
      'charset'  => 'utf8',
     )); 

  //Set up the parser object
  $parser = new DOMDocument();
  $parser->loadXML($xmlfile);
  
  $minDistance = 100;
  $minPersonId = -1;
  
  //select vsetkych trenovacich vecktorov
  $result = dibi::query('SELECT * FROM vector');

    $idtvector=array();
	$tvectorValue=array();
	$idperson=array();
    
    foreach ($result as $n => $row){
       array_push($idtvector, $row['idvector']);
	   array_push($tvectorValue, $row['vector']);
	   array_push($idperson, $row['idperson']);
    }
	
	
  $vectors = $parser->getElementsByTagName("Vector");
  //return $persons->length;
  foreach($vectors as $vector){

    $vectorData= $vector->nodeValue;
	$i=0;
	//vnoreny foreach - pre vsetky vektory v databaze a pre vsetky vectory v xml testuj vzdialenost
	foreach($tvectorValue as $tvector){
	  $result = dibi::query('SELECT Distance((%s),(%s)) as podobnost', $vectorData, $tvector);

      $podobnost=array();
    
      foreach ($result as $n => $row){
         array_push($podobnost, $row['podobnost']);
      }
	
	  if(empty($podobnost)){
	    return "error";
	  }
	
	  //porovnanie s minimalnou vzdialenostou a osetrenie, ci nenastala chyba
	  if($podobnost[0]<=$minDistance && $podobnost[0]!=-1){
	    $minDistance = $podobnost[0];
		$minPersonId = $idperson[$i];
	  }
	  
	  //i zvacsujem kvoli polu $idperson
	  $i++;
	}
	
	//selectne meno osoby , ktora vlastni vector s minimalnou vzdialenostou
	$result = dibi::query('SELECT name FROM person WHERE idperson=%i', $minPersonId);

    $menoRozpoznanejOsoby=array();
    
      foreach ($result as $n => $row){
         array_push($menoRozpoznanejOsoby, $row['name']);
      }
	  
	return "OK! value=$minDistance idPerson=$minPersonId meno=$menoRozpoznanejOsoby[0]";
  }
	return "Chyba parsovania";
}

function uploadAndTest($xmlfile){

  $dbusername = "tp";
  $dbpassword = "tp2012";
  $dbserver = "localhost";
  $dbid = "face_recognition";
  dibi::connect(array(
      'driver'   => 'mysql',
      'host'     => $dbserver,
      'username' => $dbusername,
      'password' => $dbpassword,
      'database' => $dbid,
      'charset'  => 'utf8',
     )); 

  //Set up the parser object
  $parser = new DOMDocument();
  $parser->loadXML($xmlfile);

  
  $persons = $parser->getElementsByTagName("Person");
  //return $persons->length;
  foreach($persons as $person){
    $names	= $person->getElementsByTagName("Name");
    $attr= $names->item(0)->attributes;
	$i = $attr->getNamedItem("value");
	$meno = $i->nodeValue;
	$data= $person->getElementsByTagName("Data");
	$osData = $data->item(0)->nodeValue;
	$arr = array(
    'name' => $meno,
    );
	//return "ahoj";
	
	$result = dibi::query('SELECT idperson FROM person WHERE name=%s', $meno);

    $osid=array();
    
    foreach ($result as $n => $row){
       array_push($osid, $row['idperson']);
	   //return $osid[0];
    }
	
	if(empty($osid)){
	  dibi::query('INSERT INTO  person', $arr);
	}
	
	$result = dibi::query('SELECT idperson FROM person WHERE name = %s', $meno);

    $osid=array();
    
    foreach ($result as $n => $row){
       array_push($osid, $row['idperson']);
    }
    
	$arr = array(
      'vector' => $osData,
	  'idperson' => $osid[0],
    );
	dibi::query('INSERT INTO  vector', $arr);
  }
  
  
  return "ok";
  
}

// Use the request to (try to) invoke the service
$HTTP_RAW_POST_DATA = isset($HTTP_RAW_POST_DATA) ? $HTTP_RAW_POST_DATA : '';
$server->service($HTTP_RAW_POST_DATA);
?>