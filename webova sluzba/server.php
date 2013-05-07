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

$server->register('udfRecognitionTest2',                // method name
    array('xmlfile' => 'xsd:string'),        // input parameters
    array('return' => 'xsd:string'),      // output parameters
    'urn:recognitionwsdl',                      // namespace
    'urn:recognitionwsdl#udfRecognitionTest2',                // soapaction
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

function udfRecognitionTest2($xmlfile){

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
  
  $maxDistance = 100;
  $minPersonId = -1;
	
	
  $vectors = $parser->getElementsByTagName("Vector");
  
  foreach($vectors as $vector){

    $vectorData= $vector->nodeValue;
	
	//vnoreny foreach - pre vsetky vektory v databaze a pre vsetky vectory v xml testuj vzdialenost
	  $result = dibi::query('call face_recognition.FindPerson(%s)', $vectorData);

	  $persons_id = array();
      $podobnost = array();
    
      foreach ($result as $n => $row){
	     array_push($persons_id, $row['id_person_res']);
         array_push($podobnost, $row['min_dist']);
      }
	
	  if(empty($podobnost)){
	    return "Chyba";
	  }
	
	  //porovnanie s minimalnou vzdialenostou a osetrenie, ci nenastala chyba
	  if(($podobnost[0]>$maxDistance) || ($podobnost[0]==-1)){
		return "Nenasla sa zhoda!";
	  }
	
	$result->free();
	unset($result);
	dibi::disconnect();
	dibi::connect(array(
      'driver'   => 'mysql',
      'host'     => $dbserver,
      'username' => $dbusername,
      'password' => $dbpassword,
      'database' => $dbid,
      'charset'  => 'utf8',
    )); 
	//selectne meno osoby , ktora vlastni vector s minimalnou vzdialenostou
	$result = dibi::query('SELECT name FROM face_recognition.person WHERE idperson=%i', $persons_id[0] );

    $menoRozpoznanejOsoby=array();
    
      foreach ($result as $n => $row){
         array_push($menoRozpoznanejOsoby, $row['name']);
      }
	  
	return "OK! value=$podobnost[0] idPerson=$persons_id[0] meno=$menoRozpoznanejOsoby[0]";
  }
	return "Chyba parsovania";
}

function plainRecognitionTest($xmlfile){
  return "Ok!";
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

  $osData = array();
  
  $persons = $parser->getElementsByTagName("Person");
  //return $persons->length;
  foreach($persons as $person){
    $names	= $person->getElementsByTagName("Name");
    $attr= $names->item(0)->attributes;
	$i = $attr->getNamedItem("value");
	$meno = $i->nodeValue;
	$datas= $person->getElementsByTagName("Datas");
	$dataAttr = $datas->item(0)->attributes;
	$i2 = $dataAttr->getNamedItem("size");
	$dataSize = $i2->nodeValue;
	$data = $person->getElementsByTagName("Data");
	//$osData = $data->item(0)->nodeValue;
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
    
	for($i=0;$i<$dataSize;$i++){
	  //$data->nodeValue;
	  $arr = array(
        'vector' => $data->item($i)->nodeValue,
	    'idperson' => $osid[0],
      );
	  dibi::query('INSERT INTO  vector', $arr);
	}

  }
  
  $result = $dataSize;
  return "ok, pocet vektorov: $result";
  
}

// Use the request to (try to) invoke the service
$HTTP_RAW_POST_DATA = isset($HTTP_RAW_POST_DATA) ? $HTTP_RAW_POST_DATA : '';
$server->service($HTTP_RAW_POST_DATA);
?>