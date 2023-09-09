# OpcUaClient
Read and Write data with opc ua

using OpcTools;


string IpAddress = "192.168.1.51";          // OPC Server IP Address
int Port = 48010;                           // OPC Server Port
string SessionName = "Session Name";        // Session Name
string DefaultPrefix = "ns=2;s=Tags./";     // Item ID Prefix
int Timeout = 1000;                         // Timeout

var Client = new OpcClient(IpAddress, Port, DefaultPrefix, SessionName, Timeout);


//READ SINGLE VALUE

string ValueToRead = "NodeName1";
Single value;
value = Client.ReadValue<Single>(ValueToRead);

//custom prefix
value = Client.ReadValue<Single>(ValueToRead, "ns=2;s=Tags./");


//READ MULTIPLE VALUE
string[] ValuesToRead = new string[]
{
    "NodeName1",
    "NodeName2",
    "NodeName3",
};

IEnumerable<Single>? values;
values = Client.ReadValues<Single>(ValuesToRead);


//WRITE VALUES

OpcValue[] ValuesToWrite = new OpcValue[]
{
    new OpcValue("NodeName1",150),
    new OpcValue("NodeName2",234)
};

Client.WriteValues(ValuesToWrite);





