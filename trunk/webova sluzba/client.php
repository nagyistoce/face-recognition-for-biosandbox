<?php
// Pull in the NuSOAP code
require_once('../nusoap/lib/nusoap.php');
// Create the client instance
$client = new nusoap_client('http://localhost/WebSluzbaUpload/server.php');
// Call the SOAP method
$xml = file_get_contents('new.xml');
//$xml = 'new.xml';
$result = $client->call('uploadAndTest', array('xmlfile' => $xml));
// Display the result
print_r($result);
// Display the request and response
echo '<h2>Request</h2>';
echo '<pre>' . htmlspecialchars($client->request, ENT_QUOTES) . '</pre>';
echo '<h2>Response</h2>';
echo '<pre>' . htmlspecialchars($client->response, ENT_QUOTES) . '</pre>';
//$result = $client->call('connectAndTest', array('name' => 'Martin'));

?>